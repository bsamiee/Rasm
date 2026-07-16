# [PY_ARTIFACTS_CODEC]

`Codec` is the single-blob compression producer over the four codec rows — `ZSTD`/`LZ4`/`BROTLI`/`GZIP`. It packs already-emitted payloads into ONE content-addressed blob of self-delimiting frames and walks the inverse back into a `BundleManifest`, composing the `package/bundle#BUNDLE` vocabulary downward and importing no sibling.

`emit() -> ArtifactWork` carries the producer contract — the node key is `Bundle.key`'s pre-run mint (spec ⊕ parent keys), `Admission(keyed=None)`, and `_emit` threads that key into the terminal `ArtifactReceipt.Bundle` so `receipt.slot == node.key`. In-wheel GIL-releasing `zstandard`/`zlib-ng` arms offload `Modality.THREAD` while the `lz4`/`brotli` arms cross `Modality.PROCESS`; every frame carries its own integrity proof (zstd `write_checksum`, lz4 `content_checksum`, gzip trailer CRC), `verified` reports the codec's real capability (only `BROTLI` honestly zero), and every recovery is bomb-bounded against `_DECOMPRESS_CEILING`.

## [01]-[INDEX]

- [02]-[CODEC]: the `Codec` producer over the four single-blob rows — the `emit`/`unpack` node contract, the `PackWorker` port kernels, the `_walked` anamorphic frame walk, and the per-codec end-of-frame decoders.

## [02]-[CODEC]

- Owner: `Codec` the one single-blob producer wrapping the `Bundle` carrier; the four codec rows resolve here, the container rows on `package/archive#ARCHIVE`, the delta row on `package/delta#DELTA`. `Codec.pack`/`Codec.recover` are the `PackWorker` port kernels: public staticmethods, total over the four rows, module-picklable so the `PROCESS` offload carries them by qualified name with the profile crossing as data.
- Cases: `pack` discriminates on spread arity, never a `batch` flag — the `ZSTD` arm takes `multi_compress_to_buffer` for a two-or-more spread its backend advertises, else a per-payload loop serving the singular arm too, tuning through ONE `from_level(...)` object (`compression_params=`, never `level=` beside it — mutually exclusive). Both `GZIP` lanes emit one byte-reproducible self-delimiting member any stdlib `gzip` reads, and `BROTLI` rides an 8-byte length prefix since its stream is not concatenation-splittable. `recover` walks each codec's own end-of-frame seam, every decode bomb-bounded against `_DECOMPRESS_CEILING` and the `ZSTD` decode additionally `max_window_size=_DECOMPRESS_WINDOW` against a small-declared/huge-window frame; `_zstd_frame(trained)` reuses ONE dict-aware decompressor across frames because a `FULLDICT` frame decoded dictless raises corruption and a `trained` bundle is otherwise unrecoverable by its own manifest walk.
- Entry: `_emit` offloads `Codec.pack` and maps onto `evidence.receipt(self.bundle.key)`, `unpack` offloads `Codec.recover` on the same modality row and maps onto `BundleManifest.of`. `Codec.trained` mines `zstandard.train_dictionary` over a many-similar corpus and binds the trained `as_bytes()` into a `ZSTD` profile — the dominant small-payload ratio win for receipts, glyph runs, and repeated chart JSON.
- Output: `verified` is the real per-member proof count, never a uniform lie — the zstd `write_checksum` trailer, lz4 `content_checksum`, and gzip trailer CRC each verify on decode while `BROTLI` honestly reports zero (no in-band checksum); `frame_size` reads `zstandard.frame_content_size` per zstd frame (sentinel-guarded to zero), the other arms summing payload or `content_size` lengths, the level axis capping at `zstandard.MAX_COMPRESSION_LEVEL`.
- Packages: `zstandard` (eager — the core arm and `trained`), `lz4.frame`/`brotli`/`zlib-ng` (lazy — reify at arm scope, in the worker process for the `PROCESS` rows), `xxhash` (`xxh3_128_digest` the walk digests), `expression` (`Block.unfold` the anamorphism, `Map.of_seq` the dispatch rows, `Option`/`Some`/`Nothing` the walk step), `msgspec` (`Struct`), runtime `identity`/`faults`/`lanes`/`resilience`, `artifacts.core.plan`/`core.receipt`/`package.bundle`.
- Growth: a new single-blob algorithm is one bundle-page `CompressionAlgo`/`CodecProfile`/`DEFAULT_PROFILE` row, one `pack` arm, one frame decoder, and one `recover` arm; a new tuning knob is one field on the owning bundle-page knob-struct; a new bounded knob value is one `Literal` token plus one arm-scope dispatch row — zero new verb beside `emit`/`unpack`.
- Boundary: no sibling import, no vocabulary re-own, no folder-minted limiter (`OCCT` owns worker-death retry), no receipt-case widening (the flat `Bundle` case carries every codec), no key-over-output mint (`ContentIdentity.of(blob)` as the node key is the deleted form — the output address is lane-admission evidence only). Declined as architecturally incompatible: `FORMAT_ZSTD1_MAGICLESS` (no magic to delimit, breaking the bomb-guarded walk), `multi_decompress_to_buffer` on recovery (materializes every frame, breaking the bounded `_walked` anamorphism), and the `lz4.block dict=` primer (the many-small-similar concern is `trained`'s zstd `FULLDICT` ownership).

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

_DECOMPRESS_CEILING: Final[int] = 1 << 31  # per-frame decompressed-output bomb ceiling
_DECOMPRESS_WINDOW: Final[int] = 1 << 27  # zstd window bound: a huge-window-log frame never allocates before the size guard
_GZIP_PARALLEL_THRESHOLD: Final[int] = 1 << 20  # at/above: gzip_ng_threaded block-fan; below: single-shot — a size disposition, not a flag
_SINGLE_BLOB: Final[frozenset[CompressionAlgo]] = frozenset(
    {CompressionAlgo.ZSTD, CompressionAlgo.LZ4, CompressionAlgo.BROTLI, CompressionAlgo.GZIP}
)
_PACK_LANE: Final[LanePolicy] = LanePolicy(capacity=os.process_cpu_count() or 1)

# --- [TABLES] ---------------------------------------------------------------------------

_ZSTD_DICT: Final[Map[ZstdDictMode, int]] = Map.of_seq([
    ("auto", zstandard.DICT_TYPE_AUTO),
    ("fulldict", zstandard.DICT_TYPE_FULLDICT),
    ("rawcontent", zstandard.DICT_TYPE_RAWCONTENT),
])
# the DEFLATE-matcher strategies fast-to-densest; "auto" absent by design (it skips the from_level override)
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
        # threads the pre-run key so receipt.slot == node.key
        return packed.map(lambda pe: pe[1].receipt(self.bundle.key))

    async def unpack(self, blob: bytes, /) -> RuntimeRail[BundleManifest]:
        rows = await _PACK_LANE.offload(
            Codec.recover, blob, self.bundle.algo, self.bundle.profile, modality=self._modality, retry=RetryClass.OCCT
        )
        return rows.map(lambda recovered: BundleManifest.of(self.bundle.algo, recovered))

    @property
    def _cost(self) -> float:
        return float(sum(map(len, self.bundle.payloads)) or 1)  # byte-volume CPM weight

    @property
    def _modality(self) -> Modality:
        # zstd/zlib-ng release the GIL in-wheel; lz4/brotli cross the worker lane
        return Modality.PROCESS if self.bundle.algo in (CompressionAlgo.LZ4, CompressionAlgo.BROTLI) else Modality.THREAD

    @staticmethod
    def pack(payloads: tuple[bytes, ...], algo: CompressionAlgo, profile: CodecProfile, /) -> tuple[bytes, BundleEvidence]:
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
                # length prefix the `_brotli_frame` walk slices back.
                frames = tuple(brotli.compress(payload, mode=modes[k.mode], quality=k.quality, lgwin=k.lgwin, lgblock=k.lgblock) for payload in payloads)
                blob = b"".join(len(frame).to_bytes(8, "big") + frame for frame in frames)
                return blob, BundleEvidence.measure(algo, k.quality, 0, sum(map(len, payloads)), 0, payloads, (blob,))
            case _:
                raise ValueError(f"<non-codec-profile:{profile.tag}>")  # container/delta tags resolve on siblings; the family is not exhausted here

    @staticmethod
    def recover(blob: bytes, algo: CompressionAlgo, profile: CodecProfile, /) -> tuple[MemberTriple, ...]:
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
    # the threaded writer exposes no mtime knob, so its RFC 1952 mtime field (bytes [4:8]) is overwritten with the
    # fixed stamp — both lanes byte-reproducible.
    if len(payload) < _GZIP_PARALLEL_THRESHOLD:
        return gzip_ng.compress(payload, compresslevel=k.level, mtime=k.mtime)
    sink = BytesIO()
    with gzip_ng_threaded.open(sink, "wb", compresslevel=k.level, threads=k.threads, block_size=k.block_size) as writer:
        writer.write(payload)
    raw = sink.getvalue()
    return raw[:4] + int(k.mtime).to_bytes(4, "little") + raw[8:]


def _walked(blob: bytes, decode: Callable[[bytes], tuple[bytes, bytes]], /) -> tuple[MemberTriple, ...]:
    # each `decode` peels one self-delimiting frame, the payload hashed to xxh3_128 and freed per step.
    def step(seed: tuple[bytes, int], /) -> Option[tuple[MemberTriple, tuple[bytes, int]]]:
        rest, index = seed
        if not rest:
            return Nothing
        payload, tail = decode(rest)
        return Some(((f"payload-{index}", len(payload), xxhash.xxh3_128_digest(payload)), (tail, index + 1)))

    return tuple(Block.unfold(step, (blob, 0)))


def _zstd_frame(trained: "zstandard.ZstdCompressionDict | None", /) -> Callable[[bytes], tuple[bytes, bytes]]:
    # ONE dict-aware decompressor reused across every frame; max_window_size is the second bomb gate the size guard alone cannot cover.
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

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
