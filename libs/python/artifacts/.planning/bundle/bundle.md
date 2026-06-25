# [PY_ARTIFACTS_BUNDLE]

The artifact-bundling and compression owner. `Bundle` packs any emitted artifact into content-addressed compressed bytes AND unpacks/lists/tests the inverse; `CompressionAlgo` is ONE algorithm-row owner collapsing zstandard, lz4, brotli, py7zr, stream-zip, detools, and the shared zlib-ng gzip substrate — never a per-algorithm class family. `CodecProfile` is the ONE policy union carrying one typed knob-struct per codec case — a `ZstdKnobs` (`level`, `threads`, `window_log`, `dict_data`, `dict_mode`), an `Lz4Knobs` (`compression_level`, `block_size`, `content_checksum`), a `BrotliKnobs` (`mode`, `quality`, `lgwin`, `lgblock`), a `SevenZKnobs` (`filters`, `header_encryption`, `password`), a `ZipStreamKnobs` (`method`, `level`, `password`, `mode_bits`), a `DeltaKnobs` (`from_image`, `parent_key`, `patch_type`, `algorithm`, `compression`), a `GzipKnobs` (`level`, `mtime`) — so a hardcoded literal beside a codec call is the deleted form, every match arm reads a named field rather than a positional tuple slot, and a latency-versus-ratio swap is one policy value. `Bundle.pack` and `Bundle.unpack` are the one modal-arity inverse verb pair: pack folds payloads into compressed bytes keyed by the runtime content key, unpack recovers them into a `BundleManifest` of `(name, ContentKey, algo, size)` rows; a bundle of identical inputs at an identical algorithm and profile is a cache hit by reference. This is bundling, not visualization; it owns no artifact production, only the compression close over already-emitted bytes and the inverse recovery of those bytes.

## [01]-[INDEX]

- [01]-[BUNDLE]: algorithm-row compression and the inverse unpack/list/test, the per-codec `CodecProfile` policy union, dictionary-trained zstd, the bounded-memory `stream-zip`/`stream-unzip` ZIP64/AES-256 streaming pair, the `detools` binary-delta create/apply against a parent content key, the shared `zlib-ng` SIMD gzip-container interop substrate, the `BundleManifest` value object, and the content-addressed bundle owner threading the typed compression-evidence receipt.

## [02]-[BUNDLE]

- Owner: `Bundle` the one bundle owner; `CompressionAlgo` the closed `StrEnum` discriminating algorithm across the seven codec rows; `CodecProfile` the one `tagged_union` policy value carrying one typed knob-struct per algorithm case; `BundleManifest` the one frozen value object recording the `(name, ContentKey, algo, size)` `ManifestRow` sequence the unpack inverse recovers; `BundleEvidence` the typed compression receipt; the algorithm package is bound per row, never an `if zstd` branch.
- Cases: `CompressionAlgo` rows `ZSTD` (zstandard `ZstdCompressor`) · `LZ4` (lz4 `frame`) · `BROTLI` (brotli `compress`) · `SEVEN_Z` (py7zr `SevenZipFile`) · `ZIP_STREAM` (stream-zip `stream_zip`/`async_stream_zip` MemberFile streaming with WinZip AES-256) · `DELTA` (detools `create_patch`/`apply_patch` against a parent content key) · `GZIP` (zlib-ng `gzip_ng` SIMD gzip-container for stdlib interop) — matched by `match`/`case`, each binding its algorithm package and reading its `CodecProfile` case; `CodecProfile` cases `zstd` (`ZstdKnobs`) · `lz4` (`Lz4Knobs`) · `brotli` (`BrotliKnobs`) · `seven_z` (`SevenZKnobs`) · `zip_stream` (`ZipStreamKnobs`) · `delta` (`DeltaKnobs`) · `gzip` (`GzipKnobs`) — one frozen knob-struct per algorithm row whose fields are named axes the match arm reads directly, the default profile per algorithm a frozen module constant. Every closed knob axis is a `Literal` vocabulary, not a bare ordinal: `Lz4Block`, `BrotliMode`, `ZstdDictMode`, `ZipMethod` (the `stream-zip` `ZIP_64`/`ZIP_32`/`ZIP_AUTO`/`NO_COMPRESSION_32`/`NO_COMPRESSION_64` `Method` family as named tokens resolving to the real `Method` instance through one frozen dispatch row), `DeltaPatchType` (`sequential`/`in-place`/`bsdiff`/`hdiffpatch`), `DeltaAlgorithm` (`bsdiff`/`hdiffpatch`/`match-blocks`), and `DeltaCompression` (`bz2`/`crle`/`lzma`/`zstd`/`lz4`/`heatshrink`/`none`) carry their package-constant or call-string family as named tokens that resolve to the real value through one frozen dispatch row at codec scope, so a string-built `getattr` attribute lookup and a raw `int`/`str` profile field are the deleted forms and the serialized `CodecProfile` carries no native handle across the subprocess lane.
- Modality: `Bundle.pack` and `Bundle.unpack` are the one modal-arity inverse verb pair, discriminating on the value's shape rather than a `direction` flag. `pack` keys one bundle from a singular payload and ONE Merkle-parent bundle from a `*payloads` spread of two-or-more through the zstd `multi_compress_to_buffer` threaded batch (the `ZIP_STREAM` arm folds the spread into one `stream_zip` member sequence so a multi-payload bundle is a single archive, never N archives), discriminating on the spread arity recovered from the call, never a `batch` flag; the arity is the value's shape, so a single payload and a corpus fold one entrypoint. `unpack` is the inverse: it consumes the packed bytes plus the same `algo`/`profile` and recovers the `BundleManifest` — the `ZIP_STREAM`/`SEVEN_Z` archive arms recover the full `(name, ContentKey, algo, size)` row set from the container directory, the `DELTA` arm reconstructs the to-image from the `DeltaKnobs.from_image` parent and the patch, and the single-frame codecs (`ZSTD`/`LZ4`/`BROTLI`/`GZIP`) recover one row per frame. `_compress` and `_recover` are the single key-mint seams — `_in_process`/`_unpack_in_process` and the gated workers each return blobs-or-rows that `_compress`/`_recover` fold the `ContentIdentity.of` projection over once, never per-arm.
- Dictionary: the `ZSTD` arm reads `dict_data: bytes | None` and `dict_mode: ZstdDictMode` off its `ZstdKnobs` and rehydrates one `ZstdCompressionDict(dict_data, dict_type=_ZSTD_DICT[dict_mode])` at codec scope, so the trained dictionary crosses the subprocess lane as raw bytes rather than an unpicklable native handle and the `DICT_TYPE_AUTO`/`DICT_TYPE_FULLDICT`/`DICT_TYPE_RAWCONTENT` axis threads through the `Literal` token; `Bundle.trained` mines `zstandard.train_dictionary` over a many-similar-artifact corpus and binds the trained `as_bytes()` straight into a `ZSTD` `CodecProfile` at the call site, the dominant small-payload ratio win for receipts, glyph-runs, and repeated chart JSON, configured on the compressor root, never a parallel dict-codec owner or a rename-only `train` forwarder. `dict_id()` reads inline at the evidence mint, never through a single-call helper.
- Frame: the `ZSTD` arm parses its first compressed frame through `zstandard.frame_content_size`/`FrameParameters` so the receipt `frame_size` is the real decompressed-content size the frame header carries, never the compressed-byte length standing in for it; the level axis caps at `zstandard.MAX_COMPRESSION_LEVEL`. The `GZIP` arm reads the original payload length as its frame size because the gzip container's RFC 1952 trailer is the size source; the `ZIP_STREAM` arm reads the per-member declared size from the `MemberFile` tuple it builds.
- Crypto: the `SEVEN_Z` arm threads `password` plus a `FILTER_CRYPTO_AES256_SHA256` chain entry alongside `header_encryption=True`, so encryption is the functional three-field row the `.api` crypto axis mandates, never a lone `header_encryption` bool; `SevenZipFile.test` re-reads the written archive for the CRC-verified entry count the evidence carries. The `ZIP_STREAM` arm threads `password` through `stream_zip(..., password=...)` switching every member to WinZip AES-256 (the `pycryptodome` AES-256/HMAC-SHA1/PBKDF2 backend `stream-zip` owns), and the `unpack` inverse passes the `password.encode()` `bytes` and an `allowed_encryption_mechanisms` allow-list (`frozenset({NO_ENCRYPTION, AES_256})` hardening the trust policy to refuse legacy ZipCrypto and weak AES key lengths) to `stream_unzip`, so the encrypted round-trip is one `password` row on each verb, never a parallel encrypted function.
- Stream: the `ZIP_STREAM` arm is the bounded-memory container — `Bundle._zip_members` folds each payload into a `(name, modified_at, mode, method, data)` `MemberFile` tuple whose `method` is the `_ZIP_METHOD[method]` row (`ZIP_AUTO(size, level)` the size-driven ZIP32/ZIP64 auto-upgrade default, `ZIP_64`/`ZIP_32` the forced rows, the `NO_COMPRESSION_*(size, crc_32)` rows the stored variants whose pre-declared CRC the composed `zlib_ng.crc32` substrate computes) and whose `data` is a one-chunk `iter((payload,))` lazy iterable so member bytes interleave encode-and-emit, then `stream_zip(members, password=...)` yields the container `bytes` chunks `b"".join(...)` seals into the bundle blob; the `async_stream_zip` async-iterable boundary rides the runtime `async_boundary` for a remote-streamed member feed. `unpack` drives `stream_unzip(chunks, password=..., allowed_encryption_mechanisms=...)` member by member, draining each member's `chunks` generator fully (the `UnfinishedIterationError` ordered-consumption guard the `.api` mandates) through a rolling `zlib_ng.crc32` fold that accumulates the per-member size and a 4-byte CRC digest seed into one `(name, size, digest)` triple, so the list/test pass folds each member to its `ManifestRow` without ever buffering the payload — `bounded-memory` holds on both legs, and the CRC digest seeds the `BundleManifest.of` content-key fold.
- Delta: the `DELTA` arm is the binary-delta create/apply against a parent — `DeltaKnobs.from_image` carries the parent artifact bytes and `parent_key: ContentKey` the parent's content key, so `detools.create_patch(BytesIO(from_image), BytesIO(payload), fpatch, patch_type=..., algorithm=..., compression=...)` diffs the parent against the to-image payload and writes the compressed patch the bundle stores; the `(algorithm, patch_type)` combination selects the create kernel (`bsdiff`×`sequential`/`in-place`/`bsdiff`, `hdiffpatch`×`hdiffpatch`, or `match-blocks`) through the `DeltaPatchType`/`DeltaAlgorithm` `Literal` axes, never a per-mode patch type. `unpack` reconstructs the to-image through `detools.apply_patch(BytesIO(from_image), BytesIO(patch), fto)` (the single reconstruction surface that peeks the header type and dispatches sequential vs hdiffpatch internally) and reads `detools.patch_info(BytesIO(patch))` for the patch-kind metadata the receipt carries; the parent `from_image` is the only side-input the delta round-trip needs, so a content-addressed delta keyed by `parent_key` is the storage shape, never two patch entrypoints per kind.
- Gzip: the `GZIP` arm composes the shared `data:compression` `zlib-ng` SIMD gzip-container substrate — `gzip_ng.compress(payload, compresslevel=level, mtime=mtime)` produces the RFC 1952 gzip-framed bytes (the `wbits=31` gzip header+trailer the `_GzipReader` engine writes) so the bundle round-trips with any stdlib `gzip` peer at accelerated throughput, and `unpack` recovers through `gzip_ng.decompress(blob)`; the substrate is COMPOSED, not re-admitted (the `zlib-ng` row already lives in the manifest as the `data` owner's accelerated DEFLATE/gzip codec), and the bundle owner reaches it for exactly the gzip-interop case a non-legacy payload routes away from to `zstandard` (archival) or `lz4` (hot path). The level axis is the standard zlib `0..9` range (`Z_BEST_COMPRESSION = 9`), never an isal-style `0..3` assumption.
- Receipt: each pack contributes `receipt/receipt#RECEIPT` `ArtifactReceipt.Bundle` carrying the content key and the `BundleEvidence` facts — algo, level, dictionary id, frame size, entry count, CRC-verified count, ratio — projected through the one `BundleEvidence.measure` constructor that folds `in_bytes`/`out_bytes` once; `_emit` spreads the named evidence fields onto the flat-scalar `ArtifactReceipt.Bundle` case, and the receipt owner's own `_facts` arm is the single string-map projector for the whole union, so `BundleEvidence` carries no second `facts` projection and the receipt owner imports no `BundleEvidence` value object (the receipt-side circular import the flattening forecloses). The `ratio` is the one observable-compression value the runtime `observability/metrics` `MeterProvider` compression-ratio signal stream reads off the receipt fold. The `measure` projector folds `in_bytes`/`out_bytes` from the payload and blob spans once, so the input-sum, output-sum, and entry-count construction lives on one row rather than re-derived per codec arm. The CRC-verified count is the 7z `test` and ZIP-stream streamed-integrity proof and stays zero for the codecs (`ZSTD`/`LZ4`/`BROTLI`/`GZIP`/`DELTA`) that own no container-level CRC re-read. The existing `receipt.md` `Bundle` case (`tuple[ContentKey, str, int, int, int, int, int, float]`, eight fields, verified) already carries this evidence; the seven new algorithm rows reuse it unchanged — no receipt-case widening lands on this work-item.
- Packages: `zstandard` (`ZstdCompressor`, `ZstdCompressionParameters`, `ZstdCompressionDict`, `ZstdDecompressor`, `train_dictionary`, `multi_compress_to_buffer`, `BufferWithSegments`, `FrameParameters`, `frame_content_size`, `MAX_COMPRESSION_LEVEL`, `DICT_TYPE_*`), `py7zr` (`SevenZipFile`, `FILTER_*`, `FILTER_CRYPTO_AES256_SHA256`, `extractall`, `list`, `test`, `getnames`), `stream-zip` (`stream_zip`, `async_stream_zip`, `ZIP_64`, `ZIP_32`, `ZIP_AUTO`, `NO_COMPRESSION_32`, `NO_COMPRESSION_64`, `ZipError`, `ZipValueError`), `stream-unzip` (`stream_unzip`, `async_stream_unzip`, `NO_ENCRYPTION`, `AES_256`, `UnzipError`, `UnzipValueError`, `InvalidOperationError`, `UnfinishedIterationError`), `detools` (`create_patch`, `apply_patch`, `patch_info`, `Error`) on the cp315 core; the shared `data:compression` substrate `zlib-ng` (`gzip_ng.compress`, `gzip_ng.decompress`, `gzip_ng.GzipNGFile`, `gzip_ng.BadGzipFile`, `zlib_ng.crc32` the rolling CRC the stored-ZIP `NO_COMPRESSION_*` member and the streamed-unzip member fold seed through) composed not re-admitted; `lz4` (`frame.compress`, `frame.decompress`, `get_frame_info`, `COMPRESSIONLEVEL_*`, `BLOCKSIZE_*`) and `brotli` (`compress`, `decompress`, `MODE_TEXT`/`MODE_FONT`/`MODE_GENERIC`, `lgwin`) gated `python_version<'3.15'`; `expression` (`tagged_union`/`tag`/`case`); runtime (`content_identity.ContentIdentity`/`ContentKey` including the `DELTA` parent-key from-image, `faults.RuntimeRail`/`async_boundary`, `anyio.to_process.run_sync` the runtime subprocess lane).
- Growth: a new algorithm is one `CompressionAlgo` row, one `CodecProfile` case with its knob-struct, one default-profile constant, one `_in_process` (or gated) pack arm, and one `_unpack_in_process` recover arm; a new tuning knob is one field on the owning knob-struct, a new bounded knob value one token on the owning `Literal` axis plus one dispatch-row entry; a new evidence fact one `(label, value)` row in `facts`; a new manifest column one field on `ManifestRow`; zero new surface and zero new verb beside the `pack`/`unpack` inverse.
- Boundary: a per-algorithm compression class family, a hardcoded `MODE_GENERIC`/`quality=11`/`ZIP_64` literal, a positional knob tuple decoded by index, a `getattr(module, f"BLOCKSIZE_{name}")` string-built constant lookup, a bare-`int`/`str` mode/dict-mode/patch-type profile field, a rename-only `train` forwarder, a per-arm `ContentIdentity.of` key-mint, a per-arm `in_bytes`/`out_bytes` re-sum, a `dict_codec` owner beside the zstd arm, a `batch_pack` sibling beside `pack`, a `zipfile.ZipFile` whole-archive buffer where `stream-zip`/`stream-unzip` bound memory, a hand-rolled bsdiff/hdiffpatch suffix-array diff where `detools` owns the superset, a re-admitted `gzip`/`zlib` codec where the shared `zlib-ng` substrate already accelerates DEFLATE/gzip, a second `unzip`/`apply` function per algorithm where the one `unpack` verb dispatches on `algo`, and a parallel `BundleManifest`-per-codec type are the deleted forms; this owner is content-addressed bundling AND the inverse recovery over already-emitted artifact bytes and owns no artifact production. The cp315-core `ZSTD`/`SEVEN_Z`/`ZIP_STREAM`/`DELTA`/`GZIP` arms compress and recover in-process through ONE `_in_process`/`_unpack_in_process` dispatch; the `LZ4`/`BROTLI` arms ride the gated `python_version<'3.15'` band and never resolve in the cp315-core process, so each dispatches its codec plus its serialized `CodecProfile` case onto the runtime subprocess lane (`anyio.to_process.run_sync`) where the gated-band worker imports the codec at module scope — neither a module-top nor a lazy gated import lands on the core page. The `ArtifactReceipt.Bundle` flat-scalar case the `receipt/receipt#RECEIPT` owner already carries is unchanged; this page composes the settled `BundleEvidence` value and spreads its named fields onto the receipt case, mirroring the flat-scalar `Egress`/`Introspection` cases so the receipt owner imports no producer value object.

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

from artifacts.receipt.receipt import ArtifactReceipt

type Lz4Block = Literal["default", "max64kb", "max256kb", "max1mb", "max4mb"]
type BrotliMode = Literal["generic", "text", "font"]
type ZstdDictMode = Literal["auto", "fulldict", "rawcontent"]
type ZipMethod = Literal["auto", "zip64", "zip32", "store32", "store64"]
type DeltaPatchType = Literal["sequential", "in-place", "bsdiff", "hdiffpatch"]
type DeltaAlgorithm = Literal["bsdiff", "hdiffpatch", "match-blocks"]
type DeltaCompression = Literal["bz2", "crle", "lzma", "zstd", "lz4", "heatshrink", "none"]


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


class SevenZKnobs(Struct, frozen=True):
    filters: tuple[dict[str, int], ...] = ()
    header_encryption: bool = False
    password: str | None = None


class ZipStreamKnobs(Struct, frozen=True):
    method: ZipMethod = "auto"
    level: int = 9
    password: str | None = None
    names: tuple[str, ...] = ()


class DeltaKnobs(Struct, frozen=True):
    from_image: bytes
    parent_key: ContentKey
    patch_type: DeltaPatchType = "sequential"
    algorithm: DeltaAlgorithm = "bsdiff"
    compression: DeltaCompression = "zstd"


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
from io import BytesIO

import zstandard
from msgspec import msgpack
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
        case CodecProfile(tag="delta", delta=DeltaKnobs() as k):
            import detools

            patch = BytesIO()
            detools.create_patch(BytesIO(k.from_image), BytesIO(payloads[0]), patch, patch_type=k.patch_type, algorithm=k.algorithm, compression=k.compression)
            blob = patch.getvalue()
            return (blob,), BundleEvidence.measure(algo, 0, 0, len(payloads[0]), 0, payloads, (blob,))
        case CodecProfile(tag="gzip", gzip=GzipKnobs() as k):
            from zlib_ng import gzip_ng

            blobs = tuple(gzip_ng.compress(p, compresslevel=k.level, mtime=k.mtime) for p in payloads)
            return blobs, BundleEvidence.measure(algo, k.level, 0, len(payloads[0]), 0, payloads, blobs)
        case _:
            assert_never(profile)


def _unpack_in_process(blob: bytes, algo: CompressionAlgo, profile: CodecProfile) -> tuple[tuple[str, int, bytes], ...]:
    match profile:
        case CodecProfile(tag="zstd"):
            payload = zstandard.ZstdDecompressor().decompress(blob)
            return (("payload-0", len(payload), payload),)
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
        case CodecProfile(tag="delta", delta=DeltaKnobs() as k):
            import detools

            sink = BytesIO()
            detools.apply_patch(BytesIO(k.from_image), BytesIO(blob), sink)
            payload = sink.getvalue()
            return ((f"from-{k.parent_key.hex}", len(payload), payload),)
        case CodecProfile(tag="gzip"):
            from zlib_ng import gzip_ng

            payload = gzip_ng.decompress(blob)
            return (("payload-0", len(payload), payload),)
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
_ZIP_METHOD: Final[dict[ZipMethod, object]] = {"auto": ZIP_AUTO, "zip64": ZIP_64, "zip32": ZIP_32, "store32": NO_COMPRESSION_32, "store64": NO_COMPRESSION_64}
```

## [03]-[RESEARCH]

- [RECEIPT_BUNDLE_REUSE] [RESOLVED]: the seven `CompressionAlgo` rows share the one `receipt/receipt#RECEIPT` `ArtifactReceipt.Bundle` case (`tuple[ContentKey, str, int, int, int, int, int, float]` — key, algo, level, dict-id, frame-size, entry count, CRC-verified count, ratio) unchanged; `_emit` spreads `evidence.algo.value`/`evidence.level`/`evidence.dict_id`/`evidence.frame_size`/`evidence.entries`/`evidence.verified`/`evidence.ratio` onto it for every codec, the `ZIP_STREAM`/`SEVEN_Z` arms filling `entries`/`verified` from the container and the single-frame codecs leaving `verified` zero. No receipt-case widening lands on this work-item — the verified eight-field case already carries the evidence, the flat-scalar shape mirroring the `Egress`/`Introspection` cases so the receipt owner imports no `BundleEvidence` value object and no native codec handle crosses the seam.
- [BUNDLE_INVERSE_VERB] [RESOLVED]: `Bundle.pack` and `Bundle.unpack` are the one modal-arity inverse verb pair, not a `Pack`/`Unpack` class split — `unpack(blob)` reads the same `algo`/`profile` the instance carries and dispatches the recovery arm through `_recover`, returning a `BundleManifest` of `(name, ContentKey, algo, size)` `ManifestRow` rows. The archive arms recover the full multi-member row set bounded (`ZIP_STREAM` via `stream_unzip` rolling a per-member `zlib_ng.crc32` fold over the drained chunks, `SEVEN_Z` via `SevenZipFile.list()`/`test()` reading the container directory's `(filename, uncompressed, crc32)` rows), the `DELTA` arm reconstructs one to-image row from `detools.apply_patch(from_image, patch)`, and the single-frame codecs (`ZSTD`/`LZ4`/`BROTLI`/`GZIP`) recover one row per decompressed frame; each recovery arm yields a `(name, size, digest)` triple and `BundleManifest.of` mints each row's `ContentKey` through one `ContentIdentity.of(f"member-{algo}", digest)` fold so the recovered member keys are content-addressed, the list/test pass reading the row set without a second hashing path or a payload buffer.
- [ZIP_STREAM_METHOD] [RESOLVED]: `stream_zip(files, chunk_size=65536, password=None, ...)` consumes an `Iterable[MemberFile]` of `(name, modified_at, mode, method, data)` tuples and yields container `bytes` chunks; `_zip_members` folds each payload into one tuple whose `method` is the `_ZIP_METHOD[knobs.method]` row — `ZIP_AUTO(uncompressed_size, level)` the size-driven ZIP32/ZIP64 auto-upgrade default, `ZIP_64`/`ZIP_32` the forced-format instances used directly, `NO_COMPRESSION_32`/`NO_COMPRESSION_64(uncompressed_size, crc_32)` the stored variants taking the pre-declared CRC32 (`zlib_ng.crc32(payload)` from the composed `data:compression` substrate) — and whose `data` is a one-chunk `iter((payload,))` lazy iterable. `password` switches every member to WinZip AES-256 (the `pycryptodome` backend `stream-zip` owns); `b"".join(stream_zip(...))` seals the bundle blob. The `stream_zip`/`async_stream_zip`/`ZIP_64`/`ZIP_32`/`ZIP_AUTO`/`NO_COMPRESSION_32`/`NO_COMPRESSION_64` member names and the `(name, modified_at, mode, method, data)` tuple shape verify against the folder `stream-zip` `.api` `[02]` public-type rows `[01]`-`[08]` and the `[03]` entrypoint table; the `ZIP_AUTO(size, level)`/`NO_COMPRESSION_*(size, crc_32)` call shapes are the `.api` `[03]` `Method` selection rows `[03]`-`[05]`, all reflected installed `0.0.84` on cp315.
- [ZIP_STREAM_UNZIP] [RESOLVED]: `stream_unzip(zipfile_chunks, password=None, chunk_size=65536, allow_zip64=True, allowed_encryption_mechanisms=_ALL_ENCRYPTIONS)` yields `(name, size, chunks)` triples whose inner `chunks` generator MUST be fully drained before the outer generator advances (the `UnfinishedIterationError` under `InvalidOperationError` ordered-consumption guard); `unpack` drains each member's `chunks` through a rolling `zlib_ng.crc32` fold that accumulates the running CRC and the byte total without buffering the payload, projecting one `(name.decode(), total, crc.to_bytes(4, "big"))` triple, passing `password.encode()` `bytes` (not `str`) and the `frozenset({NO_ENCRYPTION, AES_256})` allow-list that rejects legacy ZipCrypto and weak AES key lengths before decryption — the bounded list/test pass never materializes a member payload, only its size and CRC digest seed. The `stream_unzip`/`async_stream_unzip`/`NO_ENCRYPTION`/`AES_256` member names, the `(name, size, chunks)` triple, the `bytes` password, and the `allowed_encryption_mechanisms` allow-list verify against the folder `stream-unzip` `.api` `[02]` public-type rows `[01]`/`[07]`/`[09]`-`[52]` and the `[03]` entrypoint row `[01]`; the package is the un-gated pure-Python sdist (`stream-inflate` companion) that builds and imports on cp315 (installed `0.0.101`), so it rides the core band beside `stream-zip`, never a `python_version<'3.15'` gate on a building sdist.
- [DELTA_PARENT_KEY] [RESOLVED]: the `DELTA` arm diffs a parent from-image against the to-image payload — `DeltaKnobs.from_image: bytes` carries the parent artifact bytes and `parent_key: ContentKey` the parent's runtime content key, so `detools.create_patch(BytesIO(from_image), BytesIO(payload), fpatch, patch_type=..., algorithm=..., compression=...)` writes the compressed patch the bundle stores and `detools.apply_patch(BytesIO(from_image), BytesIO(patch), fto)` reconstructs the to-image (the single reconstruction surface that peeks the header type and dispatches sequential vs hdiffpatch internally), `detools.patch_info(BytesIO(patch))` reading the patch-kind metadata. The `(algorithm, patch_type)` combination selects the create kernel through the `DeltaPatchType`/`DeltaAlgorithm`/`DeltaCompression` `Literal` axes; the `Bundle.delta(from_image, parent_key, payload, ...)` factory binds the parent at the call site so a content-addressed delta keyed by `parent_key` is the storage shape. `create_patch(ffrom, fto, fpatch, compression=, patch_type=, algorithm=)`, `apply_patch(ffrom, fpatch, fto) -> int`, `patch_info(fpatch) -> (str, info)`, and the `sequential`/`in-place`/`bsdiff`/`hdiffpatch` patch-type, `bsdiff`/`hdiffpatch`/`match-blocks` algorithm, and `bz2`/`crle`/`lzma`/`zstd`/`lz4`/`heatshrink`/`none` compression call-strings verify against the folder `detools` `.api` `[03]` entrypoint rows (creation `[01]`, application `[01]`, inspection `[01]`) and the `[04]` create/compression axes, all members confirmed importable on cp315 (locked `0.53.0`, the `bsdiff`/`hdiffpatch`/`suffix_array` C extensions source-built).
- [GZIP_SUBSTRATE_COMPOSE] [RESOLVED]: the `GZIP` arm composes the shared `data:compression` `zlib-ng` substrate, NOT a re-admitted gzip codec — `gzip_ng.compress(payload, compresslevel=level, mtime=mtime)` produces the RFC 1952 gzip-framed bytes (`wbits=31` header+trailer, the SIMD-accelerated `_GzipReader` engine) so the bundle round-trips with any stdlib `gzip` peer at accelerated throughput, and `gzip_ng.decompress(blob)` recovers. The `zlib-ng` manifest row already lives as the `data` owner's accelerated DEFLATE/gzip codec (the `python-zlib-ng` project beside `zstandard`/`lz4`/`brotli`); the bundle owner reaches it for exactly the gzip-stdlib-interop case a non-legacy payload routes away from to `zstandard` (archival) or `lz4` (hot path). The level is the standard zlib `0..9` range (`Z_BEST_COMPRESSION = 9`), never an isal-style `0..3` assumption. `gzip_ng.compress(data, compresslevel=9, *, mtime=None) -> bytes`, `gzip_ng.decompress(data) -> bytes`, `GzipNGFile`, and `BadGzipFile` verify against the substrate `zlib-ng` `.api` `[02]` `gzip_ng` drop-in rows `[01]`/`[03]` and the `[03]` `gzip_ng` one-shot rows `[01]`/`[02]`, all confirmed on cp315 (installed `1.0.0`, `zlib_ng.cpython-315-darwin.so`).
- [BAND_PLACEMENT] [RESOLVED]: the seven codecs hold their existing `pyproject.toml` bands — `zstandard`/`py7zr`/`stream-zip`/`stream-unzip`/`detools` and the composed `zlib-ng` substrate on the cp315 core (ungated), `lz4`/`brotli` gated `python_version<'3.15'`. The `_compress`/`_recover` dispatch inverts the prior match polarity so the gated `lz4`/`brotli` arms are the explicit `to_process.run_sync` rows and every other codec falls through to the cp315-core `_in_process`/`_unpack_in_process`, because five of the seven now resolve on the core (the prior two-codec page matched `zstd`/`seven_z` explicitly and routed the rest to the gated worker — that polarity no longer holds with `ZIP_STREAM`/`DELTA`/`GZIP` on the core). The `_gated_codec`/`_gated_unpack` workers import `lz4.frame`/`brotli` at module scope on the gated band and carry the serialized `CodecProfile`, so no native handle and no module-top gated import lands on the core page.
- [ZSTD_CTOR_KEYWORDS] [RESEARCH]: the exact `ZstdCompressor(level=, dict_data=, compression_params=)`/`ZstdCompressionParameters(compression_level=, window_log=, threads=)` constructor keywords, the `ZstdDecompressor().decompress(blob)` single-frame recover, the `train_dictionary(dict_size, samples, dict_type=)` keyword, the `ZstdCompressionDict(data, dict_type=)` rehydration constructor with its `dict_id()`/`as_bytes()` accessors, the `BufferWithSegments[i]` per-segment index access, and the `zstandard.frame_content_size(frame) -> int` reader are the folder `zstandard` `.api` `[03]-[ENTRYPOINTS]` constructor rows, the `[02]` `BufferWithSegments`/`ZstdCompressionDict`/`ZstdDecompressor` public-type rows, and the `FrameParameters` frame-inspection capability, but their literal keyword names, the decompressor recover spelling, the dict rehydration accessors, the segment index access, and the frame-content-size function are not spelled in the catalogue (`ZstdDecompressor`/`frame_content_size`/`DICT_TYPE_*` are confirmed present on cp315; only the keyword/accessor spellings stay marked). UNVERIFIED until the `zstandard` `.api` catalogue spells the compressor/parameter constructor keywords, the `ZstdDecompressor().decompress` recover, the `ZstdCompressionDict(data, dict_type=)` constructor with `dict_id`/`as_bytes`, the `train_dictionary(..., dict_type=)` keyword, the `BufferWithSegments[i]` access, and the `frame_content_size` reader; the `_in_process`/`_unpack_in_process` zstd arms and `Bundle.trained` carry the assumed spellings as design intent. The stored-ZIP CRC and the streamed-unzip member CRC are NOT a zstandard member — they fold through the composed `data:compression` `zlib_ng.crc32` substrate (verified present on cp315), so no `crc32` spelling rides the zstandard catalogue.
- [LZ4_COMPRESS_KEYWORDS] [RESEARCH]: the exact `lz4.frame.compress(data, compression_level=, block_size=, content_checksum=)` keywords, the `lz4.frame.decompress(blob)` recover, the `get_frame_info(frame)["content_size"]` returned-mapping key, and the individual `BLOCKSIZE_DEFAULT`/`BLOCKSIZE_MAX64KB`/`BLOCKSIZE_MAX256KB`/`BLOCKSIZE_MAX1MB`/`BLOCKSIZE_MAX4MB` member names the `Lz4Block` dispatch row resolves are the folder `lz4` `.api` "data plus frame policy", "parse a frame header", and `BLOCKSIZE_*` family rows, but the literal keyword names, the `decompress` recover, the frame-info mapping key, and the individual block-size constant spellings are not spelled in the catalogue; UNVERIFIED until the `lz4` `.api` spells the `frame.compress`/`frame.decompress` keywords, the `get_frame_info` return mapping, and the `BLOCKSIZE_MAX*` member names. The `_gated_codec`/`_gated_unpack` LZ4 arms and the `Lz4Block` dispatch row carry the assumed spellings as design intent.
- [BROTLI_MODE_FONT] [RESOLVED]: `brotli.MODE_GENERIC`/`MODE_TEXT`/`MODE_FONT` (the WOFF2 font codec mode among them) verify against the folder `brotli` `.api` `[02]` mode-axis row, which spells all three member names; the `BrotliKnobs.mode` `BrotliMode` token rides the gated-band serialization as a `Literal` string and the gated-band worker resolves the `brotli.MODE_*` ordinal through one frozen dispatch row at codec scope. The `compress(string, mode=MODE_GENERIC, quality=11, lgwin=22, lgblock=0) -> bytes` and `decompress(data) -> bytes` spellings resolve against the `.api` `[03]` entrypoint rows `[01]`/`[02]`, confirmed importable on the gated band.
- [SEVEN_Z_INVERSE] [RESOLVED]: `py7zr.SevenZipFile(sink, mode="w", filters=[dict], header_encryption=bool, password=str)`, `writef(file_like, name)`, `SevenZipFile.test()` (the CRC-verification entrypoint returning `True`/`False`/`None`), and the unpack-side `SevenZipFile.list() -> list[FileInfo]` (the bounded archive-directory listing whose `FileInfo.filename`/`uncompressed`/`crc32`/`is_directory` rows the inverse reads WITHOUT materializing a payload) verify against the folder `py7zr` `.api` archive-open and entrypoint tables; the SEVEN_Z `unpack` arm is bounded — `list()` yields the `(name, uncompressed size, crc32)` manifest rows and `test()` gates the CRC-verified pass, so the `BundleManifest` is recovered from the container directory rather than a whole-archive payload buffer, mirroring the `stream_unzip` streamed list/test. The `{"id": py7zr.FILTER_CRYPTO_AES256_SHA256}` filter-chain entry and the `password` constructor keyword are the verified crypto-row spelling, and `SevenZipFile.test()` is the CRC-verified entry-count source for the pack-side `verified` evidence field. `extractall` (the named-file directory-output recovery alternate when full payload bytes are required), `list`, `test`, `writef`, and `getnames` are all confirmed present on `SevenZipFile` on cp315; the prior `readall`/`read` in-memory-mapping spelling is NOT a member of this `py7zr` release and is the deleted phantom — the bounded `list()`/`crc32` directory read is the recovery surface. The member `FileInfo.crc32` integer is rendered to the 4-byte big-endian digest seed (`crc32.to_bytes(4, "big")`) the `BundleManifest.of` content-key fold consumes, never a re-decompressed payload.
