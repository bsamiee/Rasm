# [PY_ARTIFACTS_ARCHIVE]

`Archive` is the multi-file container producer over the two container rows — the `py7zr` `SevenZipFile` 7z container and the `stream-zip`/`stream-unzip` bounded-memory ZIP container. It folds a `*payloads` spread into ONE container whose directory recovers the full member row set, never N single-frame bundles, composing the `package/bundle#BUNDLE` vocabulary downward and importing no sibling.

`emit() -> ArtifactWork` carries the producer contract — `key = Bundle.key`, `Admission(keyed=None)`, `receipt.slot == node.key` — `packed()` is the composite-consumer face returning the sealed container bytes plus evidence on the rail (`delivery/transmittal#TRANSMITTAL` seals through it), and `parents` is the scene-bundling seam: a caller bundling upstream artifacts (scene files, sheet PDFs, transmittal members) passes their content keys, so the plan graph holds a DATA edge (`scene/export` emits files and reaches no compression plane) and an identical member set elides before the container writes. An unencrypted ZIP is byte-reproducible — the fixed bundle `_EPOCH` stamp with `extended_timestamps=False` and the shared `zlib_ng` raw-DEFLATE at the profile `level` — so its content key dedups across runs; the encrypted ZIP is intentionally non-reproducible (fresh AES salt/IV per pack) and the 7z container honestly non-reproducible (`py7zr` stamps an uncontrollable wall-clock `creationtime` per entry).

## [01]-[INDEX]

- [02]-[ARCHIVE]: the `Archive` producer over the `SEVEN_Z`/`ZIP_STREAM` rows — the `emit`/`packed`/`unpack` node contract, the `PackWorker` port kernels, the streamed `xxh3_128` sinks, and the classified budget-bounded `_zip_drain` reader shared by pack-verify and unpack.

## [02]-[ARCHIVE]

- Owner: `Archive` the one container producer wrapping the `Bundle` carrier with its `lane: LanePolicy` — `SevenZKnobs`/`ZipStreamKnobs` are bundle-page vocabulary, so a 7z or ZIP container is one profile row on the one union, never a parallel owner. `Archive.pack`/`Archive.recover` are the `PackWorker` port kernels over `(payloads, profile)` — the profile IS the discriminant, its `algo` derived — public staticmethods, total over the two rows, GIL-releasing in-wheel so both directions cross `KernelTrait.RELEASING` on the thread arm.
- Cases: the `SevenZFilter` vocabulary resolves at arm scope to `py7zr.FILTER_*` ids (never a bare ordinal or string-built `getattr`); `Bundle.of` admits one unique terminal codec after only DELTA/BCJ preprocessors and lets the `"extreme"` preset reach only terminal `"lzma"`/`"lzma2"`, while the AES-256 chain entry appends when `password` is set. `ZIP_STREAM` binds the shared `zlib_ng` SIMD raw-DEFLATE at the profile `level` for the forced and stored formats, while the `"auto"` row's `ZIP_AUTO(size, level)` binds its own stdlib raw-DEFLATE at the SAME `level` — the one documented `stream-zip` asymmetry, level parity across every member, substrate parity on the forced rows alone. `Bundle.of` admits `names` only when empty or arity-equal, unique, relative POSIX-safe, and requires a password for 7z header encryption, so no ignored name, overwritten digest sink, traversal member, invalid filter chain, or unencrypted-header contradiction reaches a worker. Encryption is ONE discriminant, password presence: `stream_zip` writes exactly WinZip AE-2/AES-256 when a password is set, so `_zip_trust` derives BOTH the pack behavior and the decode allow-list from the same value — `None` admits only plaintext, set admits only the AE-2/AES-256 pair — and a decode-only trust roster that rejects its own pack leg is unspellable; legacy `ZIP_CRYPTO` and the weak AE-1/short-key variants never decode.
- Entry: `packed` offloads `Archive.pack` and `_emit` maps it onto `evidence.receipt(self.bundle.key)`; `unpack` maps `Archive.recover` onto `BundleManifest.of`; recovery buffers no payload — 7z streams each entry through the `_Xxh3Factory` sink under `max_extract_size=_ARCHIVE_CEILING` (a high-ratio container raises `DecompressionBombError`, a traversal name `AbsolutePathError`) and rejects duplicate names before the filename-keyed sink map materializes. ZIP drains chunks through `_zip_drain`'s rolling `xxh3_128` under both a per-member ceiling and the SAME aggregate `_ARCHIVE_CEILING` budget — streaming bounds the working set, the budgets bound each member and total output, and an excess faults `<zip-unpack:bomb>` mid-drain rather than emitting unbounded bytes. `_zip_drain` also rejects duplicate or non-UTF-8 names and classifies the typed `stream_unzip` fault subtree most-specific-first into the `<zip-unpack:*>` family.
- Output: `verified` is a GENUINE per-member proof — `stream_zip` inline-CRC-verifies only stored members, so the pack arm round-trips the sealed blob through `_zip_drain` (the drain fires the streamed CRC32/size/HMAC) and counts survivors, while 7z requires `test()` success before evidence materializes; `frame_size` reads the container's declared `uncompressed` total summed off the `FileInfo` rows (the true reconstructed size, not the redundant `out_bytes` blob length — `archiveinfo()` asserts a filename and is unreachable on a `BytesIO` archive), and a member that fails integrity faults loudly rather than returning a zero-proof receipt.
- Packages: `py7zr` (lazy, reifies at arm scope), `stream-zip`/`stream-unzip` (eager — the sentinel families are module vocabulary), `zlib-ng` (lazy — the shared SIMD raw-DEFLATE + `crc32` substrate, composed never re-admitted), `xxhash` (streamed member digests), `msgspec` (`Struct`), runtime `identity`/`faults`/`lanes`/`resilience`, `rasm.artifacts.core.plan`/`core.receipt`/`package.bundle`.
- Growth: a new container algorithm is one bundle-page row set plus one arm in each of `pack`/`recover`; a new ZIP method is one `ZipMethod` token plus one `_zip_members` match arm; a new 7z filter is one `SevenZFilter` token plus one arm-scope ident entry; a new encryption mechanism lands the day `stream_zip` writes one, as one more derived pair in `_zip_trust` — container evidence rides the existing `entries`/`verified` slots, zero new verb beside `emit`/`packed`/`unpack`.
- Boundary: no sibling import, no vocabulary re-own (bundle carries the knobs), no folder-minted limiter or retry caller, no receipt-case widening (container facts fold only through the `FileInfo` `uncompressed` sum), no wall-clock member stamp on the ZIP arm, no `async_stream_zip`/`async_stream_unzip` (both bridge onto their own thread executor; the kernels already cross the runtime `THREAD` lane, and a second loop-bridged executor beside it double-threads the crossing). Per-member stamps stay container-level by the reproducibility law — `names` is the one per-member axis, `"auto"` resolves ZIP32/ZIP64 per member by size and offset — so the deliberate collapse is the content-addressing contract, not a modeling gap.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from collections.abc import Iterable, Iterator
from datetime import datetime
from io import BytesIO
from typing import Final, assert_never

import xxhash
from msgspec import Struct
from stream_unzip import (
    AE_2,
    AES_256,
    NO_ENCRYPTION,
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
from rasm.runtime.lanes import LanePolicy
from rasm.runtime.workers import Kernel, KernelTrait

from rasm.artifacts.core.plan import Admission, ArtifactWork
from rasm.artifacts.core.receipt import ArtifactReceipt
from rasm.artifacts.package.bundle import (
    Bundle,
    BundleEvidence,
    BundleManifest,
    CodecProfile,
    CompressionAlgo,
    MemberTriple,
    SevenZFilter,
    SevenZKnobs,
    ZipStreamKnobs,
    unsafe_member,
)

lazy import py7zr
lazy from zlib_ng import zlib_ng  # shared SIMD substrate on the ZIP arm: raw-DEFLATE compressobj + stored-member crc32

# --- [CONSTANTS] ------------------------------------------------------------------------

# one bomb ceiling, two enforcement seams: 7z caps the streamed decode via `max_extract_size`; the ZIP drain
# spends the same value as an aggregate output budget across every member.
_ARCHIVE_CEILING: Final[int] = 1 << 31
_MEMBER_CEILING: Final[int] = 1 << 16  # ZIP central-directory bound: a metadata bomb of empty members exhausts CPU/memory before any byte budget bites
_METADATA_OVERHEAD: Final[int] = 128  # per-record charge (name bytes ride on top) debited from the same aggregate budget
_CONTAINERS: Final[frozenset[CompressionAlgo]] = frozenset({CompressionAlgo.SEVEN_Z, CompressionAlgo.ZIP_STREAM})
_PRESETABLE: Final[frozenset[SevenZFilter]] = frozenset({"lzma", "lzma2"})  # LZMA-family entries that take a preset
# --- [MODELS] ---------------------------------------------------------------------------


class Archive(Struct, frozen=True):
    # `lane` arrives projected via LanePolicy.of(context) at the composition root — a capacity literal has no owner.
    bundle: Bundle
    lane: LanePolicy

    @staticmethod
    def of(selector: CompressionAlgo | CodecProfile, *payloads: bytes, lane: LanePolicy, parents: tuple[ContentKey, ...] = ()) -> "Archive":
        built = Bundle.of(selector, *payloads, parents=parents)
        if built.algo not in _CONTAINERS:
            raise ValueError(f"<non-container-algo:{built.algo}>")
        return Archive(bundle=built, lane=lane)

    def emit(self, /) -> ArtifactWork:
        return ArtifactWork(key=self.bundle.key, work=self._emit, parents=self.bundle.parents, admission=Admission(keyed=None), cost=self._cost)

    async def packed(self, /) -> RuntimeRail[tuple[bytes, BundleEvidence]]:
        # both container arms are in-wheel GIL-releasing native, so the kernel crosses RELEASING on the thread arm.
        return await self.lane.offload(Kernel.of(Archive.pack, KernelTrait.RELEASING), self.bundle.payloads, self.bundle.profile)

    async def _emit(self, /) -> RuntimeRail[ArtifactReceipt]:
        return (await self.packed()).map(lambda pe: pe[1].receipt(self.bundle.key))

    async def unpack(self, blob: bytes, /) -> RuntimeRail[BundleManifest]:
        rows = await self.lane.offload(Kernel.of(Archive.recover, KernelTrait.RELEASING), blob, self.bundle.profile)
        return rows.map(lambda recovered: BundleManifest.of(self.bundle.algo, recovered))

    @property
    def _cost(self) -> float:
        return float(sum(map(len, self.bundle.payloads)) or 1)

    @staticmethod
    def pack(payloads: tuple[bytes, ...], profile: CodecProfile, /) -> tuple[bytes, BundleEvidence]:
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
                    uncompressed = sum(info.uncompressed for info in reader.list() if not info.is_directory)
                    if reader.test() is False:
                        raise ValueError("<7z-verify:integrity>")
                return blob, BundleEvidence.measure(profile.algo, 0, 0, uncompressed, len(payloads), payloads, blob)
            case CodecProfile(tag="zip_stream", zip_stream=ZipStreamKnobs() as k):
                blob = b"".join(
                    stream_zip(
                        _zip_members(payloads, k),
                        password=k.password,
                        extended_timestamps=False,
                        get_compressobj=lambda: zlib_ng.compressobj(wbits=-zlib_ng.MAX_WBITS, level=k.level),
                    )
                )
                verified = sum(1 for _ in _zip_drain(blob, *_zip_trust(k)))
                return blob, BundleEvidence.measure(profile.algo, k.level, 0, sum(map(len, payloads)), verified, payloads, blob)
            case _:
                raise ValueError(f"<non-archive-profile:{profile.tag}>")

    @staticmethod
    def recover(blob: bytes, profile: CodecProfile, /) -> tuple[MemberTriple, ...]:
        match profile:
            case CodecProfile(tag="seven_z", seven_z=SevenZKnobs() as k):
                sinks = _Xxh3Factory()
                with py7zr.SevenZipFile(BytesIO(blob), mode="r", password=k.password, max_extract_size=_ARCHIVE_CEILING) as reader:
                    infos = tuple(info for info in reader.list() if not info.is_directory)
                    if len(frozenset(info.filename for info in infos)) != len(infos):
                        raise ValueError("<7z-unpack:duplicate-name>")
                    if any(unsafe_member(info.filename) for info in infos):
                        # hostile-blob names cross the same relative-POSIX law Bundle.of imposes on authored names
                        raise ValueError("<7z-unpack:unsafe-name>")
                    reader.extractall(factory=sinks)
                return tuple((info.filename, info.uncompressed, sinks.get(info.filename).read()) for info in infos)
            case CodecProfile(tag="zip_stream", zip_stream=ZipStreamKnobs() as k):
                return tuple(_zip_drain(blob, *_zip_trust(k)))
            case _:
                raise ValueError(f"<non-archive-profile:{profile.tag}>")


# --- [BOUNDARIES] -----------------------------------------------------------------------

class _Xxh3Sink:
    __slots__ = ("_hasher", "_size")

    def __init__(self) -> None:
        self._hasher, self._size = xxhash.xxh3_128(), 0

    def write(self, data: bytes) -> int:
        self._hasher.update(data)
        self._size += len(data)
        return len(data)

    def read(self, size: int = -1) -> bytes:
        return self._hasher.digest()

    def seek(self, offset: int, whence: int = 0) -> int:
        return self._size

    def size(self) -> int:
        return self._size

    def flush(self) -> None:
        return None

    def close(self) -> None:
        return None


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
                # ZIP_AUTO ignores the function-level get_compressobj and binds its own stdlib raw-DEFLATE at this
                # same `level` — level parity across every member; substrate parity holds on the forced/stored rows.
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
    return (None, frozenset({NO_ENCRYPTION})) if knobs.password is None else (knobs.password.encode(), frozenset({AE_2, AES_256}))


def _zip_drain(blob: bytes, password: bytes | None, mechanisms: frozenset[object], /) -> Iterator[MemberTriple]:
    budget, seen = _ARCHIVE_CEILING, set()
    try:
        for name, _declared, chunks in stream_unzip((blob,), password=password, allowed_encryption_mechanisms=mechanisms):
            if name in seen:
                raise ValueError("<zip-unpack:duplicate-name>")
            seen.add(name)
            # hostile-blob names cross the same relative-POSIX law Bundle.of imposes on authored names — decode
            # first (the UnicodeDecodeError arm below classifies a torn name), then refuse absolute, traversal,
            # NUL, backslash, and empty forms before any budget or payload work is spent on the member.
            decoded = name.decode()
            if unsafe_member(decoded):
                raise ValueError("<zip-unpack:unsafe-name>")
            # metadata bomb gate: a flood of empty members costs no payload bytes, so the record count caps hard and
            # each entry debits its name plus record overhead from the same aggregate budget the chunks drain.
            budget -= len(name) + _METADATA_OVERHEAD
            if len(seen) > _MEMBER_CEILING or budget < 0:
                raise ValueError("<zip-unpack:bomb>")
            total, hasher = 0, xxhash.xxh3_128()
            for chunk in chunks:
                total, budget = total + len(chunk), budget - len(chunk)
                if total > _ARCHIVE_CEILING or budget < 0:
                    raise ValueError("<zip-unpack:bomb>")
                hasher.update(chunk)
            yield decoded, total, hasher.digest()
    except UnicodeDecodeError as name:
        name.add_note("<at:zip-unpack member-name>")
        raise ValueError("<zip-unpack:name>") from name
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
