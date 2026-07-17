# [PY_ARTIFACTS_CODEC]

`Codec` is the single-blob compression producer over the four codec rows — `ZSTD`/`LZ4`/`BROTLI`/`GZIP`. It packs already-emitted payloads into ONE content-addressed blob of self-delimiting frames and walks the inverse back into a `BundleManifest`, composing the `package/bundle#BUNDLE` vocabulary downward and importing no sibling.

`emit() -> ArtifactWork` carries the producer contract — the node key is `Bundle.key`'s pre-run mint (spec ⊕ parent keys), `Admission(keyed=None)`, and `_emit` threads that key into the terminal `ArtifactReceipt.Bundle` so `receipt.slot == node.key`. `packed()` is the composite-consumer face of the same offload — the sealed blob plus evidence on the rail for a caller (`delivery/transmittal#TRANSMITTAL`) that needs the bytes NOW rather than a plan node — and `_emit` is one `map` over it, so the two entries share one kernel crossing. In-wheel GIL-releasing `zstandard`/`zlib-ng` arms cross `KernelTrait.RELEASING` on the thread arm while the GIL-holding `lz4`/`brotli` arms cross `KernelTrait.HOSTILE` on the warm process pool; `verified` counts each enabled in-band proof (zstd `write_checksum`, lz4 `content_checksum`, gzip trailer CRC) plus the mandatory brotli 8-byte `xxh3_64` sidecar, and every recovery is bomb-bounded against `_DECOMPRESS_CEILING`. Recovery decodes UNTRUSTED input — the one `Enforcement.TERMINAL` class: the `unpack` kernel carries the `_RECOVER_DEADLINE` per-offload budget so a malformed-input wedge dies at wall-clock on the pebble kill arm and reclaims its slot, while `pack` stays cooperative because its payloads are already-trusted emissions.

## [01]-[INDEX]

- [02]-[CODEC]: the `Codec` producer over the four single-blob rows — the `emit`/`packed`/`unpack` node contract, the `PackWorker` port kernels, the `_walked` anamorphic frame walk, and the per-codec end-of-frame decoders.

## [02]-[CODEC]

- Owner: `Codec` the one single-blob producer wrapping the `Bundle` carrier with its `lane: LanePolicy`; the four codec rows resolve here, the container rows on `package/archive#ARCHIVE`, the delta row on `package/delta#DELTA`. `Codec.pack`/`Codec.recover` are the `PackWorker` port kernels: public staticmethods over `(payloads, profile)` — the profile IS the discriminant, its `algo` derived, no second algorithm parameter to contradict it — total over the four rows, module-picklable so the `HOSTILE` crossing ships them `REFERENCE` by qualified name with the profile crossing as data.
- Cases: `pack` discriminates on spread arity, never a `batch` flag — the `ZSTD` arm takes `multi_compress_to_buffer` for a two-or-more spread its `backend_features` advertises, else a per-payload loop serving the singular arm too, tuning through ONE `from_level(...)` object (`compression_params=`, never `level=` beside it — mutually exclusive) that receives only NON-ZERO log/length knobs, because an explicit `0` kwarg replaces the level-derived cparam with the context default and silently downgrades the keyed level. Both `GZIP` lanes emit one byte-reproducible self-delimiting member any stdlib `gzip` reads, and `BROTLI` rides a 16-byte prefix — 8-byte big-endian length plus 8-byte `xxh3_64` payload digest — since its stream is neither concatenation-splittable nor checksummed in-band. `recover` walks each codec's own end-of-frame seam, every decode bomb-bounded against `_DECOMPRESS_CEILING`: the `ZSTD` decode refuses a frame whose declared content size is unknown, erroneous, or over-ceiling BEFORE decompressing, caps `max_window_size=_DECOMPRESS_WINDOW` against a small-declared/huge-window frame, and requires both `eof` and output length equal to the declaration; the `GZIP` decode requires `eof` after the bounded call so a truncated member never masquerades as a recovered payload; the `LZ4` decode splits its two non-`eof` states through `needs_input` — starved input is `<lz4-truncated>`, a capped output the bomb — and normalizes the pre-tail `unused_data` `None` to the empty tail; the `BROTLI` decode counts the first bounded chunk and derives every empty-input drain through `Block.unfold` until `is_finished()`, using `can_accept_more_data()` only to distinguish a drained-but-incomplete stream before proving the sidecar digest. `_zstd_frame(trained)` reuses ONE dict-aware decompressor across frames because a `FULLDICT` frame decoded dictless raises corruption and a `trained` bundle is otherwise unrecoverable by its own manifest walk.
- Entry: `packed` offloads `Codec.pack` as a `Kernel` on the row's trait, `_emit` maps it onto `evidence.receipt(self.bundle.key)`, and `unpack` offloads `Codec.recover` on the same trait row under `Enforcement.TERMINAL` with the `_RECOVER_DEADLINE` budget — untrusted decode is the one kill-enforced class — and maps onto `BundleManifest.of`. `Codec.trained` offloads `zstandard.train_dictionary` as a `RELEASING` kernel, maps the returned dictionary bytes into a `ZSTD` profile, and returns `RuntimeRail[Codec]` — corpus training never blocks an event loop or leaks `ZstdError`.
- Output: `verified` is the real per-member proof count, never a uniform lie — enabled zstd `write_checksum` and lz4 `content_checksum` trailers count when present, gzip trailer CRC and the brotli sidecar digest always count; `frame_size` reads `zstandard.frame_content_size` per zstd frame (sentinel-guarded to zero on the evidence fold), while the other arms sum payload or `content_size` lengths. `Bundle.of` admits the Zstandard and LZ4 parameter bands against their provider bounds, so `pack` forwards the exact keyed level instead of clamping distinct profiles onto identical output.
- Packages: `zstandard` (eager — the core arm and `trained`), `lz4.frame`/`brotli`/`zlib-ng` (lazy — reify at arm scope, in the worker process for the `HOSTILE` rows), `xxhash` (`xxh3_128_digest` the walk digests, `xxh3_64_digest` the brotli sidecar), `expression` (`Block.unfold` the anamorphism, `Map.of_seq` the dispatch rows, `Option`/`Some`/`Nothing` the walk step), `msgspec` (`Struct`), runtime `identity`/`faults`/`lanes`/`resilience`, `rasm.artifacts.core.plan`/`core.receipt`/`package.bundle`.
- Growth: a new single-blob algorithm is one bundle-page `CompressionAlgo`/`CodecProfile`/`ALGO_OF`/`DEFAULT_PROFILE` row, one `pack` arm, one frame decoder, and one `recover` arm; a new tuning knob is one field on the owning bundle-page knob-struct; a new bounded knob value is one `Literal` token plus one arm-scope dispatch row — zero new verb beside `emit`/`packed`/`unpack`.
- Boundary: no sibling import, no vocabulary re-own, no folder-minted limiter (the kernel's trait row owns worker-death retry), no receipt-case widening (the flat `Bundle` case carries every codec), no key-over-output mint (`ContentIdentity.key(blob)` as the node key is the deleted form — the output address is lane-admission evidence only), no streaming ingress (payloads are already-emitted bytes; the store crosses at the content-keyed wire). Declined as architecturally incompatible: `FORMAT_ZSTD1_MAGICLESS` (no magic to delimit, breaking the bomb-guarded walk), `multi_decompress_to_buffer` on recovery (materializes every frame, breaking the bounded `_walked` anamorphism), the `lz4.block dict=` primer (the many-small-similar concern is `trained`'s zstd `FULLDICT` ownership), and `zlib_ng.crc32_combine` (the threaded gzip writer recombines its own block trailer internally; a codec-side re-combination re-derives what the writer already owns).

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
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
from rasm.runtime.lanes import LanePolicy
from rasm.runtime.workers import Enforcement, Kernel, KernelTrait

from rasm.artifacts.core.plan import Admission, ArtifactWork
from rasm.artifacts.core.receipt import ArtifactReceipt
from rasm.artifacts.package.bundle import (
    BrotliKnobs,
    Bundle,
    BundleEvidence,
    BundleManifest,
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
_DECOMPRESS_STEP: Final[int] = 1 << 26  # brotli per-call drain slice: peak single-call allocation, never the whole ceiling
_GZIP_PARALLEL_THRESHOLD: Final[int] = 1 << 20  # at/above: gzip_ng_threaded block-fan; below: single-shot — a size disposition, not a flag
_RECOVER_DEADLINE: Final[float] = 120.0  # untrusted-decode wall-clock: TERMINAL kills a malformed-input wedge here and reclaims the slot
_SINGLE_BLOB: Final[frozenset[CompressionAlgo]] = frozenset(
    {CompressionAlgo.ZSTD, CompressionAlgo.LZ4, CompressionAlgo.BROTLI, CompressionAlgo.GZIP}
)
# --- [TABLES] ---------------------------------------------------------------------------

_ZSTD_DICT: Final[Map[ZstdDictMode, int]] = Map.of_seq([
    ("auto", zstandard.DICT_TYPE_AUTO),
    ("fulldict", zstandard.DICT_TYPE_FULLDICT),
    ("rawcontent", zstandard.DICT_TYPE_RAWCONTENT),
])
# DEFLATE-matcher strategies fast-to-densest; "auto" absent by design (it skips the from_level override)
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
    # `lane` arrives projected via LanePolicy.of(context) at the composition root — a capacity literal has no owner.
    bundle: Bundle
    lane: LanePolicy

    @staticmethod
    def of(
        selector: CompressionAlgo | CodecProfile, *payloads: bytes, lane: LanePolicy, parents: tuple[ContentKey, ...] = ()
    ) -> "Codec":
        # lane selection rides the public factory — execution policy configurable without direct construction.
        built = Bundle.of(selector, *payloads, parents=parents)
        if built.algo not in _SINGLE_BLOB:
            raise ValueError(f"<non-codec-algo:{built.algo}>")
        return Codec(bundle=built, lane=lane)

    @staticmethod
    async def trained(
        corpus: tuple[bytes, ...], *payloads: bytes, lane: LanePolicy, level: int = 19, dict_size: int = 112_640, parents: tuple[ContentKey, ...] = ()
    ) -> RuntimeRail["Codec"]:
        trained = await lane.offload(Kernel.of(zstandard.train_dictionary, KernelTrait.RELEASING), dict_size, list(corpus))
        return trained.map(
            lambda dictionary: Codec.of(
                CodecProfile(zstd=ZstdKnobs(level=level, dict_data=dictionary.as_bytes(), dict_mode="fulldict")),
                *payloads,
                lane=lane,
                parents=parents,
            )
        )

    def emit(self, /) -> ArtifactWork:
        return ArtifactWork(key=self.bundle.key, work=self._emit, parents=self.bundle.parents, admission=Admission(keyed=None), cost=self._cost)

    async def packed(self, /) -> RuntimeRail[tuple[bytes, BundleEvidence]]:
        return await self.lane.offload(Kernel.of(Codec.pack, self._trait), self.bundle.payloads, self.bundle.profile)

    async def _emit(self, /) -> RuntimeRail[ArtifactReceipt]:
        return (await self.packed()).map(lambda pe: pe[1].receipt(self.bundle.key))

    async def unpack(self, blob: bytes, /) -> RuntimeRail[BundleManifest]:
        # recover decodes UNTRUSTED input — the one TERMINAL class: the deadline rides the pebble kill so a
        # malformed-input wedge dies at wall-clock and reclaims its slot; pack stays cooperative on trusted payloads.
        kernel = Kernel.of(Codec.recover, self._trait, deadline=Some(_RECOVER_DEADLINE), enforcement=Enforcement.TERMINAL)
        rows = await self.lane.offload(kernel, blob, self.bundle.profile)
        return rows.map(lambda recovered: BundleManifest.of(self.bundle.algo, recovered))

    @property
    def _cost(self) -> float:
        return float(sum(map(len, self.bundle.payloads)) or 1)

    @property
    def _trait(self) -> KernelTrait:
        # lz4/brotli hold the GIL through their native cores, so they earn the process pool; zstd/zlib-ng release it.
        return KernelTrait.HOSTILE if self.bundle.algo in (CompressionAlgo.LZ4, CompressionAlgo.BROTLI) else KernelTrait.RELEASING

    @staticmethod
    def pack(payloads: tuple[bytes, ...], profile: CodecProfile, /) -> tuple[bytes, BundleEvidence]:
        match profile:
            case CodecProfile(tag="zstd", zstd=ZstdKnobs() as k):
                level = k.level
                # a zero knob is WITHHELD, never forwarded: from_level substitutes an explicit kwarg for its level-derived
                # cparam, and an explicit 0 resets that cparam to the context default (compressionLevel 3), silently
                # downgrading the keyed level — live-proven on 0.25.0.
                tuned = {"window_log": k.window_log, "hash_log": k.hash_log, "chain_log": k.chain_log, "target_length": k.target_length}
                overrides = {knob: value for knob, value in tuned.items() if value} | (
                    {"strategy": _ZSTD_STRATEGY[k.strategy]} if k.strategy != "auto" else {}
                )
                params = zstandard.ZstdCompressionParameters.from_level(
                    level, threads=k.threads, enable_ldm=k.enable_ldm, write_checksum=k.write_checksum, write_content_size=True, **overrides
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
                return blob, BundleEvidence.measure(profile.algo, level, dict_id, sum(map(_declared, frames)), verified, payloads, blob)
            case CodecProfile(tag="gzip", gzip=GzipKnobs() as k):
                blob = b"".join(_gzip_member(payload, k) for payload in payloads)
                return blob, BundleEvidence.measure(profile.algo, k.level, 0, sum(map(len, payloads)), len(payloads), payloads, blob)
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
                        block_checksum=k.block_checksum,
                        block_linked=k.block_linked,
                        store_size=True,
                    )
                    for payload in payloads
                )
                blob = b"".join(frames)
                verified = sum(lz4.frame.get_frame_info(frame)["content_checksum"] for frame in frames)
                frame_size = sum(lz4.frame.get_frame_info(frame)["content_size"] for frame in frames)
                return blob, BundleEvidence.measure(profile.algo, k.compression_level, 0, frame_size, verified, payloads, blob)
            case CodecProfile(tag="brotli", brotli=BrotliKnobs() as k):
                modes = Map.of_seq([("generic", brotli.MODE_GENERIC), ("text", brotli.MODE_TEXT), ("font", brotli.MODE_FONT)])
                frames = tuple(brotli.compress(payload, mode=modes[k.mode], quality=k.quality, lgwin=k.lgwin, lgblock=k.lgblock) for payload in payloads)
                blob = b"".join(
                    len(frame).to_bytes(8, "big") + xxhash.xxh3_64_digest(payload) + frame for payload, frame in zip(payloads, frames, strict=True)
                )
                return blob, BundleEvidence.measure(profile.algo, k.quality, 0, sum(map(len, payloads)), len(payloads), payloads, blob)
            case _:
                raise ValueError(f"<non-codec-profile:{profile.tag}>")

    @staticmethod
    def recover(blob: bytes, profile: CodecProfile, /) -> tuple[MemberTriple, ...]:
        match profile:
            case CodecProfile(tag="zstd", zstd=ZstdKnobs() as k):
                trained = zstandard.ZstdCompressionDict(k.dict_data, dict_type=_ZSTD_DICT[k.dict_mode]) if k.dict_data is not None else None
                return _walked(blob, _zstd_frame(trained))
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
    return size if size >= 0 else 0  # evidence fold only: CONTENTSIZE_UNKNOWN/CONTENTSIZE_ERROR never poison the sum


def _gzip_member(payload: bytes, k: GzipKnobs, /) -> bytes:
    # threaded writer exposes no mtime knob, so its RFC 1952 mtime field (bytes [4:8]) is overwritten with the
    # fixed stamp — both lanes byte-reproducible.
    if len(payload) < _GZIP_PARALLEL_THRESHOLD:
        return gzip_ng.compress(payload, compresslevel=k.level, mtime=k.mtime)
    sink = BytesIO()
    with gzip_ng_threaded.open(sink, "wb", compresslevel=k.level, threads=k.threads, block_size=k.block_size) as writer:
        writer.write(payload)
    raw = sink.getvalue()
    return raw[:4] + k.mtime.to_bytes(4, "little") + raw[8:]


def _walked(blob: bytes, decode: Callable[[bytes], tuple[bytes, bytes]], /) -> tuple[MemberTriple, ...]:
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
        declared = zstandard.frame_content_size(frame)
        if not 0 <= declared <= _DECOMPRESS_CEILING:
            raise ValueError("<decompression-bomb>")
        obj = decompressor.decompressobj(read_across_frames=False)
        payload = obj.decompress(frame)
        if not obj.eof or len(payload) != declared:
            raise ValueError("<zstd-truncated>")
        return payload, obj.unused_data

    return decode


def _gzip_frame(frame: bytes, /) -> tuple[bytes, bytes]:
    obj = zlib_ng.decompressobj(wbits=31)
    payload = obj.decompress(frame, _DECOMPRESS_CEILING)
    if obj.unconsumed_tail:
        raise ValueError("<decompression-bomb>")
    if not obj.eof:
        raise ValueError("<gzip-truncated>")
    return payload, obj.unused_data


def _lz4_frame(frame: bytes, /) -> tuple[bytes, bytes]:
    # pre-eof `needs_input` splits the two non-eof states: True = truncated input, False = output capped mid-frame (bomb);
    # `unused_data` is None until the frame completes with a tail, so the empty tail normalizes.
    decoder = lz4.frame.LZ4FrameDecompressor()
    payload = decoder.decompress(frame, max_length=_DECOMPRESS_CEILING)
    if not decoder.eof:
        raise ValueError("<lz4-truncated>" if decoder.needs_input else "<decompression-bomb>")
    return payload, decoder.unused_data or b""


def _brotli_frame(frame: bytes, /) -> tuple[bytes, bytes]:
    if len(frame) < 16:
        raise ValueError("<brotli-header>")
    length = int.from_bytes(frame[:8], "big")
    if length > len(frame) - 16:
        raise ValueError("<brotli-truncated>")
    proof, body, tail = frame[8:16], frame[16 : 16 + length], frame[16 + length :]
    decoder = brotli.Decompressor()
    first = decoder.process(body, output_buffer_limit=_DECOMPRESS_STEP)
    if len(first) > _DECOMPRESS_CEILING:
        raise ValueError("<decompression-bomb>")

    def drain(total: int, /) -> Option[tuple[bytes, int]]:
        if decoder.is_finished():
            return Nothing
        chunk = decoder.process(b"", output_buffer_limit=_DECOMPRESS_STEP)
        measured = total + len(chunk)
        if measured > _DECOMPRESS_CEILING:
            raise ValueError("<decompression-bomb>")
        if not chunk and decoder.can_accept_more_data():
            raise ValueError("<brotli-truncated>")
        return Some((chunk, measured))

    payload = b"".join((first, *Block.unfold(drain, len(first))))
    if xxhash.xxh3_64_digest(payload) != proof:
        raise ValueError("<brotli-integrity>")
    return payload, tail
```
