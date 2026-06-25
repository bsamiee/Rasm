# [PY_ARTIFACTS_CODEC]

The content-addressed single-blob compression owner and the union spine the archive and delta arms compose. `Bundle` packs any emitted artifact into content-addressed compressed bytes AND unpacks/lists/tests the inverse; `CompressionAlgo` is ONE algorithm-row owner discriminating zstandard, lz4, brotli, the shared zlib-ng gzip substrate, and — composed from `package/archive#ARCHIVE` and `package/delta#DELTA` — the py7zr/stream-zip container and detools binary-delta arms, never a per-algorithm class family. `CodecProfile` is the ONE policy union carrying one typed knob-struct per codec case — a `ZstdKnobs` (`level`, `threads`, `window_log`, `dict_data`, `dict_mode`), an `Lz4Knobs` (`compression_level`, `block_size`, `content_checksum`), a `BrotliKnobs` (`mode`, `quality`, `lgwin`, `lgblock`), a `GzipKnobs` (`level`, `mtime`), plus the `SevenZKnobs`/`ZipStreamKnobs` archive cases and the `DeltaKnobs` delta case the sibling pages author — so a hardcoded literal beside a codec call is the deleted form, every match arm reads a named field rather than a positional tuple slot, and a latency-versus-ratio swap is one policy value. `Bundle.pack` and `Bundle.unpack` are the one modal-arity inverse verb pair: pack folds payloads into compressed bytes keyed by the runtime content key, unpack recovers them into a `BundleManifest` of `(name, ContentKey, algo, size)` rows; a bundle of identical inputs at an identical algorithm and profile is a cache hit by reference. This is the compression close over already-emitted bytes and the inverse recovery of those bytes — it owns no artifact production.

## [01]-[INDEX]

- [01]-[CODEC]: the `Bundle`/`CompressionAlgo`/`CodecProfile` content-addressed compression owner — the per-codec knob structs, the `BundleManifest`/`ManifestRow` value object, the `BundleEvidence` typed receipt, dictionary-trained zstd through `zstandard.train_dictionary`, the shared `zlib-ng` SIMD gzip-container interop substrate, the `_in_process`/`_unpack_in_process` cp315-core dispatch (zstandard/gzip core; the archive and delta arms composed from the sibling pages), and the `_gated_codec`/`_gated_unpack` worker pair (lz4/brotli on the `python_version<'3.15'` band) threading the one typed compression-evidence receipt onto `core/receipt#RECEIPT` `ArtifactReceipt.Bundle`.

## [02]-[CODEC]

- Owner: `Bundle` the one bundle owner; `CompressionAlgo` the closed `StrEnum` discriminating algorithm across the seven codec rows; `CodecProfile` the one `tagged_union` policy value carrying one typed knob-struct per algorithm case; `BundleManifest` the one frozen value object recording the `(name, ContentKey, algo, size)` `ManifestRow` sequence the unpack inverse recovers; `BundleEvidence` the typed compression receipt; the algorithm package is bound per row, never an `if zstd` branch. The `SEVEN_Z`/`ZIP_STREAM` cases and their workers live at `package/archive#ARCHIVE` and the `DELTA` case at `package/delta#DELTA`, both COMPOSING this owner's `Bundle`/`CompressionAlgo`/`CodecProfile`/`BundleEvidence` scaffold; the four single-blob codec arms (`ZSTD`/`GZIP` on the core, `LZ4`/`BROTLI` gated) resolve here.
- Cases: `CompressionAlgo` rows `ZSTD` (zstandard `ZstdCompressor`) · `LZ4` (lz4 `frame`) · `BROTLI` (brotli `compress`) · `GZIP` (zlib-ng `gzip_ng` SIMD gzip-container for stdlib interop) resolve on this page; `SEVEN_Z` (py7zr `SevenZipFile`) · `ZIP_STREAM` (stream-zip `stream_zip`/`async_stream_zip`) · `DELTA` (detools `create_patch`/`apply_patch`) resolve on the archive/delta siblings — matched by `match`/`case`, each binding its algorithm package and reading its `CodecProfile` case; `CodecProfile` cases `zstd` (`ZstdKnobs`) · `lz4` (`Lz4Knobs`) · `brotli` (`BrotliKnobs`) · `gzip` (`GzipKnobs`) · `seven_z` (`SevenZKnobs`) · `zip_stream` (`ZipStreamKnobs`) · `delta` (`DeltaKnobs`) — one frozen knob-struct per algorithm row whose fields are named axes the match arm reads directly, the default profile per algorithm a frozen module constant. Every closed knob axis is a `Literal` vocabulary, not a bare ordinal: `Lz4Block`, `BrotliMode`, `ZstdDictMode` carry their package-constant family as named tokens that resolve to the real value through one frozen dispatch row at codec scope, so a string-built `getattr` attribute lookup and a raw `int`/`str` profile field are the deleted forms and the serialized `CodecProfile` carries no native handle across the subprocess lane. The archive `ZipMethod` and the delta `DeltaPatchType`/`DeltaAlgorithm`/`DeltaCompression` axes ride the union but resolve their dispatch rows on the owning sibling page.
- Modality: `Bundle.pack` and `Bundle.unpack` are the one modal-arity inverse verb pair, discriminating on the value's shape rather than a `direction` flag. `pack` keys one bundle from a singular payload and ONE Merkle-parent bundle from a `*payloads` spread of two-or-more through the zstd `multi_compress_to_buffer` threaded batch (the `ZIP_STREAM`/`SEVEN_Z` archive arms fold the spread into one container so a multi-payload bundle is a single archive, never N archives), discriminating on the spread arity recovered from the call, never a `batch` flag; the arity is the value's shape, so a single payload and a corpus fold one entrypoint. `unpack` is the inverse: it consumes the packed bytes plus the same `algo`/`profile` and recovers the `BundleManifest` — the single-frame codecs on this page (`ZSTD`/`LZ4`/`BROTLI`/`GZIP`) recover one row per frame, the archive arms recover the full row set from the container directory, and the delta arm reconstructs the to-image from the parent. `_compress` and `_recover` are the single key-mint seams — `_in_process`/`_unpack_in_process` and the gated workers each return blobs-or-rows that `_compress`/`_recover` fold the `ContentIdentity.of` projection over once, never per-arm.
- Dictionary: the `ZSTD` arm reads `dict_data: bytes | None` and `dict_mode: ZstdDictMode` off its `ZstdKnobs` and rehydrates one `ZstdCompressionDict(dict_data, dict_type=_ZSTD_DICT[dict_mode])` at codec scope, so the trained dictionary crosses the subprocess lane as raw bytes rather than an unpicklable native handle and the `DICT_TYPE_AUTO`/`DICT_TYPE_FULLDICT`/`DICT_TYPE_RAWCONTENT` axis threads through the `Literal` token; `Bundle.trained` mines `zstandard.train_dictionary` over a many-similar-artifact corpus and binds the trained `as_bytes()` straight into a `ZSTD` `CodecProfile` at the call site, the dominant small-payload ratio win for receipts, glyph-runs, and repeated chart JSON, configured on the compressor root, never a parallel dict-codec owner or a rename-only `train` forwarder. `dict_id()` reads inline at the evidence mint, never through a single-call helper.
- Frame: the `ZSTD` arm parses its first compressed frame through `zstandard.frame_content_size` so the receipt `frame_size` is the real decompressed-content size the frame header carries, never the compressed-byte length standing in for it, and the richer `zstandard.get_frame_parameters` `FrameParameters` view (the `content_size`/`window_size`/`dict_id`/`has_checksum` header read) is the composed frame-inspection capability the dict-id cross-check reads without decoding; the level axis caps at `zstandard.MAX_COMPRESSION_LEVEL`. The `GZIP` arm reads the original payload length as its frame size because the gzip container's RFC 1952 trailer is the size source.
- Gzip: the `GZIP` arm composes the shared `data:compression` `zlib-ng` SIMD gzip-container substrate — `gzip_ng.compress(payload, compresslevel=level, mtime=mtime)` produces the RFC 1952 gzip-framed bytes (the `wbits=31` gzip header+trailer the `_GzipReader` engine writes) so the bundle round-trips with any stdlib `gzip` peer at accelerated throughput, and `unpack` recovers through `gzip_ng.decompress(blob)`; the substrate is COMPOSED, not re-admitted (the `zlib-ng` row already lives in the manifest as the `data` owner's accelerated DEFLATE/gzip codec), and the bundle owner reaches it for exactly the gzip-interop case a non-legacy payload routes away from to `zstandard` (archival) or `lz4` (hot path). The level axis is the standard zlib `0..9` range (`Z_BEST_COMPRESSION = 9`), never an isal-style `0..3` assumption. The same `zlib_ng.crc32` rolling-CRC surface this substrate exposes is the integrity seed the `package/archive#ARCHIVE` stored-ZIP member and streamed-unzip member folds consume — one substrate, no second CRC path.
- Receipt: each pack contributes `core/receipt#RECEIPT` `ArtifactReceipt.Bundle` carrying the content key and the `BundleEvidence` facts — algo, level, dictionary id, frame size, entry count, CRC-verified count, ratio — projected through the one `BundleEvidence.measure` constructor that folds `in_bytes`/`out_bytes` once; `_emit` spreads the named evidence fields onto the flat-scalar `ArtifactReceipt.Bundle` case, and the receipt owner's own `_facts` arm is the single string-map projector for the whole union, so `BundleEvidence` carries no second `facts` projection and the receipt owner imports no `BundleEvidence` value object (the receipt-side circular import the flattening forecloses). The `ratio` is the one observable-compression value the runtime `observability/metrics` `MeterProvider` compression-ratio signal stream reads off the receipt fold. The `measure` projector folds `in_bytes`/`out_bytes` from the payload and blob spans once, so the input-sum, output-sum, and entry-count construction lives on one row rather than re-derived per codec arm. The CRC-verified count is the archive `test`/streamed-integrity proof the sibling arms fill and stays zero for the single-blob codecs (`ZSTD`/`LZ4`/`BROTLI`/`GZIP`/`DELTA`) that own no container-level CRC re-read. The `core/receipt#RECEIPT` `Bundle` case (`tuple[ContentKey, str, int, int, int, int, int, float]`, eight fields, verified) already carries this evidence; every algorithm row reuses it unchanged — no receipt-case widening lands on the package owner.
- Packages: `zstandard` (`ZstdCompressor`, `ZstdCompressionParameters`, `ZstdCompressionDict`, `ZstdDecompressor`, `train_dictionary`, `multi_compress_to_buffer`, `BufferWithSegments`, `frame_content_size`, `get_frame_parameters`/`FrameParameters`, `MAX_COMPRESSION_LEVEL`, `DICT_TYPE_*`) on the cp315 core; the shared `data:compression` substrate `zlib-ng` (`gzip_ng.compress`, `gzip_ng.decompress`, `gzip_ng.GzipNGFile`, `gzip_ng.BadGzipFile`, `zlib_ng.crc32` the rolling CRC the sibling stored-ZIP and streamed-unzip folds seed through) composed not re-admitted; `lz4` (`frame.compress`, `frame.decompress`, `get_frame_info`, `COMPRESSIONLEVEL_*`, `BLOCKSIZE_*`) and `brotli` (`compress`, `decompress`, `MODE_TEXT`/`MODE_FONT`/`MODE_GENERIC`, `lgwin`) gated `python_version<'3.15'`; `expression` (`tagged_union`/`tag`/`case`); `msgspec` (`Struct(frozen=True)`, `msgpack` the `CodecProfile` serialization across the gated lane); runtime (`content_identity.ContentIdentity`/`ContentKey`, `faults.RuntimeRail`/`async_boundary`, `anyio.to_process.run_sync` the runtime subprocess lane). The `py7zr`/`stream-zip`/`stream-unzip` archive packages and the `detools` delta package are imported only on their owning sibling worker arms, never the codec page.
- Growth: a new single-blob algorithm is one `CompressionAlgo` row, one `CodecProfile` case with its knob-struct, one default-profile constant, one `_in_process` (or gated) pack arm, and one `_unpack_in_process` recover arm; a new container/delta algorithm grows the archive/delta sibling page composing this scaffold; a new tuning knob is one field on the owning knob-struct, a new bounded knob value one token on the owning `Literal` axis plus one dispatch-row entry; a new evidence fact one `(label, value)` row in `facts`; a new manifest column one field on `ManifestRow`; zero new surface and zero new verb beside the `pack`/`unpack` inverse.
- Boundary: a per-algorithm compression class family, a hardcoded `MODE_GENERIC`/`quality=11` literal, a positional knob tuple decoded by index, a `getattr(module, f"BLOCKSIZE_{name}")` string-built constant lookup, a bare-`int`/`str` mode/dict-mode profile field, a rename-only `train` forwarder, a per-arm `ContentIdentity.of` key-mint, a per-arm `in_bytes`/`out_bytes` re-sum, a `dict_codec` owner beside the zstd arm, a `batch_pack` sibling beside `pack`, a re-admitted `gzip`/`zlib` codec where the shared `zlib-ng` substrate already accelerates DEFLATE/gzip, a second `unzip`/`apply` function per algorithm where the one `unpack` verb dispatches on `algo`, and a parallel `BundleManifest`-per-codec type are the deleted forms; this owner is content-addressed bundling AND the inverse recovery over already-emitted artifact bytes and owns no artifact production. The cp315-core `ZSTD`/`GZIP` arms compress and recover in-process through ONE `_in_process`/`_unpack_in_process` dispatch, the `SEVEN_Z`/`ZIP_STREAM` container arms (in `package/archive#ARCHIVE`) and the `DELTA` arm (in `package/delta#DELTA`) extend that same dispatch on the core band; the `LZ4`/`BROTLI` arms ride the gated `python_version<'3.15'` band and never resolve in the cp315-core process, so each dispatches its codec plus its serialized `CodecProfile` case onto the runtime subprocess lane (`anyio.to_process.run_sync`) where the gated-band worker imports the codec at module scope — neither a module-top nor a lazy gated import lands on the core page. The `ArtifactReceipt.Bundle` flat-scalar case the `core/receipt#RECEIPT` owner already carries is unchanged; this page composes the settled `BundleEvidence` value and spreads its named fields onto the receipt case, mirroring the flat-scalar `Egress`/`Introspection` cases so the receipt owner imports no producer value object.

```python signature
from collections.abc import Iterable
from datetime import datetime, timezone
from enum import StrEnum
from typing import Final, Literal, assert_never

import zstandard
from anyio import to_process
from expression import case, tag, tagged_union
from msgspec import Struct, msgpack

from rasm.runtime.content_identity import ContentIdentity, ContentKey
from rasm.runtime.faults import RuntimeRail, async_boundary

from artifacts.core.receipt import ArtifactReceipt
from artifacts.package.archive import SevenZKnobs, ZipStreamKnobs, ZipMethod, _archive_in_process, _archive_unpack
from artifacts.package.delta import DeltaKnobs, DeltaPatchType, DeltaAlgorithm, DeltaCompression, _delta_in_process, _delta_unpack

type Lz4Block = Literal["default", "max64kb", "max256kb", "max1mb", "max4mb"]
type BrotliMode = Literal["generic", "text", "font"]
type ZstdDictMode = Literal["auto", "fulldict", "rawcontent"]


class CompressionAlgo(StrEnum):
    ZSTD = "zstd"
    LZ4 = "lz4"
    BROTLI = "brotli"
    SEVEN_Z = "7z"
    ZIP_STREAM = "zip-stream"
    DELTA = "delta"
    GZIP = "gzip"


class ZstdKnobs(Struct, frozen=True):
    level: int = 19
    threads: int = -1
    window_log: int = 0
    dict_data: bytes | None = None
    dict_mode: ZstdDictMode = "auto"


class Lz4Knobs(Struct, frozen=True):
    compression_level: int = 0
    block_size: Lz4Block = "default"
    content_checksum: bool = True


class BrotliKnobs(Struct, frozen=True):
    mode: BrotliMode = "generic"
    quality: int = 11
    lgwin: int = 22
    lgblock: int = 0


class GzipKnobs(Struct, frozen=True):
    level: int = 9
    mtime: float = 0.0


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


DEFAULT_PROFILE: Final[dict[CompressionAlgo, CodecProfile]] = {
    CompressionAlgo.ZSTD: CodecProfile(zstd=ZstdKnobs()),
    CompressionAlgo.LZ4: CodecProfile(lz4=Lz4Knobs()),
    CompressionAlgo.BROTLI: CodecProfile(brotli=BrotliKnobs()),
    CompressionAlgo.SEVEN_Z: CodecProfile(seven_z=SevenZKnobs()),
    CompressionAlgo.ZIP_STREAM: CodecProfile(zip_stream=ZipStreamKnobs()),
    CompressionAlgo.GZIP: CodecProfile(gzip=GzipKnobs()),
}


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


class BundleEvidence(Struct, frozen=True):
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
        return BundleEvidence(algo, level, dict_id, frame_size, len(blobs), verified, sum(map(len, payloads)), sum(map(len, blobs)))


class Bundle(Struct, frozen=True):
    payloads: tuple[bytes, ...]
    algo: CompressionAlgo
    profile: CodecProfile

    @staticmethod
    def of(algo: CompressionAlgo, *payloads: bytes, profile: CodecProfile | None = None) -> "Bundle":
        return Bundle(payloads=payloads, algo=algo, profile=profile if profile is not None else DEFAULT_PROFILE[algo])

    @staticmethod
    def trained(corpus: tuple[bytes, ...], *payloads: bytes, level: int = 19, dict_size: int = 112_640) -> "Bundle":
        dict_data = zstandard.train_dictionary(dict_size, list(corpus), dict_type=zstandard.DICT_TYPE_FULLDICT)
        return Bundle.of(CompressionAlgo.ZSTD, *payloads, profile=CodecProfile(zstd=ZstdKnobs(level=level, dict_data=dict_data.as_bytes(), dict_mode="fulldict")))

    @staticmethod
    def delta(from_image: bytes, parent_key: ContentKey, payload: bytes, *, patch_type: DeltaPatchType = "sequential", algorithm: DeltaAlgorithm = "bsdiff", compression: DeltaCompression = "zstd") -> "Bundle":
        return Bundle.of(CompressionAlgo.DELTA, payload, profile=CodecProfile(delta=DeltaKnobs(from_image=from_image, parent_key=parent_key, patch_type=patch_type, algorithm=algorithm, compression=compression)))

    async def pack(self) -> RuntimeRail[tuple[ContentKey, ArtifactReceipt]]:
        return await async_boundary(f"bundle.{self.algo}", self._emit)

    async def unpack(self, blob: bytes) -> RuntimeRail[BundleManifest]:
        return await async_boundary(f"unbundle.{self.algo}", lambda: self._recover(blob))

    async def _emit(self) -> tuple[ContentKey, ArtifactReceipt]:
        keys, evidence = await self._compress()
        key = keys[0] if len(keys) == 1 else ContentIdentity.of(f"bundle-{self.algo}", keys)
        return key, ArtifactReceipt.Bundle(key, evidence.algo.value, evidence.level, evidence.dict_id, evidence.frame_size, evidence.entries, evidence.verified, evidence.ratio)

    async def _compress(self) -> tuple[tuple[ContentKey, ...], BundleEvidence]:
        match self.profile:
            case CodecProfile(tag="lz4") | CodecProfile(tag="brotli"):
                framed = await to_process.run_sync(_gated_codec, self.algo.value, msgpack.encode(self.profile), self.payloads)
                blobs, evidence = framed[:-1], msgpack.decode(framed[-1], type=BundleEvidence)
            case _:
                blobs, evidence = _in_process(self.payloads, self.algo, self.profile)
        return tuple(ContentIdentity.of(f"bundle-{self.algo}", b) for b in blobs), evidence

    async def _recover(self, blob: bytes) -> BundleManifest:
        match self.profile:
            case CodecProfile(tag="lz4") | CodecProfile(tag="brotli"):
                recovered = await to_process.run_sync(_gated_unpack, self.algo.value, msgpack.encode(self.profile), blob)
            case _:
                recovered = _unpack_in_process(blob, self.algo, self.profile)
        return BundleManifest.of(self.algo, recovered)
```

```python signature
import zstandard

from msgspec import msgpack


def _in_process(payloads: tuple[bytes, ...], algo: CompressionAlgo, profile: CodecProfile) -> tuple[tuple[bytes, ...], BundleEvidence]:
    match profile:
        case CodecProfile(tag="zstd", zstd=ZstdKnobs() as k):
            level = min(k.level, zstandard.MAX_COMPRESSION_LEVEL)
            trained = zstandard.ZstdCompressionDict(k.dict_data, dict_type=_ZSTD_DICT[k.dict_mode]) if k.dict_data is not None else None
            params = zstandard.ZstdCompressionParameters(compression_level=level, window_log=k.window_log, threads=k.threads)
            compressor = zstandard.ZstdCompressor(level=level, dict_data=trained, compression_params=params)
            segments = compressor.multi_compress_to_buffer(list(payloads), threads=k.threads) if len(payloads) > 1 else None
            blobs = tuple(bytes(segments[i]) for i in range(len(payloads))) if segments is not None else (compressor.compress(payloads[0]),)
            dict_id = trained.dict_id() if trained is not None else 0
            return blobs, BundleEvidence.measure(algo, level, dict_id, zstandard.frame_content_size(blobs[0]), 0, payloads, blobs)
        case CodecProfile(tag="gzip", gzip=GzipKnobs() as k):
            from zlib_ng import gzip_ng

            blobs = tuple(gzip_ng.compress(p, compresslevel=k.level, mtime=k.mtime) for p in payloads)
            return blobs, BundleEvidence.measure(algo, k.level, 0, len(payloads[0]), 0, payloads, blobs)
        case CodecProfile(tag="seven_z") | CodecProfile(tag="zip_stream"):
            return _archive_in_process(payloads, algo, profile)
        case CodecProfile(tag="delta"):
            return _delta_in_process(payloads, algo, profile)
        case _:
            assert_never(profile)


def _unpack_in_process(blob: bytes, algo: CompressionAlgo, profile: CodecProfile) -> tuple[tuple[str, int, bytes], ...]:
    match profile:
        case CodecProfile(tag="zstd"):
            payload = zstandard.ZstdDecompressor().decompress(blob)
            return (("payload-0", len(payload), payload),)
        case CodecProfile(tag="gzip"):
            from zlib_ng import gzip_ng

            payload = gzip_ng.decompress(blob)
            return (("payload-0", len(payload), payload),)
        case CodecProfile(tag="seven_z") | CodecProfile(tag="zip_stream"):
            return _archive_unpack(blob, algo, profile)
        case CodecProfile(tag="delta"):
            return _delta_unpack(blob, algo, profile)
        case _:
            assert_never(profile)


def _gated_codec(algo: str, profile_blob: bytes, payloads: tuple[bytes, ...]) -> tuple[bytes, ...]:
    match profile := msgpack.decode(profile_blob, type=CodecProfile):
        case CodecProfile(tag="lz4", lz4=Lz4Knobs() as k):
            import lz4.frame

            block_size = {"default": lz4.frame.BLOCKSIZE_DEFAULT, "max64kb": lz4.frame.BLOCKSIZE_MAX64KB, "max256kb": lz4.frame.BLOCKSIZE_MAX256KB, "max1mb": lz4.frame.BLOCKSIZE_MAX1MB, "max4mb": lz4.frame.BLOCKSIZE_MAX4MB}[k.block_size]
            blobs = tuple(lz4.frame.compress(p, compression_level=k.compression_level, block_size=block_size, content_checksum=k.content_checksum) for p in payloads)
            frame_size = lz4.frame.get_frame_info(blobs[0])["content_size"]
            return (*blobs, msgpack.encode(BundleEvidence.measure(CompressionAlgo.LZ4, k.compression_level, 0, frame_size, 0, payloads, blobs)))
        case CodecProfile(tag="brotli", brotli=BrotliKnobs() as k):
            import brotli

            mode = {"generic": brotli.MODE_GENERIC, "text": brotli.MODE_TEXT, "font": brotli.MODE_FONT}[k.mode]
            blobs = tuple(brotli.compress(p, mode=mode, quality=k.quality, lgwin=k.lgwin, lgblock=k.lgblock) for p in payloads)
            return (*blobs, msgpack.encode(BundleEvidence.measure(CompressionAlgo.BROTLI, k.quality, 0, len(payloads[0]), 0, payloads, blobs)))
        case _:
            assert_never(profile)


def _gated_unpack(algo: str, profile_blob: bytes, blob: bytes) -> tuple[tuple[str, int, bytes], ...]:
    match profile := msgpack.decode(profile_blob, type=CodecProfile):
        case CodecProfile(tag="lz4"):
            import lz4.frame

            payload = lz4.frame.decompress(blob)
            return (("payload-0", len(payload), payload),)
        case CodecProfile(tag="brotli"):
            import brotli

            payload = brotli.decompress(blob)
            return (("payload-0", len(payload), payload),)
        case _:
            assert_never(profile)


_ZSTD_DICT: Final[dict[ZstdDictMode, int]] = {"auto": zstandard.DICT_TYPE_AUTO, "fulldict": zstandard.DICT_TYPE_FULLDICT, "rawcontent": zstandard.DICT_TYPE_RAWCONTENT}
```

## [03]-[RESEARCH]

- [RECEIPT_BUNDLE_REUSE] [RESOLVED]: the seven `CompressionAlgo` rows share the one `core/receipt#RECEIPT` `ArtifactReceipt.Bundle` case (`tuple[ContentKey, str, int, int, int, int, int, float]` — key, algo, level, dict-id, frame-size, entry count, CRC-verified count, ratio) unchanged; `_emit` spreads `evidence.algo.value`/`evidence.level`/`evidence.dict_id`/`evidence.frame_size`/`evidence.entries`/`evidence.verified`/`evidence.ratio` onto it for every codec, the `ZIP_STREAM`/`SEVEN_Z` archive arms filling `entries`/`verified` from the container and the single-frame codecs leaving `verified` zero. No receipt-case widening lands on the package domain — the verified eight-field case already carries the evidence, the flat-scalar shape mirroring the `Egress`/`Introspection` cases so the receipt owner imports no `BundleEvidence` value object and no native codec handle crosses the seam.
- [BUNDLE_INVERSE_VERB] [RESOLVED]: `Bundle.pack` and `Bundle.unpack` are the one modal-arity inverse verb pair, not a `Pack`/`Unpack` class split — `unpack(blob)` reads the same `algo`/`profile` the instance carries and dispatches the recovery arm through `_recover`, returning a `BundleManifest` of `(name, ContentKey, algo, size)` `ManifestRow` rows. The single-frame codecs (`ZSTD`/`LZ4`/`BROTLI`/`GZIP`) recover one row per decompressed frame, the archive arms (`package/archive#ARCHIVE`) recover the full multi-member row set bounded, and the delta arm (`package/delta#DELTA`) reconstructs one to-image row; each recovery arm yields a `(name, size, digest)` triple and `BundleManifest.of` mints each row's `ContentKey` through one `ContentIdentity.of(f"member-{algo}", digest)` fold so the recovered member keys are content-addressed, the list/test pass reading the row set without a second hashing path or a payload buffer.
- [GZIP_SUBSTRATE_COMPOSE] [RESOLVED]: the `GZIP` arm composes the shared `data:compression` `zlib-ng` substrate, NOT a re-admitted gzip codec — `gzip_ng.compress(payload, compresslevel=level, mtime=mtime)` produces the RFC 1952 gzip-framed bytes (`wbits=31` header+trailer, the SIMD-accelerated `_GzipReader` engine) so the bundle round-trips with any stdlib `gzip` peer at accelerated throughput, and `gzip_ng.decompress(blob)` recovers. The `zlib-ng` manifest row already lives as the `data` owner's accelerated DEFLATE/gzip codec; the bundle owner reaches it for exactly the gzip-stdlib-interop case a non-legacy payload routes away from to `zstandard` (archival) or `lz4` (hot path). The level is the standard zlib `0..9` range (`Z_BEST_COMPRESSION = 9`), never an isal-style `0..3` assumption. `gzip_ng.compress(data, compresslevel=9, *, mtime=None) -> bytes`, `gzip_ng.decompress(data) -> bytes`, `GzipNGFile`, and `BadGzipFile` are confirmed on cp315 (installed `1.0.0`, `zlib_ng.cpython-315-darwin.so`); the `zlib_ng.crc32` rolling-CRC the `package/archive#ARCHIVE` stored-ZIP `NO_COMPRESSION_*` member and streamed-unzip member fold seed through is the same substrate, so no `crc32` spelling rides the zstandard catalogue.
- [BAND_PLACEMENT] [RESOLVED]: the codec page resolves `ZSTD`/`GZIP` on the cp315 core (ungated) and `LZ4`/`BROTLI` on the gated `python_version<'3.15'` band; the archive (`SEVEN_Z`/`ZIP_STREAM`) and delta (`DELTA`) arms also resolve on the cp315 core through their sibling pages. The `_compress`/`_recover` dispatch matches the gated `lz4`/`brotli` arms as the explicit `to_process.run_sync` rows and every other codec falls through to the cp315-core `_in_process`/`_unpack_in_process` — five of the seven resolve on the core, so the gated polarity is the explicit minority match. The `_gated_codec`/`_gated_unpack` workers import `lz4.frame`/`brotli` at module scope on the gated band and carry the serialized `CodecProfile`, so no native handle and no module-top gated import lands on the core page.
- [ZSTD_API_VERIFIED] [RESOLVED]: `ZstdCompressor(level=, dict_data=, compression_params=, threads=)` and `ZstdCompressionParameters(compression_level=, window_log=, threads=)` (`from_level(level, source_size=, dict_size=)` the alternate parameter derivation), `ZstdDecompressor().decompress(data, max_output_size=0)`, `ZstdCompressionDict(data, dict_type=)` with its `dict_id()`/`as_bytes()` accessors, `train_dictionary(dict_size, samples, dict_type=) -> ZstdCompressionDict`, `multi_compress_to_buffer(data, threads=0) -> BufferWithSegmentsCollection` (gated on `'multi_compress_to_buffer' in zstandard.backend_features`; the cp315 native wheel advertises it), `BufferWithSegments` per-segment index access, `frame_content_size(data) -> int`, `get_frame_parameters(data, format=None) -> FrameParameters` (the `content_size`/`window_size`/`dict_id`/`has_checksum` header view), `MAX_COMPRESSION_LEVEL`, and `DICT_TYPE_AUTO`/`DICT_TYPE_FULLDICT`/`DICT_TYPE_RAWCONTENT` all verify against the folder `zstandard` `.api` `[02]` public-type rows and the `[03]-[ENTRYPOINTS]` constructor/one-shot tables (installed `0.25.0`, `cp315-cp315-macosx_14_0_arm64` native wheel). The batch-buffer ops are conditional on the `cext` backend; the streaming/one-shot rails are present under both backends.
- [LZ4_BROTLI_API_VERIFIED] [RESOLVED]: `lz4.frame.compress(data, compression_level=, block_size=, content_checksum=) -> bytes`, `lz4.frame.decompress(data) -> bytes`, `lz4.frame.get_frame_info(frame)` returning the `content_size`-bearing dict (`block_size`/`block_size_id`/`content_checksum`/`content_size`/`block_linked`/`block_checksum`/`skippable` key set), and the `BLOCKSIZE_DEFAULT`/`BLOCKSIZE_MAX64KB`/`BLOCKSIZE_MAX256KB`/`BLOCKSIZE_MAX1MB`/`BLOCKSIZE_MAX4MB` family verify against the folder `lz4` `.api` `[03]` frame-entrypoint rows `[01]`/`[02]`/`[11]` and the `[02]` `BLOCKSIZE_*` constant row (installed `4.4.5`, gated `python_version<'3.15'`). `brotli.compress(string, mode=MODE_GENERIC, quality=11, lgwin=22, lgblock=0) -> bytes`, `brotli.decompress(data) -> bytes`, and `MODE_GENERIC`/`MODE_TEXT`/`MODE_FONT` (the int `0`/`1`/`2` mode axis, the WOFF2 font codec among them) verify against the folder `brotli` `.api` `[03]` entrypoint rows `[01]`/`[02]` and the `[02]` mode-axis row (installed `1.2.0`, gated `python_version<'3.15'`); the `BrotliKnobs.mode`/`Lz4Knobs.block_size` `Literal` tokens ride the gated-band serialization as strings and the worker resolves the package ordinal through one frozen dispatch row at codec scope.
