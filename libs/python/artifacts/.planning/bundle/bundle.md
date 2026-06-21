# [PY_ARTIFACTS_BUNDLE]

The artifact-bundling and compression owner. `Bundle` packs any emitted artifact into content-addressed compressed bytes; `CompressionAlgo` is ONE algorithm-row owner collapsing zstandard, lz4, brotli, and py7zr — never a per-algorithm class family. `CodecProfile` is the ONE policy union carrying one typed knob-struct per codec case — a `ZstdKnobs` (`level`, `threads`, `window_log`, `dict_data`, `dict_mode`), an `Lz4Knobs` (`compression_level`, `block_size`, `content_checksum`), a `BrotliKnobs` (`mode`, `quality`, `lgwin`, `lgblock`), a `SevenZKnobs` (`filters`, `header_encryption`, `password`) — so a hardcoded literal beside a codec call is the deleted form, every match arm reads a named field rather than a positional tuple slot, and a latency-versus-ratio swap is one policy value. The packed bundle keys by the runtime content key over the compressed bytes, so a bundle of identical inputs at an identical algorithm and profile is a cache hit by reference. This is bundling, not visualization; it owns no artifact production, only the compression close over already-emitted bytes.

## [01]-[INDEX]

- [01]-[COMPRESSION]: algorithm-row compression, the per-codec `CodecProfile` policy union, dictionary-trained zstd, and the content-addressed bundle owner threading the typed compression-evidence receipt.

## [02]-[COMPRESSION]

- Owner: `Bundle` the one bundle owner; `CompressionAlgo` the closed `StrEnum` discriminating algorithm; `CodecProfile` the one `tagged_union` policy value carrying one typed knob-struct per algorithm case; `BundleEvidence` the typed compression receipt; the algorithm package is bound per row, never an `if zstd` branch.
- Cases: `CompressionAlgo` rows `ZSTD` (zstandard `ZstdCompressor`) · `LZ4` (lz4 `frame`) · `BROTLI` (brotli `compress`) · `SEVEN_Z` (py7zr `SevenZipFile`) — matched by `match`/`case`, each binding its algorithm package and reading its `CodecProfile` case; `CodecProfile` cases `zstd` (`ZstdKnobs`) · `lz4` (`Lz4Knobs`) · `brotli` (`BrotliKnobs`) · `seven_z` (`SevenZKnobs`) — one frozen knob-struct per algorithm row whose fields are named axes the match arm reads directly, the default profile per algorithm a frozen module constant. Every closed knob axis is a `Literal` vocabulary, not a bare ordinal: `Lz4Block`, `BrotliMode`, and `ZstdDictMode` carry the package-constant family as named tokens that resolve to the real `BLOCKSIZE_*`/`MODE_*`/`DICT_TYPE_*` value through one frozen dispatch row at codec scope, so a string-built `getattr` attribute lookup and a raw `int` profile field are the deleted forms and the serialized `CodecProfile` carries no native handle across the subprocess lane.
- Modality: `Bundle.pack` is the one modal-arity entrypoint — a singular payload keys one bundle, and a `*payloads` spread of two-or-more keys ONE Merkle-parent bundle through the zstd `multi_compress_to_buffer` threaded batch returning a `BufferWithSegments` the arm reads by segment index, discriminating on the spread arity recovered from the call, never a `batch` flag; the arity is the value's shape, so a single payload and a corpus fold one entrypoint. `_compress` is the single key-mint seam — `_in_process` and the gated worker each return `(blobs, evidence)` and `_compress` folds the `ContentIdentity.of` projection over the blobs once, never per-arm.
- Dictionary: the `ZSTD` arm reads `dict_data: bytes | None` and `dict_mode: ZstdDictMode` off its `ZstdKnobs` and rehydrates one `ZstdCompressionDict(dict_data, dict_type=_ZSTD_DICT[dict_mode])` at codec scope, so the trained dictionary crosses the subprocess lane as raw bytes rather than an unpicklable native handle and the `DICT_TYPE_AUTO`/`DICT_TYPE_FULLDICT`/`DICT_TYPE_RAWCONTENT` axis threads through the `Literal` token; `Bundle.trained` mines `zstandard.train_dictionary` over a many-similar-artifact corpus and binds the trained `as_bytes()` straight into a `ZSTD` `CodecProfile` at the call site, the dominant small-payload ratio win for receipts, glyph-runs, and repeated chart JSON, configured on the compressor root, never a parallel dict-codec owner or a rename-only `train` forwarder. `dict_id()` reads inline at the evidence mint, never through a single-call helper.
- Frame: the `ZSTD` arm parses its first compressed frame through `zstandard.frame_content_size`/`FrameParameters` so the receipt `frame_size` is the real decompressed-content size the frame header carries, never the compressed-byte length standing in for it; the level axis caps at `zstandard.MAX_COMPRESSION_LEVEL`.
- Crypto: the `SEVEN_Z` arm threads `password` plus a `FILTER_CRYPTO_AES256_SHA256` chain entry alongside `header_encryption=True`, so encryption is the functional three-field row the `.api` crypto axis mandates, never a lone `header_encryption` bool; `SevenZipFile.test` re-reads the written archive for the CRC-verified entry count the evidence carries.
- Receipt: each pack contributes `receipt/receipt#RECEIPT` `ArtifactReceipt.Bundle` carrying the content key and the `BundleEvidence` facts — algo, level, dictionary id, frame size, entry count, CRC-verified count, ratio — projected through the one `BundleEvidence.measure` constructor that folds `in_bytes`/`out_bytes` once; `_emit` spreads the named evidence fields onto the flat-scalar `ArtifactReceipt.Bundle` case, and the receipt owner's own `_facts` arm is the single string-map projector for the whole union, so `BundleEvidence` carries no second `facts` projection and the receipt owner imports no `BundleEvidence` value object (the receipt-side circular import the flattening forecloses). The `ratio` is the one observable-compression value the runtime `observability/metrics` `MeterProvider` compression-ratio signal stream reads off the receipt fold. The `measure` projector folds `in_bytes`/`out_bytes` from the payload and blob spans once, so the input-sum, output-sum, and entry-count construction lives on one row rather than re-derived per codec arm. The CRC-verified count is the 7z `test` archive proof and stays zero for the stream codecs that own no container-level CRC.
- Packages: `zstandard` (`ZstdCompressor`, `ZstdCompressionParameters`, `ZstdCompressionDict`, `train_dictionary`, `multi_compress_to_buffer`, `BufferWithSegments`, `FrameParameters`, `frame_content_size`, `MAX_COMPRESSION_LEVEL`, `DICT_TYPE_*`), `py7zr` (`SevenZipFile`, `FILTER_*`, `FILTER_CRYPTO_AES256_SHA256`) on the cp315 core; `lz4` (`frame.compress`, `get_frame_info`, `COMPRESSIONLEVEL_*`, `BLOCKSIZE_*`) and `brotli` (`compress`, `MODE_TEXT`/`MODE_FONT`/`MODE_GENERIC`, `lgwin`) gated `python_version<'3.15'`; `expression` (`tagged_union`/`tag`/`case`); runtime (`content_identity.ContentIdentity`/`ContentKey`, `faults.RuntimeRail`/`async_boundary`, `anyio.to_process.run_sync` the runtime subprocess lane).
- Growth: a new algorithm is one `CompressionAlgo` row, one `CodecProfile` case with its knob-struct, one default-profile constant, and one acceptor arm; a new tuning knob is one field on the owning knob-struct, a new bounded knob value one token on the owning `Literal` axis plus one dispatch-row entry; a new evidence fact one `(label, value)` row in `facts`; zero new surface.
- Boundary: a per-algorithm compression class family, a hardcoded `MODE_GENERIC`/`quality=11` literal, a positional knob tuple decoded by index, a `getattr(module, f"BLOCKSIZE_{name}")` string-built constant lookup, a bare-`int` mode/dict-mode profile field, a rename-only `train` forwarder, a per-arm `ContentIdentity.of` key-mint, a per-arm `in_bytes`/`out_bytes` re-sum, a `dict_codec` owner beside the zstd arm, and a `batch_pack` sibling beside `pack` are the deleted forms; this owner is content-addressed bundling over already-emitted artifact bytes and owns no artifact production. The cp315-core `ZSTD`/`SEVEN_Z` arms compress in-process through ONE `_in_process` dispatch; the `LZ4`/`BROTLI` arms ride the gated `python_version<'3.15'` band and never resolve in the cp315-core process, so each dispatches its codec plus its serialized `CodecProfile` case onto the runtime subprocess lane (`anyio.to_process.run_sync`) where the gated-band worker imports the codec at module scope — neither a module-top nor a lazy gated import lands on the core page. The `ArtifactReceipt.Bundle` case-tuple widening to carry the flat `(algo, level, dict_id, frame_size, entries, verified, ratio)` evidence scalars lands on the same-folder `receipt/receipt#RECEIPT` owner; this page composes the settled `BundleEvidence` value and spreads its named fields onto the receipt case, mirroring the flat-scalar `Egress`/`Introspection` cases so the receipt owner imports no producer value object.

```python signature
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


class CompressionAlgo(StrEnum):
    ZSTD = "zstd"
    LZ4 = "lz4"
    BROTLI = "brotli"
    SEVEN_Z = "7z"


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


@tagged_union(frozen=True)
class CodecProfile:
    tag: Literal["zstd", "lz4", "brotli", "seven_z"] = tag()
    zstd: ZstdKnobs = case()
    lz4: Lz4Knobs = case()
    brotli: BrotliKnobs = case()
    seven_z: SevenZKnobs = case()


DEFAULT_PROFILE: Final[dict[CompressionAlgo, CodecProfile]] = {
    CompressionAlgo.ZSTD: CodecProfile(zstd=ZstdKnobs()),
    CompressionAlgo.LZ4: CodecProfile(lz4=Lz4Knobs()),
    CompressionAlgo.BROTLI: CodecProfile(brotli=BrotliKnobs()),
    CompressionAlgo.SEVEN_Z: CodecProfile(seven_z=SevenZKnobs()),
}


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

    async def pack(self) -> RuntimeRail[tuple[ContentKey, ArtifactReceipt]]:
        return await async_boundary(f"bundle.{self.algo}", self._emit)

    async def _emit(self) -> tuple[ContentKey, ArtifactReceipt]:
        keys, evidence = await self._compress()
        key = keys[0] if len(keys) == 1 else ContentIdentity.of(f"bundle-{self.algo}", keys)
        return key, ArtifactReceipt.Bundle(key, evidence.algo.value, evidence.level, evidence.dict_id, evidence.frame_size, evidence.entries, evidence.verified, evidence.ratio)

    async def _compress(self) -> tuple[tuple[ContentKey, ...], BundleEvidence]:
        match self.profile:
            case CodecProfile(tag="zstd") | CodecProfile(tag="seven_z"):
                blobs, evidence = _in_process(self.payloads, self.algo, self.profile)
            case _:
                framed = await to_process.run_sync(_gated_codec, self.algo.value, msgpack.encode(self.profile), self.payloads)
                blobs, evidence = framed[:-1], msgpack.decode(framed[-1], type=BundleEvidence)
        return tuple(ContentIdentity.of(f"bundle-{self.algo}", b) for b in blobs), evidence
```

```python signature
from io import BytesIO

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


_ZSTD_DICT: Final[dict[ZstdDictMode, int]] = {"auto": zstandard.DICT_TYPE_AUTO, "fulldict": zstandard.DICT_TYPE_FULLDICT, "rawcontent": zstandard.DICT_TYPE_RAWCONTENT}
```

## [03]-[RESEARCH]

- [RECEIPT_BUNDLE_WIDENING] [RESOLVED]: the `ArtifactReceipt.Bundle` case carries `tuple[ContentKey, str, int, int, int, int, int, float]` (key, algo, level, dict-id, frame-size, entry count, CRC-verified count, ratio) on the same-folder `receipt/receipt#RECEIPT` owner, and `_emit` spreads `evidence.algo.value`/`evidence.level`/`evidence.dict_id`/`evidence.frame_size`/`evidence.entries`/`evidence.verified`/`evidence.ratio` onto it. The widening is flat scalars, not `tuple[ContentKey, BundleEvidence]`: `bundle.py` imports `ArtifactReceipt`, so a `receipt.py` import of `BundleEvidence` would close a module-scope cycle, and the flat-scalar case mirrors the `Egress`/`Introspection` cases that flatten producer evidence for exactly that reason — the `verdict` value-object pattern is acyclic only because `conformance.py` never imports `ArtifactReceipt`, which `bundle.py` does. The receipt owner's own `_facts` arm is the single string-map projector for the flat scalars; `BundleEvidence` carries no second `facts` projection.
- [BROTLI_MODE_FONT] [RESOLVED]: `brotli.MODE_GENERIC`/`brotli.MODE_TEXT`/`brotli.MODE_FONT` (the WOFF2 font codec mode among them) verify against the folder `brotli` `.api` `[02]` row `[04]` mode-axis, which spells all three member names; the `BrotliKnobs.mode` `BrotliMode` token rides the gated-band serialization as a `Literal` string and the gated-band worker resolves the `brotli.MODE_*` ordinal through one frozen dispatch row at codec scope, serving the `fonttools` WOFF2 egress through the same profile axis as the generic and text payload classes, so no native ordinal crosses the subprocess lane. The `compress(string, mode=MODE_GENERIC, quality=11, lgwin=22, lgblock=0) -> bytes` spelling resolves against the `.api` `[03]` row `[01]` entrypoint table.
- [ZSTD_BATCH_AND_DICT] [RESOLVED]: `zstandard.ZstdCompressor` (constructor compression policy), `ZstdCompressor.compress(buffer)`, `ZstdCompressor.multi_compress_to_buffer(buffer batch, threads)` (the threaded batch over a many-payload corpus returning a per-payload-indexed `BufferWithSegments`), `BufferWithSegments` (the batch buffer public type, row `[10]`), `zstandard.train_dictionary(dict_size, samples)` → `ZstdCompressionDict`, `ZstdCompressionParameters` (the advanced-tuning parameter object), `FrameParameters` (the parsed frame-header view), `MAX_COMPRESSION_LEVEL` (the level cap the `level` axis clamps to), and the `DICT_TYPE_AUTO`/`DICT_TYPE_FULLDICT`/`DICT_TYPE_RAWCONTENT` dict-mode axis verify against the folder `zstandard` `.api` entrypoint and public-type tables (rows `[02]`/`[05]`-`[07]`/`[10]` and the `[COMPRESSION_ZSTD]` dictionary axis); the dictionary id and frame size are the receipt evidence the `.api` `[COMPRESSION_ZSTD]` evidence line mandates. The threaded-batch dispatch over `len(payloads) > 1` is the `multi_compress_to_buffer` modality row, the single-payload path the `compress` row.
- [ZSTD_CTOR_KEYWORDS] [RESEARCH]: the exact `ZstdCompressor(level=, dict_data=, compression_params=)` and `ZstdCompressionParameters(compression_level=, window_log=, threads=)` constructor keyword spellings, the `train_dictionary(dict_size, samples, dict_type=)` keyword, the `ZstdCompressionDict(data, dict_type=)` rehydration constructor, the `ZstdCompressionDict.dict_id()`/`ZstdCompressionDict.as_bytes()` accessors, the `BufferWithSegments` per-segment index access (`segments[i] -> buffer`), and the module-level `zstandard.frame_content_size(frame) -> int` decompressed-content-size reader are the `.api` `[03]-[ENTRYPOINTS]` "Constructor rows carry level, dictionary, parameter, format, and thread policy" rows, the `[02]` `BufferWithSegments`/`ZstdCompressionDict` public-type rows, and the `FrameParameters` "frame inspection" capability, but their literal keyword names, the dict rehydration constructor and `as_bytes`/`dict_id` accessors, the segment index access, and the frame-content-size function are not spelled in the catalogue. UNVERIFIED until the `zstandard` `.api` catalogue spells the compressor and parameter constructor keyword arguments, the `ZstdCompressionDict(data, dict_type=)` constructor with its `dict_id`/`as_bytes` accessors, the `train_dictionary(..., dict_type=)` keyword, the `BufferWithSegments[i]` segment access, and the `frame_content_size`/`FrameParameters.content_size` frame-size reader; the `_in_process` zstd arm and `Bundle.trained` carry the assumed spellings as design intent, never as confirmed against the catalogue.
- [LZ4_FRAME_KNOBS] [RESOLVED]: `lz4.frame.compress` (the one-shot framed compress "data plus frame policy"), `lz4.frame.get_frame_info` (the frame-header parser, ENTRYPOINTS row `[08]`), the `BLOCKSIZE_*` frame-max-block-size axis, and the `COMPRESSIONLEVEL_*` level-bound axis verify against the folder `lz4` `.api` frame entrypoint and frame-constants tables; the `Lz4Block` `Literal` axis maps each token through one frozen dispatch row at gated-band boundary scope rather than a string-built `getattr`, the frame-vs-block axis fixed at the verified `frame` submodule.
- [LZ4_COMPRESS_KEYWORDS] [RESEARCH]: the exact `lz4.frame.compress(data, compression_level=, block_size=, content_checksum=)` keyword spellings, the `get_frame_info(frame)["content_size"]` returned-mapping key, and the individual `BLOCKSIZE_DEFAULT`/`BLOCKSIZE_MAX64KB`/`BLOCKSIZE_MAX256KB`/`BLOCKSIZE_MAX1MB`/`BLOCKSIZE_MAX4MB` member names the `Lz4Block` dispatch row resolves are the `.api` "data plus frame policy", "parse a frame header", and `BLOCKSIZE_*` family rows, but the literal keyword argument names, the frame-info mapping key, and the individual block-size constant spellings are not spelled in the catalogue; UNVERIFIED until the `lz4` `.api` spells the `frame.compress` keyword arguments, the `get_frame_info` return mapping, and the `BLOCKSIZE_MAX*` member names. The `_gated_codec` LZ4 arm and the `Lz4Block` dispatch row carry the assumed spellings as design intent.
- [SEVEN_Z_FILTERS] [RESOLVED]: `py7zr.SevenZipFile(sink, mode="w", filters=[dict], header_encryption=bool, password=str)`, `writef(file_like, name)`, and `SevenZipFile.test()` (the CRC-verification entrypoint, row `[09]`) verify against the folder `py7zr` `.api` archive-open and entrypoint tables; the `FILTER_*` table is the `filters` row source consumed as a `list[dict]` filter-chain value, never a per-codec archive type, and `header_encryption` plus `FILTER_CRYPTO_AES256_SHA256` plus `password` are the three encryption rows on the archive root (the `[COMPRESSION_7Z]` crypto axis). The `{"id": py7zr.FILTER_CRYPTO_AES256_SHA256}` filter-chain entry and the `password` constructor keyword are the verified crypto-row spelling; `SevenZipFile.test()` returning `True`/`False`/`None` is the CRC-verified entry-count source for the `verified` evidence field.
