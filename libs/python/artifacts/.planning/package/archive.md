# [PY_ARTIFACTS_ARCHIVE]

`Archive` is the multi-file container producer over the two container rows — the `py7zr` `SevenZipFile` 7z container and the `stream-zip`/`stream-unzip` bounded-memory ZIP container. It folds a `*payloads` spread into ONE container whose directory recovers the full member row set, never N single-frame bundles, composing the `package/bundle#BUNDLE` vocabulary downward and importing no sibling.

`emit() -> ArtifactWork` carries the producer contract — `key = Bundle.key`, `Admission(keyed=None)`, `receipt.slot == node.key` — and `parents` is the scene-bundling seam: a caller bundling upstream artifacts (scene files, sheet PDFs, transmittal members) passes their content keys, so the plan graph holds a DATA edge (`scene/export` emits files and reaches no compression plane) and an identical member set elides before the container writes. An unencrypted ZIP is byte-reproducible — the fixed bundle `_EPOCH` stamp with `extended_timestamps=False` and the shared `zlib_ng` raw-DEFLATE at the profile `level` — so its content key dedups across runs; the encrypted ZIP is intentionally non-reproducible (fresh AES salt/IV per pack) and the 7z container honestly non-reproducible (`py7zr` stamps an uncontrollable wall-clock `creationtime` per entry).

## [01]-[INDEX]

- [02]-[ARCHIVE]: the `Archive` producer over the `SEVEN_Z`/`ZIP_STREAM` rows — the `emit`/`unpack` node contract, the `PackWorker` port kernels, the streamed `xxh3_128` sinks, and the classified `_zip_drain` reader shared by pack-verify and unpack.

## [02]-[ARCHIVE]

- Owner: `Archive` the one container producer wrapping the `Bundle` carrier — `SevenZKnobs`/`ZipStreamKnobs` are bundle-page vocabulary, so a 7z or ZIP container is one profile row on the one union, never a parallel owner. `Archive.pack`/`Archive.recover` are the `PackWorker` port kernels: public staticmethods, total over the two rows, GIL-releasing in-wheel so both directions offload `Modality.THREAD`.
- Cases: the `SevenZFilter` vocabulary resolves at arm scope to `py7zr.FILTER_*` ids (never a bare ordinal or string-built `getattr`), `SevenZPreset` reaching only the `_PRESETABLE` `lzma`/`lzma2` entries and the AES-256 chain entry appended when `password` is set; the `ZIP_STREAM` arm binds the shared `zlib_ng` SIMD raw-DEFLATE at the profile `level` so one substrate serves the deflate and integrity legs. Encryption is a functional profile row, never a lone bool: the `unpack` inverse gates the WinZip-AES variant (`ae1`/`ae2`) AND key length (`aes128`/`aes192`/`aes256`) as orthogonal axes through `_ZIP_MECHANISM` before decryption, the default `("none", "ae2", "aes256")` refusing legacy `ZIP_CRYPTO`, one `_zip_trust` value serving pack-verify and unpack alike.
- Entry: `_emit` maps `Archive.pack` onto `evidence.receipt(self.bundle.key)` and `unpack` maps `Archive.recover` onto `BundleManifest.of`; recovery buffers no payload — 7z streams each entry through the `_Xxh3Factory` sink under `max_extract_size=_ARCHIVE_CEILING` (a high-ratio container raises `DecompressionBombError`, a traversal name `AbsolutePathError`) and ZIP drains chunks through `_zip_drain`'s rolling `xxh3_128`, `_zip_drain` classifying the typed `stream_unzip` fault subtree most-specific-first into a closed `<zip-unpack:*>` discriminant so a hostile container faults structurally, never on one flattened token.
- Output: `verified` is a GENUINE per-member proof — `stream_zip` inline-CRC-verifies only stored members, so the pack arm round-trips the sealed blob through `_zip_drain` (the drain fires the streamed CRC32/size/HMAC) and counts survivors, the same pattern as the 7z `test()` re-read and the delta apply; `frame_size` reads the container's declared `uncompressed` total (the true reconstructed size, not the redundant `out_bytes` blob length), and a member that fails integrity faults loudly rather than overcounting.
- Packages: `py7zr` (lazy, reifies at arm scope), `stream-zip`/`stream-unzip` (eager — the sentinel families are module vocabulary), `zlib-ng` (lazy — the shared SIMD raw-DEFLATE + `crc32` substrate, composed never re-admitted), `xxhash` (streamed member digests), `expression` (`Map.of_seq` the mechanism row), `msgspec` (`Struct`), runtime `identity`/`faults`/`lanes`/`resilience`, `artifacts.core.plan`/`core.receipt`/`package.bundle`.
- Growth: a new container algorithm is one bundle-page row set plus one arm in each of `pack`/`recover`; a new ZIP method is one `ZipMethod` token plus one `_zip_members` match arm; a new 7z filter is one `SevenZFilter` token plus one arm-scope ident entry; a new decrypt mechanism is one `ZipMechanism` token plus one `_ZIP_MECHANISM` row — container evidence rides the existing `entries`/`verified` slots, zero new verb beside `emit`/`unpack`.
- Boundary: no sibling import, no vocabulary re-own (bundle carries the knobs), no folder-minted limiter or retry caller, no receipt-case widening (the `ArchiveInfo` `method_names`/`solid`/`blocks` facts fold only through `uncompressed`), no wall-clock member stamp on the ZIP arm.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
import os
from collections.abc import Iterable, Iterator
from datetime import datetime
from io import BytesIO
from typing import Final, assert_never

import xxhash
from expression.collections import Map
from msgspec import Struct
from stream_unzip import (
    AE_1,
    AE_2,
    AES_128,
    AES_192,
    AES_256,
    NO_ENCRYPTION,
    ZIP_CRYPTO,
    DataError,
    InvalidOperationError,
    MissingPasswordError,
    PasswordError,
    UncompressError,
    UnsupportedFeatureError,
    stream_unzip,
)
from stream_zip import Method, NO_COMPRESSION_32, NO_COMPRESSION_64, ZIP_32, ZIP_64, ZIP_AUTO, stream_zip

from rasm.runtime.faults import RuntimeRail
from rasm.runtime.identity import ContentKey
from rasm.runtime.lanes import LanePolicy, Modality
from rasm.runtime.resilience import RetryClass

from artifacts.core.plan import Admission, ArtifactWork
from artifacts.core.receipt import ArtifactReceipt
from artifacts.package.bundle import (
    Bundle,
    BundleEvidence,
    BundleManifest,
    CodecProfile,
    CompressionAlgo,
    MemberTriple,
    SevenZFilter,
    SevenZKnobs,
    ZipMechanism,
    ZipStreamKnobs,
)

lazy import py7zr
lazy from zlib_ng import zlib_ng  # shared SIMD substrate on the ZIP arm: raw-DEFLATE compressobj + stored-member crc32

# --- [CONSTANTS] ------------------------------------------------------------------------

# 7z extract bomb bound: `max_extract_size` caps the streamed decode; the ZIP arm is bounded by stream_unzip's rolling chunk drain.
_ARCHIVE_CEILING: Final[int] = 1 << 31
_CONTAINERS: Final[frozenset[CompressionAlgo]] = frozenset({CompressionAlgo.SEVEN_Z, CompressionAlgo.ZIP_STREAM})
_PRESETABLE: Final[frozenset[SevenZFilter]] = frozenset({"lzma", "lzma2"})  # LZMA-family entries that take a preset
_PACK_LANE: Final[LanePolicy] = LanePolicy(capacity=os.process_cpu_count() or 1)  # both arms in-wheel GIL-releasing: THREAD modality

# --- [TABLES] ---------------------------------------------------------------------------

# decrypt allow-list: each `ZipMechanism` token -> its opaque stream_unzip sentinel.
_ZIP_MECHANISM: Final[Map[ZipMechanism, object]] = Map.of_seq([
    ("none", NO_ENCRYPTION),
    ("zipcrypto", ZIP_CRYPTO),
    ("ae1", AE_1),
    ("ae2", AE_2),
    ("aes128", AES_128),
    ("aes192", AES_192),
    ("aes256", AES_256),
])

# --- [MODELS] ---------------------------------------------------------------------------


class Archive(Struct, frozen=True):
    bundle: Bundle

    @staticmethod
    def of(
        algo: CompressionAlgo, *payloads: bytes, profile: CodecProfile | None = None, parents: tuple[ContentKey, ...] = ()
    ) -> "Archive":
        if algo not in _CONTAINERS:
            raise ValueError(f"<non-container-algo:{algo}>")  # a single-blob/delta algo is the sibling producer's construction
        return Archive(bundle=Bundle.of(algo, *payloads, profile=profile, parents=parents))

    def emit(self, /) -> ArtifactWork:
        return ArtifactWork(key=self.bundle.key, work=self._emit, parents=self.bundle.parents, admission=Admission(keyed=None), cost=self._cost)

    async def _emit(self, /) -> RuntimeRail[ArtifactReceipt]:
        packed = await _PACK_LANE.offload(
            Archive.pack, self.bundle.payloads, self.bundle.algo, self.bundle.profile, modality=Modality.THREAD, retry=RetryClass.OCCT
        )
        return packed.map(lambda pe: pe[1].receipt(self.bundle.key))

    async def unpack(self, blob: bytes, /) -> RuntimeRail[BundleManifest]:
        rows = await _PACK_LANE.offload(
            Archive.recover, blob, self.bundle.algo, self.bundle.profile, modality=Modality.THREAD, retry=RetryClass.OCCT
        )
        return rows.map(lambda recovered: BundleManifest.of(self.bundle.algo, recovered))

    @property
    def _cost(self) -> float:
        return float(sum(map(len, self.bundle.payloads)) or 1)  # byte-volume CPM weight

    @staticmethod
    def pack(payloads: tuple[bytes, ...], algo: CompressionAlgo, profile: CodecProfile, /) -> tuple[bytes, BundleEvidence]:
        match profile:
            case CodecProfile(tag="seven_z", seven_z=SevenZKnobs() as k):
                ident = {
                    "lzma": py7zr.FILTER_LZMA,
                    "lzma2": py7zr.FILTER_LZMA2,
                    "bzip2": py7zr.FILTER_BZIP2,
                    "ppmd": py7zr.FILTER_PPMD,
                    "zstd": py7zr.FILTER_ZSTD,
                    "brotli": py7zr.FILTER_BROTLI,
                    "deflate": py7zr.FILTER_DEFLATE,
                    "copy": py7zr.FILTER_COPY,
                    "delta": py7zr.FILTER_DELTA,
                    "x86": py7zr.FILTER_X86,
                    "arm": py7zr.FILTER_ARM,
                    "armthumb": py7zr.FILTER_ARMTHUMB,
                    "powerpc": py7zr.FILTER_POWERPC,
                    "sparc": py7zr.FILTER_SPARC,
                    "ia64": py7zr.FILTER_IA64,
                }  # arm-scope row: the lazy py7zr proxy reifies here, never at module load
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
                    # declared uncompressed total: the reconstructed size for `frame_size`, not the redundant len(blob) on `out_bytes`.
                    uncompressed = reader.archiveinfo().uncompressed
                return blob, BundleEvidence.measure(algo, 0, 0, uncompressed, verified, payloads, (blob,))
            case CodecProfile(tag="zip_stream", zip_stream=ZipStreamKnobs() as k):
                blob = b"".join(
                    stream_zip(
                        _zip_members(payloads, k),
                        password=k.password,
                        extended_timestamps=False,
                        get_compressobj=lambda: zlib_ng.compressobj(wbits=-zlib_ng.MAX_WBITS, level=k.level),
                    )
                )
                # stream_zip inline-verifies only stored members; the pack round-trips through `_zip_drain` and counts survivors.
                verified = sum(1 for _ in _zip_drain(blob, *_zip_trust(k)))
                return blob, BundleEvidence.measure(algo, k.level, 0, sum(map(len, payloads)), verified, payloads, (blob,))
            case _:
                raise ValueError(f"<non-archive-profile:{profile.tag}>")  # the codec/delta tags resolve on siblings; the family is not exhausted here

    @staticmethod
    def recover(blob: bytes, algo: CompressionAlgo, profile: CodecProfile, /) -> tuple[MemberTriple, ...]:
        match profile:
            case CodecProfile(tag="seven_z", seven_z=SevenZKnobs() as k):
                sinks = _Xxh3Factory()  # streamed xxh3_128 sink: each entry decodes into one digest, never buffered whole
                with py7zr.SevenZipFile(BytesIO(blob), mode="r", password=k.password, max_extract_size=_ARCHIVE_CEILING) as reader:
                    infos = [info for info in reader.list() if not info.is_directory]
                    reader.extractall(factory=sinks)
                return tuple((info.filename, info.uncompressed, sinks.get(info.filename).read()) for info in infos)
            case CodecProfile(tag="zip_stream", zip_stream=ZipStreamKnobs() as k):
                return tuple(_zip_drain(blob, *_zip_trust(k)))
            case _:
                raise ValueError(f"<non-archive-profile:{profile.tag}>")


# --- [BOUNDARIES] -----------------------------------------------------------------------

# the py7zr streamed-sink protocol (Py7zIO + WriterFactory) folded onto the runtime xxh3_128 family, replacing the
# built-in sha256 `HashIOFactory`. Duck-typed, not subclassed, so the base classes stay behind the lazy py7zr proxy.


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
    # password as stream_unzip's bytes (it rejects str) + the mechanisms allow-list; shared by pack-verify and unpack.
    return (knobs.password.encode() if knobs.password is not None else None, frozenset(_ZIP_MECHANISM[m] for m in knobs.mechanisms))


def _zip_drain(blob: bytes, password: bytes | None, mechanisms: frozenset[object], /) -> Iterator[MemberTriple]:
    # the ONE streamed ZIP reader both legs consume: draining a member triggers stream_unzip's CRC32/size/HMAC
    # verification (drains without raising = integrity-proven) while one xxh3_128 rolls per member. The typed fault
    # subtree classifies most-specific-first at the seam into a closed trust/corruption/unsupported/ordering discriminant.
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
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
