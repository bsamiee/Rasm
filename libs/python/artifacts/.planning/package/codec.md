# [PY_ARTIFACTS_CODEC]

The content-addressed single-blob compression owner and the union spine the archive and delta arms compose. `Bundle` packs any emitted artifact into content-addressed compressed bytes AND unpacks/lists/tests the inverse; `CompressionAlgo` is ONE algorithm-row owner discriminating zstandard, lz4, brotli, the shared `zlib-ng` gzip substrate, and — composed from `package/archive#ARCHIVE` and `package/delta#DELTA` — the py7zr/stream-zip container and detools binary-delta arms, never a per-algorithm class family. `CodecProfile` is the ONE policy union carrying one typed knob-struct per codec case — a `ZstdKnobs` (`level`, `threads`, `window_log`, `enable_ldm`, `write_checksum`, `dict_data`, `dict_mode`, plus the `strategy`/`hash_log`/`chain_log`/`target_length` advanced-matcher axes), an `Lz4Knobs` (`compression_level`, `block_size`, `content_checksum`), a `BrotliKnobs` (`mode`, `quality`, `lgwin`, `lgblock`), a `GzipKnobs` (`level`, `mtime`, `threads`, `block_size`), plus the `SevenZKnobs`/`ZipStreamKnobs` archive cases and the `DeltaKnobs` delta case the sibling pages author — so a hardcoded literal beside a codec call is the deleted form, every match arm reads a named field rather than a positional tuple slot, and a latency-versus-ratio swap is one policy value. `Bundle.pack` and `Bundle.unpack` are the one modal-arity inverse verb pair: pack folds payloads into ONE content-addressed blob of self-delimiting frames keyed by the runtime content key, unpack walks the frames back into a `BundleManifest` of `(name, ContentKey, algo, size)` rows; a bundle of identical inputs at an identical algorithm and profile is a cache hit by reference. Every frame carries its own integrity proof — the zstd frame `write_checksum`, the lz4 `content_checksum`, the gzip RFC 1952 trailer CRC — so `verified` is the real per-member integrity count, never a hardcoded zero, and every recovery is bomb-bounded against `_DECOMPRESS_CEILING`. This is the compression close over already-emitted bytes and the inverse recovery of those bytes — it owns no artifact production.

## [01]-[INDEX]

- [02]-[CODEC]: the `Bundle` single-blob compression owner and the union spine — `CompressionAlgo` the closed `StrEnum` over the seven codec rows, `CodecProfile` the one `tagged_union` policy value carrying one typed knob-struct per case (`ZstdKnobs`/`Lz4Knobs`/`BrotliKnobs`/`GzipKnobs` here, the `SevenZKnobs`/`ZipStreamKnobs`/`DeltaKnobs` sibling cases), `BundleManifest`/`ManifestRow` the recovered `(name, ContentKey, algo, size)` row set, `BundleEvidence.measure` the one compression-receipt fold, the `Lz4Block`/`BrotliMode`/`ZstdDictMode` closed `Literal` axes resolved through frozen dispatch rows, the `Bundle.pack`/`unpack` modal-arity inverse verb pair, the `_walked` `Block.unfold` anamorphic frame walk hashing each frame to the runtime `xxh3_128` identity family (the dict-aware `_zstd_frame` decoder factory rehydrating the profile dictionary on recovery so a `Bundle.trained` frame is decodable, the `_gzip_member` size-disposition routing a large payload to the parallel gzip lane), the `_ZSTD_STRATEGY` matcher row, the dual `_CODEC_LIMITER`-bounded offload lanes (`ZSTD`/`GZIP` on `to_thread`, `LZ4`/`BROTLI` on `to_process`), the `_DECOMPRESS_CEILING`/`_DECOMPRESS_WINDOW`/`_GZIP_PARALLEL_THRESHOLD` bomb-and-lane bounds, and the `_in_process`/`_unpack_in_process`/`_worker_codec`/`_worker_unpack` dispatch delegating `seven_z`/`zip_stream`/`delta` to the sibling arms; `zstandard` `ZstdCompressor`/`ZstdCompressionParameters.from_level`(`strategy`/`hash_log`/`chain_log`/`target_length`/`window_log`)/`STRATEGY_*`/`ZstdCompressionDict`(`train_dictionary`/`precompute_compress`)/`multi_compress_to_buffer`(`backend_features`-guarded)/`ZstdDecompressor(dict_data=, max_window_size=)`/`get_frame_parameters`/`frame_content_size`/`MAX_COMPRESSION_LEVEL`/`DICT_TYPE_*`, `lz4` `frame.compress`/`LZ4FrameDecompressor`/`get_frame_info`/`BLOCKSIZE_*`, `brotli` `compress`/`Decompressor`/`MODE_*`, and the shared `zlib-ng` `gzip_ng`/`gzip_ng_threaded`(`threads`/`block_size` block-fan)/`zlib_ng`/`crc32`/`crc32_combine` + `xxhash.xxh3_128_digest` rails settled against the both-tier `.api`, contributing the one `core/receipt#RECEIPT` `ArtifactReceipt.Bundle` case and a `core/plan#PLAN` `ArtifactWork` node.

## [02]-[CODEC]

- Owner: `Bundle` the one bundle owner; `CompressionAlgo` the closed `StrEnum` discriminating algorithm across the seven codec rows; `CodecProfile` the one `tagged_union` policy value carrying one typed knob-struct per algorithm case; `BundleManifest` the one frozen value object recording the `(name, ContentKey, algo, size)` `ManifestRow` sequence the unpack inverse recovers; `BundleEvidence` the typed compression receipt projected once through `measure`. Every knob-struct is a `msgspec.Struct(frozen=True, gc=False)` — all-scalar leaves dropped from the GC-tracked set on the hot pack path, and the msgspec-native shape the worker lane serializes across the subprocess. The algorithm package is bound per row, never an `if zstd` branch. The `SEVEN_Z`/`ZIP_STREAM` cases and their workers live at `package/archive#ARCHIVE` and the `DELTA` case at `package/delta#DELTA`, both COMPOSING this owner's `Bundle`/`CompressionAlgo`/`CodecProfile`/`BundleEvidence` scaffold; the four single-blob codec arms (`ZSTD`/`GZIP` on the core, `LZ4`/`BROTLI` on the worker lane) resolve here.
- Cases: `CompressionAlgo` rows `ZSTD` (zstandard `ZstdCompressor`) · `LZ4` (lz4 `frame`) · `BROTLI` (brotli `compress`) · `GZIP` (`zlib-ng` `gzip_ng` SIMD gzip-container for stdlib interop) resolve on this page; `SEVEN_Z` (py7zr `SevenZipFile`) · `ZIP_STREAM` (stream-zip `stream_zip`/`async_stream_zip`) · `DELTA` (detools `create_patch`/`apply_patch`) resolve on the archive/delta siblings — matched by `match`/`case`, each binding its algorithm package and reading its `CodecProfile` case; `CodecProfile` cases `zstd` (`ZstdKnobs`) · `lz4` (`Lz4Knobs`) · `brotli` (`BrotliKnobs`) · `gzip` (`GzipKnobs`) · `seven_z` (`SevenZKnobs`) · `zip_stream` (`ZipStreamKnobs`) · `delta` (`DeltaKnobs`) — one frozen knob-struct per algorithm row whose fields are named axes the match arm reads directly, the default profile per algorithm a frozen module constant (the `DELTA` case carries no default because `DeltaKnobs` requires a parent `from_image`, minted only through `Bundle.delta`). Every closed knob axis is a `Literal` vocabulary, not a bare ordinal: `Lz4Block`, `BrotliMode`, `ZstdDictMode` carry their package-constant family as named tokens that resolve to the real value through one frozen dispatch row at codec scope, so a string-built `getattr` attribute lookup and a raw `int`/`str` profile field are the deleted forms. The archive `ZipMethod` and the delta `DeltaPatchType`/`DeltaAlgorithm`/`DeltaCompression` axes ride the union but resolve their dispatch rows on the owning sibling page.
- Modality: `Bundle.pack` and `Bundle.unpack` are the one modal-arity inverse verb pair, discriminating on the value's shape rather than a `direction` flag. `pack` folds a singular payload OR a `*payloads` spread of two-or-more into ONE content-addressed blob: the `ZSTD` arm runs the `multi_compress_to_buffer` threaded batch (sharing the trained dictionary across the spread) when the spread carries two-or-more AND `zstandard.backend_features` advertises `multi_compress_to_buffer` — the batch carriers exist only under the `cext` backend, so the arm guards on the feature set (the zstandard `.api` explicitly rejects assuming they exist) and falls to a per-payload `compress` loop that also serves the singular arm under the `cffi` fallback — joining the resulting self-delimiting frames into one blob (the `ZIP_STREAM`/`SEVEN_Z` archive arms fold the spread into one container so a multi-payload bundle is a single archive, never N archives), discriminating on the spread arity recovered from the call, never a `batch` flag; the arity is the value's shape, so a single payload and a corpus fold one entrypoint. `unpack` is the inverse: it consumes the packed blob plus the same `algo`/`profile` and walks the self-delimiting frames back into a `BundleManifest` — the single-frame codecs on this page recover one row per concatenated frame through `_walked` reading each codec's own end-of-frame seam (`ZSTD`/`GZIP` `unused_data`, `LZ4FrameDecompressor.eof`/`unused_data`, brotli a per-member 8-byte length prefix since its stream is not concatenation-splittable), the archive arms recover the full member row set from the container, and the delta arm reconstructs the to-image from the parent. Every frame decode is bomb-bounded — refused when the declared content size exceeds `_DECOMPRESS_CEILING` or the per-call output cap (`max_output_size`/`max_length`/`output_buffer_limit`) is hit before the frame ends, and the `ZSTD` decode additionally bounds `ZstdDecompressor(max_window_size=_DECOMPRESS_WINDOW)` so an adversarial small-declared/huge-window-log frame never forces an oversized window allocation before the size guard fires — so an adversarial bundle never exhausts memory on the recovery path. `_compress` and `_recover` are the single key-mint seams — `_in_process` and the worker each return ONE blob that `_compress` folds the `ContentIdentity.of` projection over once, never per-arm, and `_walked` mints each recovered member key once. The heavy codec body never runs on the event loop: `_compress`/`_recover` offload the in-wheel GIL-releasing arms (`zstandard`/`zlib-ng`/`py7zr`/`detools`) through `to_thread.run_sync` and the worker `lz4`/`brotli` arms through `to_process.run_sync`, every arm bounded by the one shared `_CODEC_LIMITER` `CapacityLimiter` so subsystem offload concurrency is fixed at the boundary, never the per-loop default.
- Dictionary: the `ZSTD` arm reads `dict_data: bytes | None` and `dict_mode: ZstdDictMode` off its `ZstdKnobs` and rehydrates one `ZstdCompressionDict(dict_data, dict_type=_ZSTD_DICT[dict_mode])` at codec scope, so the trained dictionary crosses the subprocess lane as raw bytes rather than an unpicklable native handle and the `DICT_TYPE_AUTO`/`DICT_TYPE_FULLDICT`/`DICT_TYPE_RAWCONTENT` axis threads through the `Literal` token; `Bundle.trained` mines `zstandard.train_dictionary` over a many-similar-artifact corpus and binds the trained `as_bytes()` straight into a `ZSTD` `CodecProfile` at the call site, the dominant small-payload ratio win for receipts, glyph-runs, and repeated chart JSON — driven hardest by the `multi_compress_to_buffer` spread that compresses the whole corpus against the one shared dictionary in one threaded pass, never a parallel dict-codec owner or a rename-only `train` forwarder. After the worker rehydrates the dictionary it calls `ZstdCompressionDict.precompute_compress(compression_params=params)` once so the compression-side dictionary state is cached for the repeated-compress corpus pass (a worker-side precompute, since the dict crosses the subprocess lane as bytes and rehydrates there). `dict_id()` reads inline at the evidence mint, never through a single-call helper. The recovery lane is symmetric and this is load-bearing correctness, not tuning: `_unpack_in_process` rehydrates the SAME `ZstdCompressionDict(dict_data, dict_type=_ZSTD_DICT[dict_mode])` from the profile and threads it into the `_zstd_frame` decoder factory, because a `FULLDICT` frame is compressed against the dictionary and a dictless `ZstdDecompressor` raises "Data corruption detected" — a `Bundle.trained` bundle whose `_walked` manifest pass decodes each frame to hash it is otherwise UNRECOVERABLE, so the dictionary crosses both the pack and unpack lanes and the factory builds one dict-aware decompressor reused across every frame rather than reconstructing one per frame.
- Frame: the `ZSTD` arm tunes through ONE `ZstdCompressionParameters.from_level(level, window_log=, hash_log=, chain_log=, target_length=, strategy=, threads=, enable_ldm=, write_checksum=, write_content_size=True)` derived parameter object passed as `compression_params=` (never `level=` AND `compression_params=` on the same `ZstdCompressor`, which the package rejects as mutually exclusive), so `enable_ldm` lights long-distance matching for large repetitive artifacts, the `window_log`/`hash_log`/`chain_log`/`target_length` matcher axes carry 0 as the from_level auto-sentinel (unset -> level-derived) while a `strategy` token overrides the DEFLATE matcher fast-to-`btultra2` through the `_ZSTD_STRATEGY` row (the "auto" token skipping the override, since `STRATEGY_*` carries no zero member), and `write_content_size=True` guarantees the frame header carries the decompressed size. The receipt `frame_size` sums each frame's `zstandard.frame_content_size`, sentinel-guarded against `CONTENTSIZE_UNKNOWN`/`CONTENTSIZE_ERROR` (negative returns folded to zero) so a header without a declared size never poisons the evidence; the level axis caps at `zstandard.MAX_COMPRESSION_LEVEL`. The `GZIP` arm sums the original payload lengths because the gzip container's RFC 1952 trailer is the size source.
- Integrity: the `verified` evidence field is the real per-member integrity count, never a hardcoded zero. The `ZSTD` arm sets `write_checksum=True` so every frame carries a 4-byte xxhash64 trailer, then counts frames whose `zstandard.get_frame_parameters(frame).has_checksum` is set; the `LZ4` arm reads `lz4.frame.get_frame_info(frame)["content_checksum"]` (the `Lz4Knobs.content_checksum` it sets at pack); the `GZIP` arm counts every member because the RFC 1952 trailer CRC is always present and `gzip_ng` verifies it on decode (raising `BadGzipFile` on mismatch). Only `BROTLI` leaves `verified` zero — the Brotli stream carries no in-band checksum and relies on its transport — so the field honestly reports the codec's own integrity capability rather than a uniform lie. The archive `entries`/`verified` container slots ride the same field through the sibling arms.
- Gzip: the `GZIP` arm composes the shared `data:compression` `zlib-ng` SIMD gzip-container substrate through the one `_gzip_member` size-disposition — a payload below `_GZIP_PARALLEL_THRESHOLD` rides `gzip_ng.compress(payload, compresslevel=level, mtime=mtime)` (the `wbits=31` gzip header+trailer the `_GzipReader` engine writes), a larger one rides `gzip_ng_threaded.open(sink, "wb", threads=-1, block_size=)`, the GIL-escaping block-fan that partitions the payload into `block_size` blocks, compresses each on a `_ParallelCompress` worker (`compress_and_crc` in one GIL-released call), and recombines the per-block CRCs into the RFC 1952 trailer via `crc32_combine` — all owned INSIDE the threaded writer, never a hand-rolled partition/pool — so a large-artifact gzip write parallelizes even nested inside the `to_thread` offload while a small one stays single-shot, a disposition on the value's size rather than a mode flag. The threaded writer exposes no mtime knob, so `_gzip_member` overwrites the RFC 1952 header mtime field (bytes `[4:8]`) with the fixed `int(mtime)` little-endian stamp, keeping the member byte-reproducible for the content-addressing dedup exactly as the single-shot `mtime=` path does. Both lanes emit ONE self-delimiting gzip member per payload, so each round-trips with any stdlib `gzip` peer at accelerated throughput and a multi-payload bundle concatenates the self-delimiting members into one valid multi-member gzip blob. `unpack` walks members through `zlib_ng.decompressobj(wbits=31)` reading each member's `unused_data` tail (NOT `gzip_ng.decompress`, which concatenates members and erases their boundaries), so the member row set recovers bounded; the substrate is COMPOSED, not re-admitted (the `zlib-ng` row already lives in the manifest as the `data` owner's accelerated DEFLATE/gzip codec), and the bundle owner reaches it for exactly the gzip-interop case a non-legacy payload routes away from to `zstandard` (archival) or `lz4` (hot path). The level axis is the standard zlib `0..9` range (`Z_BEST_COMPRESSION = 9`), never an isal-style `0..3` assumption. The same `zlib_ng.crc32` rolling-CRC surface this substrate exposes is the integrity seed the `package/archive#ARCHIVE` stored-ZIP member and streamed-unzip member folds consume — one substrate, no second CRC path.
- Receipt: each pack contributes `core/receipt#RECEIPT` `ArtifactReceipt.Bundle` carrying the content key and the `BundleEvidence` facts — algo, level, dictionary id, frame size, member count, integrity-verified count, ratio — projected through the one `BundleEvidence.measure` constructor that folds member count and `in_bytes`/`out_bytes` once; `_emit` spreads the named evidence fields onto the flat-scalar `ArtifactReceipt.Bundle` case, and the receipt owner's own `_facts` arm is the single string-map projector for the whole union, so `BundleEvidence` carries no second `facts` projection and the receipt owner imports no `BundleEvidence` value object (the receipt-side circular import the flattening forecloses). The `ratio` is the one observable-compression value the runtime `observability/metrics` `MeterProvider` compression-ratio signal stream reads off the receipt fold. The `measure` projector folds `entries` from the payload count and `in_bytes`/`out_bytes` from the payload and blob spans once, so the member-count, input-sum, and output-sum construction lives on one row rather than re-derived per codec arm — the same fold the archive arms reuse, which is why their `entries` is the member count rather than the blob count. The `core/receipt#RECEIPT` `Bundle` case (`tuple[ContentKey, str, int, int, int, int, int, float]`, eight fields, verified) already carries this evidence; every algorithm row reuses it unchanged — no receipt-case widening lands on the admission owner.
- Growth: a new single-blob algorithm is one `CompressionAlgo` row, one `CodecProfile` case with its knob-struct, one default-profile constant, one `_in_process` (or worker) pack arm, and one `_walked` frame decoder; a new container/delta algorithm grows the archive/delta sibling page composing this scaffold; a new tuning knob is one field on the owning knob-struct, a new bounded knob value one token on the owning `Literal` axis plus one dispatch-row entry; a new evidence fact one `(label, value)` row in `facts`; a new manifest column one field on `ManifestRow`; zero new surface and zero new verb beside the `pack`/`unpack` inverse.

```python signature
import os
from enum import StrEnum
from typing import Final, Literal

import msgspec
import zstandard
from anyio import CapacityLimiter, to_process, to_thread
from builtins import frozendict
from expression import case, tag, tagged_union
from msgspec import Struct, msgpack

from rasm.runtime.content_identity import ContentIdentity, ContentKey
from rasm.runtime.faults import RuntimeRail, async_boundary

from artifacts.core.receipt import ArtifactReceipt
from artifacts.package.archive import SevenZKnobs, ZipStreamKnobs, _archive_in_process, _archive_unpack
from artifacts.package.delta import DeltaKnobs, DeltaPatchType, DeltaAlgorithm, DeltaCompression, _delta_in_process, _delta_unpack

type Lz4Block = Literal["default", "max64kb", "max256kb", "max1mb", "max4mb"]
type BrotliMode = Literal["generic", "text", "font"]
type ZstdDictMode = Literal["auto", "fulldict", "rawcontent"]
type ZstdStrategy = Literal["auto", "fast", "dfast", "greedy", "lazy", "lazy2", "btlazy2", "btopt", "btultra", "btultra2"]

# bounds the codec offload subsystem so a heavy native body never runs on the event loop: the in-wheel
# GIL-releasing arms (zstandard/zlib-ng/py7zr/detools) ride `to_thread`, the worker `lz4`/`brotli`
# ride `to_process`, both capped by this one limiter so subsystem concurrency is fixed at the boundary,
# never the per-loop default (40-token thread, cpu-count process).
_CODEC_LIMITER: Final[CapacityLimiter] = CapacityLimiter(os.process_cpu_count() or 1)


class CompressionAlgo(StrEnum):
    ZSTD = "zstd"
    LZ4 = "lz4"
    BROTLI = "brotli"
    SEVEN_Z = "7z"
    ZIP_STREAM = "zip-stream"
    DELTA = "delta"
    GZIP = "gzip"


class ZstdKnobs(Struct, frozen=True, gc=False):
    level: int = 19
    threads: int = -1
    window_log: int = 0
    enable_ldm: bool = False
    write_checksum: bool = True
    dict_data: bytes | None = None
    dict_mode: ZstdDictMode = "auto"
    strategy: ZstdStrategy = "auto"  # the DEFLATE matcher axis; "auto" lets from_level derive it (STRATEGY_* has no zero member), a token overrides
    hash_log: int = 0                # 0 is the from_level auto-sentinel: unset -> level-derived, a value overrides the match-hash-table log
    chain_log: int = 0               # 0 auto-sentinel: the match-chain log for the btlazy2/btopt/btultra strategies
    target_length: int = 0           # 0 auto-sentinel: the minimum match length the btopt/btultra strategies emit


class Lz4Knobs(Struct, frozen=True, gc=False):
    compression_level: int = 0
    block_size: Lz4Block = "default"
    content_checksum: bool = True


class BrotliKnobs(Struct, frozen=True, gc=False):
    mode: BrotliMode = "generic"
    quality: int = 11
    lgwin: int = 22
    lgblock: int = 0


class GzipKnobs(Struct, frozen=True, gc=False):
    level: int = 9
    mtime: float = 0.0
    threads: int = -1      # the gzip_ng_threaded block-fan worker count for the large-payload lane; -1 = all logical cores
    block_size: int = 1 << 20  # per-block queue size the threaded writer fans across workers (only the >= _GZIP_PARALLEL_THRESHOLD lane)


@tagged_union(frozen=True)
class CodecProfile:
    tag: Literal["zstd", "lz4", "brotli", "seven_z", "zip_stream", "delta", "gzip"] = tag()
    zstd: ZstdKnobs = case()
    lz4: Lz4Knobs = case()
    brotli: BrotliKnobs = case()
    seven_z: SevenZKnobs = case()
    zip_stream: ZipStreamKnobs = case()
    delta: DeltaKnobs = case()
    gzip: GzipKnobs = case()


DEFAULT_PROFILE: Final[frozendict[CompressionAlgo, CodecProfile]] = frozendict({
    CompressionAlgo.ZSTD: CodecProfile(zstd=ZstdKnobs()),
    CompressionAlgo.LZ4: CodecProfile(lz4=Lz4Knobs()),
    CompressionAlgo.BROTLI: CodecProfile(brotli=BrotliKnobs()),
    CompressionAlgo.SEVEN_Z: CodecProfile(seven_z=SevenZKnobs()),
    CompressionAlgo.ZIP_STREAM: CodecProfile(zip_stream=ZipStreamKnobs()),
    CompressionAlgo.GZIP: CodecProfile(gzip=GzipKnobs()),
})


class ManifestRow(Struct, frozen=True):
    name: str
    key: ContentKey
    algo: CompressionAlgo
    size: int


class BundleManifest(Struct, frozen=True):
    algo: CompressionAlgo
    rows: tuple[ManifestRow, ...]

    @property
    def entries(self) -> int:
        return len(self.rows)

    @staticmethod
    def of(algo: CompressionAlgo, recovered: tuple[tuple[str, int, bytes], ...]) -> "BundleManifest":
        return BundleManifest(
            algo,
            tuple(ManifestRow(name, ContentIdentity.of(f"member-{algo}", digest), algo, size) for name, size, digest in recovered),
        )


class BundleEvidence(Struct, frozen=True, gc=False):
    algo: CompressionAlgo
    level: int
    dict_id: int
    frame_size: int
    entries: int
    verified: int
    in_bytes: int
    out_bytes: int

    @property
    def ratio(self) -> float:
        return self.out_bytes / self.in_bytes if self.in_bytes else 1.0

    @staticmethod
    def measure(algo: CompressionAlgo, level: int, dict_id: int, frame_size: int, verified: int, payloads: tuple[bytes, ...], blobs: tuple[bytes, ...]) -> "BundleEvidence":
        # entries is the member count (payload arity), never the joined-blob count;
        # the archive arms reuse this fold so their container `entries` is also the member count.
        return BundleEvidence(algo, level, dict_id, frame_size, len(payloads), verified, sum(map(len, payloads)), sum(map(len, blobs)))


class Bundle(Struct, frozen=True):
    payloads: tuple[bytes, ...]
    algo: CompressionAlgo
    profile: CodecProfile

    @staticmethod
    def of(algo: CompressionAlgo, *payloads: bytes, profile: CodecProfile | None = None) -> "Bundle":
        return Bundle(payloads=payloads, algo=algo, profile=profile if profile is not None else DEFAULT_PROFILE[algo])

    @staticmethod
    def trained(corpus: tuple[bytes, ...], *payloads: bytes, level: int = 19, dict_size: int = 112_640) -> "Bundle":
        dict_data = zstandard.train_dictionary(dict_size, list(corpus))  # COVER-trains a fulldict; `dict_type` is a ZstdCompressionDict ctor axis, not a train_dictionary kwarg
        return Bundle.of(CompressionAlgo.ZSTD, *payloads, profile=CodecProfile(zstd=ZstdKnobs(level=level, dict_data=dict_data.as_bytes(), dict_mode="fulldict")))

    @staticmethod
    def delta(from_image: bytes, parent_key: ContentKey, payload: bytes, *, patch_type: DeltaPatchType = "sequential", algorithm: DeltaAlgorithm = "bsdiff", compression: DeltaCompression = "zstd") -> "Bundle":
        return Bundle.of(CompressionAlgo.DELTA, payload, profile=CodecProfile(delta=DeltaKnobs(from_image=from_image, parent_key=parent_key, patch_type=patch_type, algorithm=algorithm, compression=compression)))

    async def pack(self) -> RuntimeRail[tuple[ContentKey, ArtifactReceipt]]:
        return await async_boundary(f"bundle.{self.algo}", self._emit)

    async def unpack(self, blob: bytes) -> RuntimeRail[BundleManifest]:
        return await async_boundary(f"unbundle.{self.algo}", lambda: self._recover(blob))

    async def _emit(self) -> tuple[ContentKey, ArtifactReceipt]:
        key, evidence = await self._compress()
        return key, ArtifactReceipt.Bundle(key, evidence.algo.value, evidence.level, evidence.dict_id, evidence.frame_size, evidence.entries, evidence.verified, evidence.ratio)

    async def _compress(self) -> tuple[ContentKey, BundleEvidence]:
        match self.profile:
            case CodecProfile(tag="lz4", lz4=knobs) | CodecProfile(tag="brotli", brotli=knobs):
                blob, evidence_blob = await to_process.run_sync(_worker_codec, self.algo.value, msgpack.encode(knobs), self.payloads, limiter=_CODEC_LIMITER)
                evidence = msgpack.decode(evidence_blob, type=BundleEvidence)
            case _:  # zstd/gzip native + py7zr/stream-zip/detools all release the GIL: offload the synchronous body off the event loop onto a bounded thread, never inline
                blob, evidence = await to_thread.run_sync(_in_process, self.payloads, self.algo, self.profile, limiter=_CODEC_LIMITER)
        return ContentIdentity.of(f"bundle-{self.algo}", blob), evidence

    async def _recover(self, blob: bytes) -> BundleManifest:
        match self.profile:
            case CodecProfile(tag="lz4") | CodecProfile(tag="brotli"):
                recovered = await to_process.run_sync(_worker_unpack, self.algo.value, blob, limiter=_CODEC_LIMITER)
            case _:  # the 7z streamed-decode recovery and the frame walks release the GIL: bounded-thread offload, never inline
                recovered = await to_thread.run_sync(_unpack_in_process, blob, self.algo, self.profile, limiter=_CODEC_LIMITER)
        return BundleManifest.of(self.algo, recovered)
```

```python signature
from collections.abc import Callable
from io import BytesIO
from typing import Final, assert_never

import msgspec
import xxhash
import zstandard
from builtins import frozendict
from expression import Nothing, Option, Some
from expression.collections import Block

lazy import brotli
lazy import lz4.frame
lazy from zlib_ng import gzip_ng, gzip_ng_threaded, zlib_ng

_DECOMPRESS_CEILING: Final[int] = 1 << 31  # per-frame decompressed-output bomb ceiling; a declared or actual size above it is refused
_DECOMPRESS_WINDOW: Final[int] = 1 << 27   # zstd decompressor window bound: an adversarial small-declared/huge-window-log frame never forces an oversized window allocation BEFORE the size guard fires
_GZIP_PARALLEL_THRESHOLD: Final[int] = 1 << 20  # payloads at/above this route to the GIL-escaping gzip_ng_threaded block-fan; below it the single-shot gzip_ng.compress — a disposition on the value's size, never a mode flag


def _declared(frame: bytes, /) -> int:
    size = zstandard.frame_content_size(frame)
    return size if size >= 0 else 0  # CONTENTSIZE_UNKNOWN/CONTENTSIZE_ERROR fold to zero, never poison the evidence


def _gzip_member(payload: bytes, k: GzipKnobs, /) -> bytes:
    # size-disposition on the value, never a mode flag: a payload at/above the threshold rides the GIL-escaping
    # gzip_ng_threaded block-fan (each `block_size` block a `_ParallelCompress` worker, the per-block CRCs recombined into
    # the RFC 1952 trailer via crc32_combine — all owned INSIDE the threaded writer, never a hand-rolled partition/pool),
    # a smaller one the single-shot gzip_ng.compress; both emit ONE self-delimiting gzip member the `_gzip_frame` walk
    # recovers unchanged. The threaded writer exposes no mtime knob, so its RFC 1952 header mtime field (bytes [4:8]) is
    # overwritten with the fixed `int(k.mtime)` little-endian stamp to keep the member byte-reproducible (the single-shot
    # path already honours mtime), preserving the "identical inputs at identical profile = cache hit by reference" seam.
    if len(payload) < _GZIP_PARALLEL_THRESHOLD:
        return gzip_ng.compress(payload, compresslevel=k.level, mtime=k.mtime)
    sink = BytesIO()
    with gzip_ng_threaded.open(sink, "wb", compresslevel=k.level, threads=k.threads, block_size=k.block_size) as writer:
        writer.write(payload)
    raw = sink.getvalue()
    return raw[:4] + int(k.mtime).to_bytes(4, "little") + raw[8:]


def _in_process(payloads: tuple[bytes, ...], algo: CompressionAlgo, profile: CodecProfile) -> tuple[bytes, BundleEvidence]:
    match profile:
        case CodecProfile(tag="zstd", zstd=ZstdKnobs() as k):
            level = min(k.level, zstandard.MAX_COMPRESSION_LEVEL)
            # the advanced matcher axes ride from_level's override tail: window_log/hash_log/chain_log/target_length carry
            # 0 as the from_level auto-sentinel (unset -> level-derived), while strategy skips the override entirely on
            # "auto" because STRATEGY_* carries no zero member — ONE derived ZstdCompressionParameters, never a raw-int spread.
            strategy = {"strategy": _ZSTD_STRATEGY[k.strategy]} if k.strategy != "auto" else {}
            params = zstandard.ZstdCompressionParameters.from_level(level, window_log=k.window_log, hash_log=k.hash_log, chain_log=k.chain_log, target_length=k.target_length, threads=k.threads, enable_ldm=k.enable_ldm, write_checksum=k.write_checksum, write_content_size=True, **strategy)
            trained = zstandard.ZstdCompressionDict(k.dict_data, dict_type=_ZSTD_DICT[k.dict_mode]) if k.dict_data is not None else None
            if trained is not None:
                trained.precompute_compress(compression_params=params)  # cache the compression-side dict state worker-side for the repeated-compress corpus pass
            compressor = zstandard.ZstdCompressor(dict_data=trained, compression_params=params)
            # the batch carriers exist only under the cext backend: guard `multi_compress_to_buffer` on `backend_features`
            # (the cffi fallback omits it), falling to a per-payload `compress` loop that also serves the singular arm.
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
        case CodecProfile(tag="seven_z") | CodecProfile(tag="zip_stream"):
            (blob,), evidence = _archive_in_process(payloads, algo, profile)
            return blob, evidence
        case CodecProfile(tag="delta"):
            (blob,), evidence = _delta_in_process(payloads, algo, profile)
            return blob, evidence
        case CodecProfile(tag="lz4") | CodecProfile(tag="brotli"):
            raise ValueError(f"<worker-off-core:{profile.tag}>")  # lz4/brotli resolve on the subprocess lane, never the core dispatch
        case _:
            assert_never(profile)


def _unpack_in_process(blob: bytes, algo: CompressionAlgo, profile: CodecProfile) -> tuple[tuple[str, int, bytes], ...]:
    match profile:
        case CodecProfile(tag="zstd", zstd=ZstdKnobs() as k):
            trained = zstandard.ZstdCompressionDict(k.dict_data, dict_type=_ZSTD_DICT[k.dict_mode]) if k.dict_data is not None else None
            return _walked(blob, _zstd_frame(trained))  # dict-aware recovery: a Bundle.trained fulldict frame is unrecoverable without its dictionary
        case CodecProfile(tag="gzip"):
            return _walked(blob, _gzip_frame)
        case CodecProfile(tag="seven_z") | CodecProfile(tag="zip_stream"):
            return _archive_unpack(blob, algo, profile)
        case CodecProfile(tag="delta"):
            return _delta_unpack(blob, algo, profile)
        case CodecProfile(tag="lz4") | CodecProfile(tag="brotli"):
            raise ValueError(f"<worker-off-core:{profile.tag}>")
        case _:
            assert_never(profile)


def _walked(blob: bytes, decode: Callable[[bytes], tuple[bytes, bytes]], /) -> tuple[tuple[str, int, bytes], ...]:
    # anamorphic frame walk: each `decode` peels one self-delimiting frame, the payload hashed to a 16-byte
    # xxh3_128 content digest (the runtime XXH3_128 identity family `ContentIdentity.of` uses, faster on the hot
    # walk than sha256) and freed per step so the manifest walk never buffers every decompressed member —
    # the uniform `(name, size, digest)` triple every recovery arm yields into `BundleManifest.of`.
    def step(seed: tuple[bytes, int], /) -> Option[tuple[tuple[str, int, bytes], tuple[bytes, int]]]:
        rest, index = seed
        if not rest:
            return Nothing
        payload, tail = decode(rest)
        return Some(((f"payload-{index}", len(payload), xxhash.xxh3_128_digest(payload)), (tail, index + 1)))

    return tuple(Block.unfold(step, (blob, 0)))


def _zstd_frame(trained: "zstandard.ZstdCompressionDict | None", /) -> Callable[[bytes], tuple[bytes, bytes]]:
    # ONE dict-aware decompressor built once and reused across every frame of the walk. The dict thread is load-bearing
    # correctness, not tuning: a Bundle.trained frame is compressed against a FULLDICT dictionary and cannot decode
    # without it — a dictless ZstdDecompressor raises "Data corruption detected" (assay-confirmed on the trained frame),
    # so the recovery rehydrates the SAME dictionary the pack arm bound rather than minting an unrecoverable manifest.
    # The `max_window_size` bound is the second bomb gate the declared-size guard alone cannot cover: an adversarial
    # small-declared/huge-window-log frame would force an oversized window allocation before decode; the two gates contain it.
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


def _worker_codec(algo: str, knob_blob: bytes, payloads: tuple[bytes, ...]) -> tuple[bytes, bytes]:
    match algo:
        case "lz4":
            k = msgspec.msgpack.decode(knob_blob, type=Lz4Knobs)
            block_size = {"default": lz4.frame.BLOCKSIZE_DEFAULT, "max64kb": lz4.frame.BLOCKSIZE_MAX64KB, "max256kb": lz4.frame.BLOCKSIZE_MAX256KB, "max1mb": lz4.frame.BLOCKSIZE_MAX1MB, "max4mb": lz4.frame.BLOCKSIZE_MAX4MB}[k.block_size]
            frames = tuple(lz4.frame.compress(payload, compression_level=k.compression_level, block_size=block_size, content_checksum=k.content_checksum, store_size=True) for payload in payloads)
            blob = b"".join(frames)
            verified = sum(lz4.frame.get_frame_info(frame)["content_checksum"] for frame in frames)
            frame_size = sum(lz4.frame.get_frame_info(frame)["content_size"] for frame in frames)
            return blob, msgspec.msgpack.encode(BundleEvidence.measure(CompressionAlgo.LZ4, k.compression_level, 0, frame_size, verified, payloads, (blob,)))
        case "brotli":
            k = msgspec.msgpack.decode(knob_blob, type=BrotliKnobs)
            mode = {"generic": brotli.MODE_GENERIC, "text": brotli.MODE_TEXT, "font": brotli.MODE_FONT}[k.mode]
            # a Brotli stream is NOT self-delimiting on concatenation (the decoder has no `unused_data` and faults on a
            # concatenated blob), so each member rides an 8-byte big-endian compressed-length prefix the `_brotli_frame`
            # walk slices back — the whole `*payloads` spread folds into one multi-member blob, never silently keeping payloads[0].
            frames = tuple(brotli.compress(payload, mode=mode, quality=k.quality, lgwin=k.lgwin, lgblock=k.lgblock) for payload in payloads)
            blob = b"".join(len(frame).to_bytes(8, "big") + frame for frame in frames)
            return blob, msgspec.msgpack.encode(BundleEvidence.measure(CompressionAlgo.BROTLI, k.quality, 0, sum(map(len, payloads)), 0, payloads, (blob,)))
        case _:
            raise ValueError(f"<not-worker:{algo}>")


def _worker_unpack(algo: str, blob: bytes) -> tuple[tuple[str, int, bytes], ...]:
    match algo:
        case "lz4":
            return _walked(blob, _lz4_frame)
        case "brotli":
            return _walked(blob, _brotli_frame)
        case _:
            raise ValueError(f"<not-worker:{algo}>")


def _lz4_frame(frame: bytes, /) -> tuple[bytes, bytes]:
    decoder = lz4.frame.LZ4FrameDecompressor()
    payload = decoder.decompress(frame, max_length=_DECOMPRESS_CEILING)
    if not decoder.eof:
        raise ValueError("<decompression-bomb>")
    return payload, decoder.unused_data


def _brotli_frame(frame: bytes, /) -> tuple[bytes, bytes]:
    length = int.from_bytes(frame[:8], "big")  # the 8-byte member-length prefix isolates one Brotli stream (the decoder cannot split a concatenation)
    body, tail = frame[8 : 8 + length], frame[8 + length :]
    decoder = brotli.Decompressor()
    payload = decoder.process(body, output_buffer_limit=_DECOMPRESS_CEILING)  # output cap bombs-bounds; the prefix already bounds the input span
    if not decoder.is_finished():
        raise ValueError("<decompression-bomb>")
    return payload, tail


_ZSTD_DICT: Final[frozendict[ZstdDictMode, int]] = frozendict({"auto": zstandard.DICT_TYPE_AUTO, "fulldict": zstandard.DICT_TYPE_FULLDICT, "rawcontent": zstandard.DICT_TYPE_RAWCONTENT})
# the nine named DEFLATE-matcher strategies fast-to-densest; "auto" is absent by design (it skips the from_level override, since STRATEGY_* has no zero member)
_ZSTD_STRATEGY: Final[frozendict[ZstdStrategy, int]] = frozendict({"fast": zstandard.STRATEGY_FAST, "dfast": zstandard.STRATEGY_DFAST, "greedy": zstandard.STRATEGY_GREEDY, "lazy": zstandard.STRATEGY_LAZY, "lazy2": zstandard.STRATEGY_LAZY2, "btlazy2": zstandard.STRATEGY_BTLAZY2, "btopt": zstandard.STRATEGY_BTOPT, "btultra": zstandard.STRATEGY_BTULTRA, "btultra2": zstandard.STRATEGY_BTULTRA2})
```

## [03]-[RESEARCH]

- [RECEIPT_BUNDLE_REUSE] [RESOLVED]: the seven `CompressionAlgo` rows share the one `core/receipt#RECEIPT` `ArtifactReceipt.Bundle` case (`tuple[ContentKey, str, int, int, int, int, int, float]` — key, algo, level, dict-id, frame-size, member count, integrity-verified count, ratio) unchanged; `_emit` spreads `evidence.algo.value`/`evidence.level`/`evidence.dict_id`/`evidence.frame_size`/`evidence.entries`/`evidence.verified`/`evidence.ratio` onto it for every codec. `entries` is the member count (`len(payloads)`) for every arm — the `multi_compress_to_buffer` frame sequence, the gzip member sequence, and the archive container members all report their real member arity, never the joined-blob count of one. `verified` is the per-codec integrity count: the `ZSTD` arm's `has_checksum` frame count, the `LZ4` arm's `content_checksum` count, the `GZIP` arm's always-present trailer count (one per member), the archive arms' container CRC-verified count, and only the `BROTLI` arm's honest zero. No receipt-case widening lands on the package domain — the verified eight-field case already carries the evidence, the flat-scalar shape mirroring the `Egress`/`Introspection` cases so the receipt owner imports no `BundleEvidence` value object and no native codec handle crosses the seam.
- [BUNDLE_INVERSE_VERB] [RESOLVED]: `Bundle.pack` and `Bundle.unpack` are the one modal-arity inverse verb pair, not a `Pack`/`Unpack` class split — `pack` folds a singular payload or a `*payloads` spread into ONE content-addressed blob of self-delimiting frames, and `unpack(blob)` reads the same `algo`/`profile` the instance carries and walks those frames through `_walked` into a `BundleManifest` of `(name, ContentKey, algo, size)` `ManifestRow` rows. The single-frame codecs walk frame boundaries with each codec's own end-of-frame seam — `ZSTD`/`GZIP` through `decompressobj().unused_data`, `LZ4` through `LZ4FrameDecompressor.eof`/`unused_data`, `BROTLI` through a per-member 8-byte length prefix (its stream is not concatenation-splittable, so the prefix delimits each member and `is_finished` confirms the single sliced stream) — the archive arms (`package/archive#ARCHIVE`) recover the full multi-member row set from the container directory, and the delta arm (`package/delta#DELTA`) reconstructs one to-image row; each recovery arm yields the one uniform `(name, size, digest)` triple — the third slot the 16-byte `xxh3_128` content digest on the runtime XXH3_128 identity family (this run aligns `_walked`'s per-frame `xxhash.xxh3_128_digest` and the `package/archive` ZIP/`_Xxh3Factory` sinks onto it, faster on the hot walk than the prior sha256 and one fixed family with `ContentIdentity.of`) — and `BundleManifest.of` mints each row's `ContentKey` through one `ContentIdentity.of(f"member-{algo}", digest)` fold so member content-addressing is identical across every codec path. Cross-file: `package/delta#DELTA` `_delta_unpack` now seeds `xxhash.xxh3_128_digest(payload)` too, closing the uniform-digest claim end to end — every recovery arm (this page's `_walked`, both `package/archive#ARCHIVE` arms, the delta arm) yields the one 16-byte `xxh3_128` triple. `_walked` is `expression.Block.unfold(step, (blob, 0))` — the anamorphism that peels one frame per step, hashes it, and frees the decompressed payload, so the list/test pass reads the row set without a mutable index, a retained whole-payload buffer, or a per-codec walk loop, and a multi-payload bundle round-trips with member boundaries intact rather than minting N blobs the single-blob unpack cannot reassemble.
- [ZSTD_DICT_RECOVERY] [RESOLVED]: a latent correctness defect the "dense confident" trained-dict prose hid. `Bundle.trained` binds a `FULLDICT` `ZstdCompressionDict` on `ZstdKnobs.dict_data`/`dict_mode`, so every pack frame is dictionary-compressed — but the prior `_zstd_frame` recovery built `ZstdDecompressor(max_window_size=_DECOMPRESS_WINDOW)` with NO `dict_data`, and a dictless decode of a fulldict frame raises `ZstdError("...Data corruption detected")` (assay-reproduced against a real `train_dictionary(112640, corpus)` frame). Because the `_walked` manifest pass DECODES each frame to hash its payload, every `Bundle.trained` bundle was UNRECOVERABLE — the small-payload ratio win the Dictionary bullet advertises minted manifests its own unpack could not read. Fix: `_zstd_frame` is now the factory `_zstd_frame(trained)` closing over ONE dict-aware `ZstdDecompressor(dict_data=trained, max_window_size=_DECOMPRESS_WINDOW)` reused across every frame, and `_unpack_in_process` rehydrates `trained` from the profile's `dict_data`/`dict_mode` exactly as the pack arm does; a dictless profile (`dict_data is None`) threads `trained=None`, the identical dictless decode as before, so the untrained path is unchanged and denser (one decompressor built once, not one per frame). Both the `<decompression-bomb>` declared-size guard and the `max_window_size` window bound stay on the decode. Verified: `ZstdDecompressor(dict_data=d).decompressobj().decompress(frame) == payload` round-trips the trained frame the dictless decompressor rejects.
- [GZIP_PARALLEL_LANE] [RESOLVED]: the `GZIP` arm compressed each member single-threaded (`gzip_ng.compress`); the GIL-escaping `zlib_ng.gzip_ng_threaded.open(threads=-1, block_size=)` block-fan was unused. The `_gzip_member` size-disposition now routes a payload `>= _GZIP_PARALLEL_THRESHOLD` (1 MiB) through the threaded writer — which partitions into `block_size` blocks, compresses each on a `_ParallelCompress` worker (`compress_and_crc`, one GIL-released call), and recombines the per-block CRCs into the RFC 1952 trailer via `crc32_combine` INTERNALLY (never hand-rolled: the `zlib-ng` `.api` `RAIL_LAW` rejects a hand-rolled partition/CRC-combine/pool) — while a smaller payload stays single-shot, a disposition on the value's size, not a mode flag, and the fan parallelizes even nested inside the `to_thread` offload (the compression releases the GIL). Both lanes emit ONE self-delimiting gzip member the `_gzip_frame` walk (`zlib_ng.decompressobj(wbits=31)` + `unused_data`) recovers unchanged (verified: `unused_data == b""` for a single threaded member, round-trips byte-identically). The threaded writer exposes no `mtime`, so `_gzip_member` overwrites the RFC 1952 header mtime field (`raw[:4] + int(k.mtime).to_bytes(4, "little") + raw[8:]`) to keep the member byte-reproducible (verified: two mtime-patched threaded packs of one payload are byte-identical), preserving the content-addressing dedup the one-shot `mtime=` path already held. `GzipKnobs` grows two fields (`threads`/`block_size`) and the module gains `_GZIP_PARALLEL_THRESHOLD` — the Growth-bullet "a new tuning knob is one field" law, zero new verb.
- [ZSTD_MATCHER_AXES] [RESOLVED]: `ZstdKnobs` exposed only `level`/`threads`/`window_log`/`enable_ldm`/`write_checksum` of the `ZstdCompressionParameters.from_level` override surface. It now also carries `strategy` (a `ZstdStrategy` `Literal` resolved through `_ZSTD_STRATEGY` to the nine `STRATEGY_FAST..BTULTRA2` selectors, `"auto"` skipping the override since `STRATEGY_*` has no zero member), `hash_log`, `chain_log`, and `target_length`, each carrying 0 as the from_level auto-sentinel (assay-confirmed `from_level(hash_log=0/chain_log=0/target_length=0)` accepted as "level-derived", the same contract as the existing `window_log=0`), threaded into the one derived parameter object — never a raw-int spread, never `level=` alongside `compression_params=`. Three capabilities are DECLINED as architecturally incompatible (justified, not omitted): `FORMAT_ZSTD1_MAGICLESS` — a magicless frame carries no magic to delimit and `frame_content_size` cannot read its declared size, breaking the `_walked`/`_declared` bomb-guard and the `unused_data` boundary walk; the `stream_writer`/`compressobj` streaming SINK and the brotli `Compressor.process/finish` sink — the bundle input is already a fully-materialized `bytes`, so a stream sink adds no back-pressure the memory has not already committed (and `MODE_FONT` is already the `BrotliMode` "font" token); `multi_decompress_to_buffer` on recovery — it materializes every frame at once, breaking the bounded-memory `_walked` anamorphism that frees each decompressed payload per step (the STREAM-OVER-MATERIALIZE law). The `lz4.block` `dict=` shared-dictionary primer is DECLINED as redundant capability: the many-small-similar-payload dict-ratio concern it targets is already owned by the `Bundle.trained` zstd `FULLDICT` path (the `ZSTD` archival arm), while lz4 owns the orthogonal hot-path low-latency concern — adding a dict to lz4 would mix two owners the doctrine keeps separate, and would additionally require threading the profile through `_worker_unpack` (which receives only `(algo, blob)`) plus an 8-byte-prefix block-frame walk (the raw block codec is not concatenation-self-delimiting, unlike the frame codec), a subprocess-dispatch restructure disproportionate to a concern already owned. The WOFF2 per-table brotli orchestration (`MODE_FONT` is already the `BrotliMode` "font" token this arm exposes) pairs with `fontTools.ttLib.woff2` on the typography plane — a genuine CROSS-FILE residual for a `typography/font` consumer to compose this arm, never this byte-blob owner's concern.
