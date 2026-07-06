# [PY_ARTIFACTS_CODEC]

The single-blob compression PRODUCER over the four codec rows — `ZSTD`/`LZ4`/`BROTLI`/`GZIP` — composing the `package/bundle#BUNDLE` vocabulary downward and importing no sibling. `Codec` packs already-emitted payloads into ONE content-addressed blob of self-delimiting frames and walks the inverse back into a `BundleManifest`; the entry is `emit() -> ArtifactWork` per the one producer contract — the node key is `Bundle.key`'s PRE-RUN input mint (spec ⊕ parent keys), `admission=Admission(keyed=None)`, and `_emit` threads that same key into the terminal `ArtifactReceipt.Bundle` so `receipt.slot == node.key`. Every heavy body rides the runtime lane, never the event loop: the in-wheel GIL-releasing `zstandard`/`zlib-ng` arms offload `Modality.THREAD`, the `lz4`/`brotli` arms cross `Modality.PROCESS`, both under `offload(retry=RetryClass.OCCT)` — the worker-death band — with the runtime `THREAD_BAND`/`WORKER_BAND` owning the limiter; the page mints no `CapacityLimiter` and no retry caller. Every frame carries its own integrity proof (zstd `write_checksum` trailer, lz4 `content_checksum`, gzip RFC 1952 trailer CRC), `verified` reports the codec's real capability (only `BROTLI` honestly zero), and every recovery is bomb-bounded against `_DECOMPRESS_CEILING`. This is the compression close over already-emitted bytes — it produces no upstream artifact and re-owns no vocabulary.

## [01]-[INDEX]

- [02]-[CODEC]: the `Codec` producer over the four single-blob `CompressionAlgo` rows — `of`/`trained` construction, `emit()`/`_emit` the node contract, `unpack` the manifest inverse, `Codec.pack`/`Codec.recover` the `PackWorker` port kernels (one total match each, THREAD or PROCESS by row), the `_walked` `Block.unfold` anamorphic frame walk hashing each frame to `xxh3_128` (the dict-aware `_zstd_frame` decoder factory rehydrating the profile dictionary so a `trained` bundle is recoverable, `_gzip_frame`/`_lz4_frame`/`_brotli_frame` the per-codec end-of-frame seams), the `_gzip_member` size-disposition routing large payloads onto the `gzip_ng_threaded` block-fan, the `_ZSTD_DICT`/`_ZSTD_STRATEGY` provider dispatch rows, and the `_DECOMPRESS_CEILING`/`_DECOMPRESS_WINDOW`/`_GZIP_PARALLEL_THRESHOLD` bomb-and-lane bounds; `zstandard` `ZstdCompressor`/`ZstdCompressionParameters.from_level`(`strategy`/`hash_log`/`chain_log`/`target_length`/`window_log`)/`STRATEGY_*`/`ZstdCompressionDict`(`train_dictionary`/`precompute_compress`)/`multi_compress_to_buffer`(`backend_features`-guarded)/`ZstdDecompressor(dict_data=, max_window_size=)`/`get_frame_parameters`/`frame_content_size`/`MAX_COMPRESSION_LEVEL`/`DICT_TYPE_*`, `lz4` `frame.compress`/`LZ4FrameDecompressor`/`get_frame_info`/`BLOCKSIZE_*`, `brotli` `compress`/`Decompressor`/`MODE_*`, and the shared `zlib-ng` `gzip_ng`/`gzip_ng_threaded`/`zlib_ng`/`crc32_combine` rails settled against the both-tier `.api`, contributing the one `core/receipt#RECEIPT` `ArtifactReceipt.Bundle` case and a `core/plan#PLAN` `ArtifactWork` node.

## [02]-[CODEC]

- Owner: `Codec` the one single-blob producer wrapping the `package/bundle#BUNDLE` `Bundle` carrier; the four codec rows resolve here, the container rows on `package/archive#ARCHIVE`, the delta row on `package/delta#DELTA` — each sibling composes bundle downward, none imports another. `Codec.pack` and `Codec.recover` are the page's `PackWorker` port kernels: public staticmethods, total over the four rows, module-picklable by qualified name so the `PROCESS` offload carries them across the worker lane with the profile crossing as msgspec data, never a native handle.
- Entry: `emit()` returns ONE `ArtifactWork(key=self.bundle.key, work=self._emit, parents=self.bundle.parents, admission=Admission(keyed=None), cost=byte-volume)` — the key mints over the frozen request (algo, profile, payload digests) ⊕ parent keys BEFORE any compression runs, so a re-issued identical bundle elides at admission. `_emit` is the bound zero-arg render thunk: it offloads `Codec.pack` through the runtime lane and maps the rail onto `evidence.receipt(self.bundle.key)` — the receipt carries the input key (`receipt.slot == node.key`), a failed pack folds to the rail fault, and the packed blob's own output address is store-side lane evidence, never the elision key. `unpack(blob)` is the inverse entry: it offloads `Codec.recover` on the same modality row and maps the rail onto `BundleManifest.of`.
- Modality: `pack` folds a singular payload OR a `*payloads` spread into ONE blob of self-delimiting frames, discriminating on spread arity, never a `batch` flag — the `ZSTD` arm runs `multi_compress_to_buffer` (threaded, dictionary-shared) when the spread carries two-or-more AND `zstandard.backend_features` advertises it (the batch carriers exist only under the `cext` backend), falling to a per-payload `compress` loop that also serves the singular arm under `cffi`. `recover` walks frames back through each codec's own end-of-frame seam — `ZSTD`/`GZIP` `decompressobj().unused_data`, `LZ4` `LZ4FrameDecompressor.eof`/`unused_data`, `BROTLI` a per-member 8-byte length prefix (its stream is not concatenation-splittable) — every decode bomb-bounded: refused when the declared size exceeds `_DECOMPRESS_CEILING` or the per-call output cap trips, the `ZSTD` decode additionally bounded `ZstdDecompressor(max_window_size=_DECOMPRESS_WINDOW)` so an adversarial small-declared/huge-window-log frame never forces an oversized allocation before the size guard fires.
- Dictionary: the `ZSTD` arm rehydrates `ZstdCompressionDict(dict_data, dict_type=_ZSTD_DICT[dict_mode])` from `ZstdKnobs.dict_data`/`dict_mode` at arm scope (the trained dictionary crosses any lane as raw bytes) and calls `precompute_compress(compression_params=params)` once for the repeated-compress corpus pass; `Codec.trained` mines `zstandard.train_dictionary` over a many-similar-artifact corpus and binds the trained `as_bytes()` into a `ZSTD` profile — the dominant small-payload ratio win for receipts, glyph runs, and repeated chart JSON. Recovery is symmetric and load-bearing correctness: `_zstd_frame(trained)` closes over ONE dict-aware `ZstdDecompressor(dict_data=trained, max_window_size=_DECOMPRESS_WINDOW)` reused across every frame, because a `FULLDICT` frame decoded dictless raises "Data corruption detected" and a `trained` bundle would otherwise be unrecoverable by its own manifest walk.
- Frame: the `ZSTD` arm tunes through ONE `ZstdCompressionParameters.from_level(level, window_log=, hash_log=, chain_log=, target_length=, threads=, enable_ldm=, write_checksum=, write_content_size=True)` derived object passed as `compression_params=` (never `level=` beside it — mutually exclusive); the matcher axes carry 0 as the from_level auto-sentinel while a `strategy` token overrides through `_ZSTD_STRATEGY` ("auto" skips the override — `STRATEGY_*` has no zero member). Receipt `frame_size` sums `zstandard.frame_content_size` per frame, sentinel-guarded (`CONTENTSIZE_UNKNOWN`/`CONTENTSIZE_ERROR` fold to zero); the level axis caps at `zstandard.MAX_COMPRESSION_LEVEL`; the `GZIP` arm sums original payload lengths (the RFC 1952 trailer is its size source).
- Gzip: the `GZIP` arm composes the shared `zlib-ng` SIMD substrate through the `_gzip_member` size-disposition — below `_GZIP_PARALLEL_THRESHOLD` the single-shot `gzip_ng.compress(payload, compresslevel=, mtime=)`, at/above it the `gzip_ng_threaded.open(sink, "wb", threads=, block_size=)` GIL-escaping block-fan whose per-block CRCs recombine into the RFC 1952 trailer via `crc32_combine` INSIDE the writer, never a hand-rolled partition/pool. The threaded writer exposes no mtime knob, so the member's RFC 1952 header mtime field (bytes `[4:8]`) is overwritten with the fixed `int(mtime)` little-endian stamp — both lanes emit ONE byte-reproducible self-delimiting member, so a multi-payload bundle concatenates into one valid multi-member gzip blob any stdlib `gzip` peer reads, and `recover` walks members through `zlib_ng.decompressobj(wbits=31)` + `unused_data` (never `gzip_ng.decompress`, which erases member boundaries). The level axis is the zlib `0..9` range; the substrate is COMPOSED (the `[ARTIFACTS]`-tagged manifest row), this page its sole live consumer band.
- Integrity: `verified` is the real per-member proof count — `ZSTD` counts `get_frame_parameters(frame).has_checksum` (set by `write_checksum=True`, a 4-byte xxhash64 trailer per frame), `LZ4` counts `get_frame_info(frame)["content_checksum"]`, `GZIP` counts every member (the trailer CRC is always present and verified on decode), and `BROTLI` honestly reports zero (no in-band checksum; transport-integrity only) — never a uniform lie. Declined as architecturally incompatible: `FORMAT_ZSTD1_MAGICLESS` (no magic to delimit, `frame_content_size` unreadable — breaks the bomb-guarded walk), the `stream_writer`/`compressobj` and brotli `Compressor.process` streaming sinks (the ingress is already-materialized `bytes`; a sink adds no back-pressure memory has not committed), `multi_decompress_to_buffer` on recovery (materializes every frame at once, breaking the bounded `_walked` anamorphism), and the `lz4.block dict=` primer (the many-small-similar concern is `trained`'s zstd `FULLDICT` ownership; lz4 owns the orthogonal hot-path).
- Growth: a new single-blob algorithm is one bundle-page `CompressionAlgo`/`CodecProfile`/`DEFAULT_PROFILE` row, one `pack` arm, one frame decoder, and one `recover` arm here; a new tuning knob is one field on the owning bundle-page knob-struct; a new bounded knob value is one `Literal` token plus one arm-scope dispatch row; zero new verb beside the `emit`/`unpack` pair.
- Packages: `zstandard` (eager — the core arm and `trained`), `lz4.frame`/`brotli`/`zlib-ng` (lazy — reify at arm scope, in the worker process for the `PROCESS` rows), `xxhash` (`xxh3_128_digest` the walk digests on the runtime identity family), `expression` (`Block.unfold` the anamorphism, `Map.of_seq` the dispatch rows, `Option`/`Some`/`Nothing` the walk step), `msgspec` (`Struct`), runtime `identity`/`faults`/`lanes`/`resilience` (`ContentKey`, `RuntimeRail`, `LanePolicy.offload`/`Modality`, `RetryClass.OCCT`), `artifacts.core.plan` (`ArtifactWork`/`Admission`), `artifacts.core.receipt` (`ArtifactReceipt`), `artifacts.package.bundle` (the whole vocabulary floor).
- Boundary: no sibling import (archive/delta compose bundle themselves), no vocabulary re-own, no folder-minted `CapacityLimiter`/`stamina` caller (the lane bands own concurrency; `OCCT` owns worker-death retry), no receipt-case widening (the eight-scalar `Bundle` case carries every codec), no key-over-output mint (`ContentIdentity.of(blob)` as the node key is the deleted form — the output address is lane admission evidence only).

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
import os
from collections.abc import Callable
from io import BytesIO
from typing import Final

import xxhash
import zstandard
from expression import Nothing, Option, Some
from expression.collections import Block, Map
from msgspec import Struct

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
    BrotliKnobs,
    CodecProfile,
    CompressionAlgo,
    GzipKnobs,
    Lz4Knobs,
    MemberTriple,
    ZstdDictMode,
    ZstdKnobs,
    ZstdStrategy,
)

lazy import brotli
lazy import lz4.frame
lazy from zlib_ng import gzip_ng, gzip_ng_threaded, zlib_ng

# --- [CONSTANTS] ------------------------------------------------------------------------

_DECOMPRESS_CEILING: Final[int] = 1 << 31  # per-frame decompressed-output bomb ceiling; a declared or actual size above it is refused
_DECOMPRESS_WINDOW: Final[int] = 1 << 27  # zstd window bound: a small-declared/huge-window-log frame never allocates before the size guard
_GZIP_PARALLEL_THRESHOLD: Final[int] = 1 << 20  # at/above: the gzip_ng_threaded block-fan; below: single-shot — a size disposition, not a flag
_SINGLE_BLOB: Final[frozenset[CompressionAlgo]] = frozenset(
    {CompressionAlgo.ZSTD, CompressionAlgo.LZ4, CompressionAlgo.BROTLI, CompressionAlgo.GZIP}
)
# the page's one offload seam: THREAD for the in-wheel GIL-releasing zstd/zlib-ng arms, PROCESS for lz4/brotli;
# the runtime THREAD_BAND/WORKER_BAND own the limiter — zero artifacts-minted CapacityLimiter.
_PACK_LANE: Final[LanePolicy] = LanePolicy(capacity=os.process_cpu_count() or 1)

# --- [TABLES] ---------------------------------------------------------------------------

_ZSTD_DICT: Final[Map[ZstdDictMode, int]] = Map.of_seq([
    ("auto", zstandard.DICT_TYPE_AUTO),
    ("fulldict", zstandard.DICT_TYPE_FULLDICT),
    ("rawcontent", zstandard.DICT_TYPE_RAWCONTENT),
])
# the nine named DEFLATE-matcher strategies fast-to-densest; "auto" absent by design (it skips the from_level override)
_ZSTD_STRATEGY: Final[Map[ZstdStrategy, int]] = Map.of_seq([
    ("fast", zstandard.STRATEGY_FAST),
    ("dfast", zstandard.STRATEGY_DFAST),
    ("greedy", zstandard.STRATEGY_GREEDY),
    ("lazy", zstandard.STRATEGY_LAZY),
    ("lazy2", zstandard.STRATEGY_LAZY2),
    ("btlazy2", zstandard.STRATEGY_BTLAZY2),
    ("btopt", zstandard.STRATEGY_BTOPT),
    ("btultra", zstandard.STRATEGY_BTULTRA),
    ("btultra2", zstandard.STRATEGY_BTULTRA2),
])

# --- [MODELS] ---------------------------------------------------------------------------


class Codec(Struct, frozen=True):
    bundle: Bundle

    @staticmethod
    def of(
        algo: CompressionAlgo, *payloads: bytes, profile: CodecProfile | None = None, parents: tuple[ContentKey, ...] = ()
    ) -> "Codec":
        if algo not in _SINGLE_BLOB:
            raise ValueError(f"<non-codec-algo:{algo}>")  # a container/delta algo is the sibling producer's construction
        return Codec(bundle=Bundle.of(algo, *payloads, profile=profile, parents=parents))

    @staticmethod
    def trained(
        corpus: tuple[bytes, ...], *payloads: bytes, level: int = 19, dict_size: int = 112_640, parents: tuple[ContentKey, ...] = ()
    ) -> "Codec":
        dict_data = zstandard.train_dictionary(dict_size, list(corpus))  # COVER-trains a fulldict; dict_type is a ctor axis, not a train kwarg
        profile = CodecProfile(zstd=ZstdKnobs(level=level, dict_data=dict_data.as_bytes(), dict_mode="fulldict"))
        return Codec.of(CompressionAlgo.ZSTD, *payloads, profile=profile, parents=parents)

    def emit(self, /) -> ArtifactWork:
        return ArtifactWork(key=self.bundle.key, work=self._emit, parents=self.bundle.parents, admission=Admission(keyed=None), cost=self._cost)

    async def _emit(self, /) -> RuntimeRail[ArtifactReceipt]:
        packed = await _PACK_LANE.offload(
            Codec.pack, self.bundle.payloads, self.bundle.algo, self.bundle.profile, modality=self._modality, retry=RetryClass.OCCT
        )
        # threads the PRE-RUN input key so receipt.slot == node.key; the blob's own output address is lane-admission evidence
        return packed.map(lambda pe: pe[1].receipt(self.bundle.key))

    async def unpack(self, blob: bytes, /) -> RuntimeRail[BundleManifest]:
        rows = await _PACK_LANE.offload(
            Codec.recover, blob, self.bundle.algo, self.bundle.profile, modality=self._modality, retry=RetryClass.OCCT
        )
        return rows.map(lambda recovered: BundleManifest.of(self.bundle.algo, recovered))

    @property
    def _cost(self) -> float:
        return float(sum(map(len, self.bundle.payloads)) or 1)  # byte-volume CPM weight the plan's forward pass sums

    @property
    def _modality(self) -> Modality:
        # zstd/zlib-ng release the GIL in-wheel (THREAD_BAND); lz4/brotli cross the worker lane (WORKER_BAND)
        return Modality.PROCESS if self.bundle.algo in (CompressionAlgo.LZ4, CompressionAlgo.BROTLI) else Modality.THREAD

    @staticmethod
    def pack(payloads: tuple[bytes, ...], algo: CompressionAlgo, profile: CodecProfile, /) -> tuple[bytes, BundleEvidence]:
        # the PackWorker pack kernel — ONE total match over the four rows; picklable by qualified name for the PROCESS lane
        match profile:
            case CodecProfile(tag="zstd", zstd=ZstdKnobs() as k):
                level = min(k.level, zstandard.MAX_COMPRESSION_LEVEL)
                strategy = {"strategy": _ZSTD_STRATEGY[k.strategy]} if k.strategy != "auto" else {}
                params = zstandard.ZstdCompressionParameters.from_level(
                    level,
                    window_log=k.window_log,
                    hash_log=k.hash_log,
                    chain_log=k.chain_log,
                    target_length=k.target_length,
                    threads=k.threads,
                    enable_ldm=k.enable_ldm,
                    write_checksum=k.write_checksum,
                    write_content_size=True,
                    **strategy,
                )
                trained = zstandard.ZstdCompressionDict(k.dict_data, dict_type=_ZSTD_DICT[k.dict_mode]) if k.dict_data is not None else None
                if trained is not None:
                    trained.precompute_compress(compression_params=params)  # cache the compress-side dict state for the corpus pass
                compressor = zstandard.ZstdCompressor(dict_data=trained, compression_params=params)
                frames = (
                    tuple(bytes(segment) for segment in compressor.multi_compress_to_buffer(list(payloads), threads=k.threads))
                    if len(payloads) > 1 and "multi_compress_to_buffer" in zstandard.backend_features
                    else tuple(compressor.compress(payload) for payload in payloads)
                )
                blob = b"".join(frames)
                verified = sum(zstandard.get_frame_parameters(frame).has_checksum for frame in frames)
                dict_id = trained.dict_id() if trained is not None else 0
                return blob, BundleEvidence.measure(algo, level, dict_id, sum(map(_declared, frames)), verified, payloads, (blob,))
            case CodecProfile(tag="gzip", gzip=GzipKnobs() as k):
                blob = b"".join(_gzip_member(payload, k) for payload in payloads)
                return blob, BundleEvidence.measure(algo, k.level, 0, sum(map(len, payloads)), len(payloads), payloads, (blob,))
            case CodecProfile(tag="lz4", lz4=Lz4Knobs() as k):
                blocks = Map.of_seq([
                    ("default", lz4.frame.BLOCKSIZE_DEFAULT),
                    ("max64kb", lz4.frame.BLOCKSIZE_MAX64KB),
                    ("max256kb", lz4.frame.BLOCKSIZE_MAX256KB),
                    ("max1mb", lz4.frame.BLOCKSIZE_MAX1MB),
                    ("max4mb", lz4.frame.BLOCKSIZE_MAX4MB),
                ])  # arm-scope row: the lazy lz4 proxy reifies here, in the worker process, never at module load
                frames = tuple(
                    lz4.frame.compress(
                        payload,
                        compression_level=k.compression_level,
                        block_size=blocks[k.block_size],
                        content_checksum=k.content_checksum,
                        store_size=True,
                    )
                    for payload in payloads
                )
                blob = b"".join(frames)
                verified = sum(lz4.frame.get_frame_info(frame)["content_checksum"] for frame in frames)
                frame_size = sum(lz4.frame.get_frame_info(frame)["content_size"] for frame in frames)
                return blob, BundleEvidence.measure(algo, k.compression_level, 0, frame_size, verified, payloads, (blob,))
            case CodecProfile(tag="brotli", brotli=BrotliKnobs() as k):
                modes = Map.of_seq([("generic", brotli.MODE_GENERIC), ("text", brotli.MODE_TEXT), ("font", brotli.MODE_FONT)])
                # a Brotli stream is NOT self-delimiting on concatenation, so each member rides an 8-byte big-endian
                # length prefix the `_brotli_frame` walk slices back — the whole spread folds, never silently payloads[0].
                frames = tuple(brotli.compress(payload, mode=modes[k.mode], quality=k.quality, lgwin=k.lgwin, lgblock=k.lgblock) for payload in payloads)
                blob = b"".join(len(frame).to_bytes(8, "big") + frame for frame in frames)
                return blob, BundleEvidence.measure(algo, k.quality, 0, sum(map(len, payloads)), 0, payloads, (blob,))
            case _:
                raise ValueError(f"<non-codec-profile:{profile.tag}>")  # container/delta tags resolve on siblings; the family is not exhausted here

    @staticmethod
    def recover(blob: bytes, algo: CompressionAlgo, profile: CodecProfile, /) -> tuple[MemberTriple, ...]:
        # the PackWorker recover kernel — each row walks its own end-of-frame seam through the one `_walked` anamorphism
        match profile:
            case CodecProfile(tag="zstd", zstd=ZstdKnobs() as k):
                trained = zstandard.ZstdCompressionDict(k.dict_data, dict_type=_ZSTD_DICT[k.dict_mode]) if k.dict_data is not None else None
                return _walked(blob, _zstd_frame(trained))  # dict-aware: a trained fulldict frame is unrecoverable without its dictionary
            case CodecProfile(tag="gzip"):
                return _walked(blob, _gzip_frame)
            case CodecProfile(tag="lz4"):
                return _walked(blob, _lz4_frame)
            case CodecProfile(tag="brotli"):
                return _walked(blob, _brotli_frame)
            case _:
                raise ValueError(f"<non-codec-profile:{profile.tag}>")


# --- [OPERATIONS] -----------------------------------------------------------------------


def _declared(frame: bytes, /) -> int:
    size = zstandard.frame_content_size(frame)
    return size if size >= 0 else 0  # CONTENTSIZE_UNKNOWN/CONTENTSIZE_ERROR fold to zero, never poison the evidence


def _gzip_member(payload: bytes, k: GzipKnobs, /) -> bytes:
    # size disposition on the value: at/above the threshold the GIL-escaping gzip_ng_threaded block-fan (per-block CRCs
    # recombined via crc32_combine INSIDE the writer), below it single-shot; the threaded writer exposes no mtime knob,
    # so its RFC 1952 header mtime field (bytes [4:8]) is overwritten with the fixed stamp — both lanes byte-reproducible.
    if len(payload) < _GZIP_PARALLEL_THRESHOLD:
        return gzip_ng.compress(payload, compresslevel=k.level, mtime=k.mtime)
    sink = BytesIO()
    with gzip_ng_threaded.open(sink, "wb", compresslevel=k.level, threads=k.threads, block_size=k.block_size) as writer:
        writer.write(payload)
    raw = sink.getvalue()
    return raw[:4] + int(k.mtime).to_bytes(4, "little") + raw[8:]


def _walked(blob: bytes, decode: Callable[[bytes], tuple[bytes, bytes]], /) -> tuple[MemberTriple, ...]:
    # anamorphic frame walk: each `decode` peels one self-delimiting frame, the payload hashed to a 16-byte xxh3_128
    # digest (the runtime identity family) and freed per step, so the walk never buffers every decompressed member.
    def step(seed: tuple[bytes, int], /) -> Option[tuple[MemberTriple, tuple[bytes, int]]]:
        rest, index = seed
        if not rest:
            return Nothing
        payload, tail = decode(rest)
        return Some(((f"payload-{index}", len(payload), xxhash.xxh3_128_digest(payload)), (tail, index + 1)))

    return tuple(Block.unfold(step, (blob, 0)))


def _zstd_frame(trained: "zstandard.ZstdCompressionDict | None", /) -> Callable[[bytes], tuple[bytes, bytes]]:
    # ONE dict-aware decompressor built once and reused across every frame; max_window_size is the second bomb gate
    # the declared-size guard alone cannot cover.
    decompressor = zstandard.ZstdDecompressor(dict_data=trained, max_window_size=_DECOMPRESS_WINDOW)

    def decode(frame: bytes, /) -> tuple[bytes, bytes]:
        if not 0 <= _declared(frame) <= _DECOMPRESS_CEILING:
            raise ValueError("<decompression-bomb>")
        obj = decompressor.decompressobj(read_across_frames=False)
        return obj.decompress(frame), obj.unused_data

    return decode


def _gzip_frame(frame: bytes, /) -> tuple[bytes, bytes]:
    obj = zlib_ng.decompressobj(wbits=31)
    payload = obj.decompress(frame, _DECOMPRESS_CEILING)
    if obj.unconsumed_tail:
        raise ValueError("<decompression-bomb>")
    return payload, obj.unused_data


def _lz4_frame(frame: bytes, /) -> tuple[bytes, bytes]:
    decoder = lz4.frame.LZ4FrameDecompressor()
    payload = decoder.decompress(frame, max_length=_DECOMPRESS_CEILING)
    if not decoder.eof:
        raise ValueError("<decompression-bomb>")
    return payload, decoder.unused_data


def _brotli_frame(frame: bytes, /) -> tuple[bytes, bytes]:
    length = int.from_bytes(frame[:8], "big")  # the 8-byte member-length prefix isolates one Brotli stream
    body, tail = frame[8 : 8 + length], frame[8 + length :]
    decoder = brotli.Decompressor()
    payload = decoder.process(body, output_buffer_limit=_DECOMPRESS_CEILING)  # output cap bomb-bounds; the prefix bounds the input span
    if not decoder.is_finished():
        raise ValueError("<decompression-bomb>")
    return payload, tail
```
