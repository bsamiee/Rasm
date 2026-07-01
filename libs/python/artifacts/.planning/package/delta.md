# [PY_ARTIFACTS_DELTA]

The binary diff/patch arm of the package close — incremental artifact delta bundles keyed by a parent content key. Where `package/codec#CODEC` compresses one payload against nothing and `package/archive#ARCHIVE` folds many payloads into one container, `Delta` diffs ONE to-image payload against a parent from-image and stores only the compressed patch, so an artifact that revises a near-identical predecessor ships the delta, not the whole blob. It COMPOSES the `package/codec#CODEC` `Bundle`/`CompressionAlgo`/`CodecProfile`/`BundleEvidence` scaffold rather than re-owning it: `DeltaKnobs` is the one `CodecProfile` knob struct the codec union carries for the `delta` case, and `_delta_in_process`/`_delta_unpack` are the worker arms the codec `_in_process`/`_unpack_in_process` dispatch delegates the `delta` tag to. `DeltaKnobs` owns the FULL `detools.create_patch` surface as named axes — the `(algorithm, patch_type)` create-kernel combination, the `suffix_array` construction choice, the `match_score`/`match_block_size`/`heatshrink_window`/`heatshrink_lookahead` tuning scalars, the `InPlaceSegments` flash-segmentation band, and the `FirmwareLayout` ELF/AArch64/Cortex-M4/Xtensa data-format band — so the patch types, algorithms, and compressions the axes advertise are each fully configurable rather than a half-built case. Reconstruction keys on the patch HEADER type through the `_delta_apply` dispatch — `apply_patch` for the self-describing `sequential`/`hdiffpatch` header, `apply_patch_in_place` for the in-place memory image, `apply_patch_bsdiff` for the raw `BSDIFF40` patch — and the same kernel re-applies the just-minted patch at pack time so `verified` carries a real round-trip integrity proof, never a stored patch trusted blind. `_header` then peeks that same self-describing header through `detools.patch_info` and reads the reconstructed `to_size`, the encoded compression name, and the firmware `dfpatch` proof straight off the bytes `create_patch` wrote, so `frame_size` is the header-declared reconstructed size and `verified` witnesses BOTH the round-trip and a header that agrees with the requested codec and firmware band — the receipt reads the stored fact rather than re-inferring it. The content key threads the `DELTA` parent-key per the ARCHITECTURE seam (`package/delta ← python:runtime [CONTENT_KEY]: DELTA parent-key from-image`) — the from-image is the only side-input the delta round-trip needs, so a content-addressed delta keyed by `parent_key` is the storage shape. `detools` owns the superset diff engine (`bsdiff`/`hdiffpatch`/`suffix_array` native extensions) — a hand-rolled bsdiff or hdiffpatch suffix-array diff is the deleted form.

## [01]-[INDEX]

- [02]-[DELTA]: the `Delta` binary diff/patch concern over the `package/codec#CODEC` `CompressionAlgo.DELTA` row, the `DeltaKnobs` frozen policy struct owning the full `detools.create_patch` axis set, the `InPlaceSegments`/`FirmwareLayout` bands projecting through `kwargs()` to the flat `detools` offset names, the `DeltaPatchType`/`DeltaAlgorithm`/`DeltaCompression`/`DeltaSuffixArray`/`DataFormat` closed `Literal` axes, the `_delta_apply` patch-header-keyed reconstruction kernel (`apply_patch`/`apply_patch_in_place`/`apply_patch_bsdiff`) shared by the round-trip verify and the unpack recovery, the `_header`/`_TO_SIZE_AT` `detools.patch_info` header peek folding header-read `to_size`/compression/firmware-`dfpatch` proof into a two-part `verified` verdict, the `parent_key`-keyed content-addressed storage seam, and the `_delta_in_process`/`_delta_unpack` worker arms the codec `_in_process`/`_unpack_in_process` dispatch delegates the `delta` tag to; `detools` `create_patch`(`patch_type`/`algorithm`/`compression`/`suffix_array_algorithm`/`match_score`/`match_block_size`/`heatshrink_*`/`data_format`+`from_*`/`to_*`)/`apply_patch`/`apply_patch_in_place`/`apply_patch_bsdiff`/`patch_info`/`data_format_from_files` and the `xxhash.xxh3_128_digest` recovered-to-image seam settled against the both-tier `.api`, contributing the shared `core/receipt#RECEIPT` `ArtifactReceipt.Bundle` `frame_size`/`verified` slots.

## [02]-[DELTA]

- Owner: `Delta` the binary diff/patch concern over the one `CompressionAlgo.DELTA` row, composing the `package/codec#CODEC` `Bundle` owner; `DeltaKnobs` the frozen delta policy struct (`from_image: bytes` the parent artifact bytes, `parent_key: ContentKey` the parent's runtime content key, plus the full `create_patch` axis set — `patch_type`/`algorithm`/`compression`/`suffix_array` selectors and `match_score`/`match_block_size`/`heatshrink_window`/`heatshrink_lookahead` tuning scalars); `InPlaceSegments` the frozen in-place flash-segmentation band (`memory_size`/`segment_size`/`minimum_shift_size`) carried `in_place: InPlaceSegments | None` and `FirmwareLayout` the frozen firmware data-format band (`data_format` plus the six `from_*`/`to_*` `tuple[int, int]` offset ranges) carried `firmware: FirmwareLayout | None`, each projecting through its own `kwargs()` to the flat `detools` kwarg names at the edge; `DeltaPatchType`/`DeltaAlgorithm`/`DeltaCompression`/`DeltaSuffixArray`/`DataFormat` the five closed `Literal` axes whose call-strings ride straight into the `detools` call rows. `Delta` owns no `Bundle`/`CompressionAlgo`/`CodecProfile`/`BundleEvidence`/`BundleManifest` type — those are the `package/codec#CODEC` scaffold this page extends through the one `CodecProfile` case and the two worker arms, so a delta bundle is one row on the one union, never a parallel delta owner.
- Cases: `CompressionAlgo.DELTA` binds `detools.create_patch`/`_delta_apply` reading the `delta` `DeltaKnobs` profile case — matched by the codec `_in_process`/`_unpack_in_process` dispatch falling through to `_delta_in_process`/`_delta_unpack`, never an `if delta` branch. The `(algorithm, patch_type)` pair selects the create kernel through the six `detools` combinations: `bsdiff`×`sequential` (the default sequential patch), `bsdiff`×`in-place` (the flash-segmented patch reading the `in_place` band), `bsdiff`×`bsdiff` (the raw `BSDIFF40` patch), `hdiffpatch`×`hdiffpatch` (the smaller hdiffpatch patch tuned by `match_score`), and `match-blocks`×`sequential`/`hdiffpatch` (the low-memory create tuned by `match_block_size`) — every other pair is a `detools` "Bad algorithm and patch type combination" raise the codec `async_boundary` converts at the seam. The `DeltaSuffixArray` (`"divsufsort"`/`"sais"`) axis selects the bsdiff suffix-array construction (`divsufsort` faster, `sais` lower-memory), `match_score` tunes the hdiffpatch algorithm, `match_block_size` tunes match-blocks, and `heatshrink_window`/`heatshrink_lookahead` tune the heatshrink codec — detools reads only the axes the selected `(algorithm, patch_type, compression)` exercises and ignores the rest, so every axis carries its `detools` default and a tuning value is one named field, never a positional row decoded by index. `use_mmap` is pinned `False` at the call rather than carried on `DeltaKnobs` — it is a boundary fact, not a knob: the `BytesIO` ingress exposes no `fileno()`, and the `hdiffpatch`/`match-blocks` create kernels mmap their inputs with no heap fallback, so the `bsdiff`/`sequential` path's mmap-then-`io.UnsupportedOperation`-to-heap fallback would not save the other create kernels — every diff path reads the in-memory buffer through `file_read`. The axes ARE the `detools` call-row strings, so the serialized `DeltaKnobs` carries no native patch-kind handle and the profile stays content-key-foldable.
- Apply: the `DELTA` arm is single-image diff/apply against a parent — `Bundle.delta(from_image, parent_key, payload, ...)` (the parent-binding factory the `package/codec#CODEC` owner declares) binds the parent at the call site so the delta keys by `parent_key`, and a single to-image payload is the only modality (a corpus diff is N parent-bound deltas, never one). `_delta_in_process` runs `detools.create_patch(BytesIO(from_image), BytesIO(payload), fpatch, ...)` spreading the always-present axes plus the `in_place.kwargs()`/`firmware.kwargs()` bands, writing the compressed patch the bundle stores. `_delta_apply` is the ONE patch-header-keyed reconstruction kernel both the round-trip verify and the unpack recovery share: it dispatches on `DeltaKnobs.patch_type` — `apply_patch(BytesIO(from_image), BytesIO(patch), fto)` for `sequential`/`hdiffpatch` (the surface that peeks the header type and routes sequential vs hdiffpatch internally and reads any firmware dfpatch from the self-describing header), `apply_patch_in_place(fmem, BytesIO(patch))` for `in-place` (where `fmem` is the `memory_size`-padded parent image the patch mutates, the recovered to-image the `to_size`-prefix the call returns, never the full memory buffer), and `apply_patch_bsdiff(BytesIO(from_image), BytesIO(patch), fto)` for the headerless raw `bsdiff` patch — so a single `apply_patch` call never stands in for the three reconstruction paths the three patch-header kinds demand. The parent `from_image` is the only side-input either leg needs; the recovered to-image is one `(name, size, digest)` row keyed `f"from-{parent_key.hex}"` so the recovered member traces its parent, feeding the codec `BundleManifest.of` content-key fold.
- Firmware: the `FirmwareLayout` band is the firmware-aware diff `detools` owns through `pyelftools` — the headline capability that subsumes `bsdiff4` — selecting per-architecture code/data segmentation through the `DataFormat` (`"arm-cortex-m4"`/`"aarch64"`/`"xtensa-lx106"`) token and the six `from_*`/`to_*` offset ranges, so a relocation-heavy firmware revision diffs against the parent with the moved code segments aligned rather than byte-shifted. `FirmwareLayout.kwargs()` expands the six `tuple[int, int]` ranges to the twelve flat `from_data_offset_begin`/`…`/`to_code_end` `detools` kwargs at the edge — the canonical owner carries the dense range pairs, the boundary maps to the provider's flat names — and `detools.data_format_from_files(option, elffile, binfile, offset)` is the ELF-offset resolver the caller binds into the ranges (it opens the ELF through `pyelftools` and returns the `from_*` offset tuple per image, the to-side a second call), never hand-computed offsets. The band rides the `bsdiff`/`sequential` create path where `create_patch_sequential_data` encodes the dfpatch into the patch header and `apply_patch_sequential` reads it back self-describing, so the unpack arm re-supplies no offsets; a `FirmwareLayout` on an `in-place` patch is the rejected combination because `detools`' in-place segment apply raises `NotImplementedError` on a dfpatch, surfaced loudly by the pack-time round-trip rather than a corrupt stored patch.
- Compression: the `DeltaCompression` axis selects the patch-payload codec row at create time — `bz2`/`crle`/`lzma`/`zstd`/`lz4`/`heatshrink`/`none` is a `compression` row on `create_patch`, never a parallel patch container; `apply_patch` resolves the codec from the self-describing patch header, so `_delta_apply` re-supplies neither the compression nor the algorithm, only the parent and the patch. The codec backends are the admitted siblings `lz4`/`zstandard`/`heatshrink2` and stdlib `bz2`/`lzma` that `detools` selects and frames — the delta arm never re-implements the codec, the `heatshrink_window`/`heatshrink_lookahead` axes thread into the `heatshrink2` compressor only on the `heatshrink` row, and the default `zstd` compression rides the same `zstandard` core the `package/codec#CODEC` `ZSTD` arm owns so the delta payload and the single-blob zstd path share one codec.
- Receipt: each delta pack contributes the `package/codec#CODEC` `BundleEvidence.measure` projection onto `core/receipt#RECEIPT` `ArtifactReceipt.Bundle` — the `frame_size` slot carries the reconstructed to-image size READ OFF THE PATCH HEADER through `detools.patch_info` (the `to_size` field `create_patch` wrote into the self-describing header), never re-inferred from the input length: the producer wrote the fact, the receipt reads the stored fact rather than trusting the raw input span. The `level` and `dict_id` slots stay zero (the patch compression is header-encoded, not a single integer level), and the `verified` slot carries a TWO-PART proof: `_delta_in_process` re-applies the just-minted patch through `_delta_apply` for the byte-identical round-trip AND `_header` cross-checks the header's self-description — the header `to_size` equals the recovered length, the header `compression` equals the requested `DeltaCompression`, and a firmware pack's header carries a non-zero `dfpatch_size` proving the segmentation rode the header — so `verified = 1` witnesses both the reconstruction and a header that agrees with itself. A raw headerless `bsdiff` patch has no `patch_info` header, so `_header` returns `None` and `verified`/`frame_size` fall back to the round-trip alone. A delta that does not reconstruct byte-identically OR whose header contradicts the request stores `verified = 0` and a downstream consumer reads the integrity verdict off the receipt rather than discovering the corruption on a future reconstruction. This is the delta-side counterpart to the `package/archive#ARCHIVE` container `test()` re-read — the single-blob `ZSTD`/`LZ4`/`BROTLI`/`GZIP` codecs own no such self-verification, the delta arm does because the parent-relative patch is the one codec whose output cannot be trusted without applying it. `_delta_in_process` returns the `(blobs, BundleEvidence)` pair the codec `_in_process` dispatch threads onto `_emit`, so the delta arm mints no receipt of its own; the shared `BundleEvidence` value crosses no module boundary into the receipt owner — the codec `_emit` spreads its named fields onto the flat-scalar `Bundle` case. The header-read compression name and the firmware `dfpatch_info` proof richer than the flat eight-scalar `Bundle` case can hold are folded INTO the `verified` verdict here rather than widening the shared receipt case, which `package/codec#CODEC` legislates unchanged across every codec.
- Growth: a new patch type is one token on the `DeltaPatchType` `Literal` plus one `_delta_apply` reconstruction arm; a new diff algorithm is one token on `DeltaAlgorithm`; a new patch-payload codec is one token on `DeltaCompression`; a new firmware architecture is one token on `DataFormat` (the segmentation `detools` already owns through `pyelftools`); a new suffix-array constructor is one token on `DeltaSuffixArray`; a new create-kernel tuning knob is one named field on `DeltaKnobs`; a new in-place or firmware offset is one field on the owning band struct; a new delta evidence fact rides the existing receipt slots. The `InPlaceSegments`/`FirmwareLayout` bands are the anticipatory collapse already absorbed — the in-place and firmware concerns the patch types and algorithms imply land as configured bands here, not as growth — so zero new surface and zero new verb beside the two worker arms, the one apply kernel, the one profile struct, and its two bands.

```python signature
from io import BytesIO
from typing import Final, Literal, assert_never

import xxhash

from builtins import frozendict
from msgspec import Struct

from rasm.runtime.content_identity import ContentKey

lazy from artifacts.package.codec import BundleEvidence, CodecProfile, CompressionAlgo  # cyclic owner: codec eager-imports these workers for DEFAULT_PROFILE, so this back-edge defers (used only in the worker bodies) to break the import cycle

lazy import detools

type DeltaPatchType = Literal["sequential", "in-place", "bsdiff", "hdiffpatch"]
type DeltaAlgorithm = Literal["bsdiff", "hdiffpatch", "match-blocks"]
type DeltaCompression = Literal["bz2", "crle", "lzma", "zstd", "lz4", "heatshrink", "none"]
type DeltaSuffixArray = Literal["divsufsort", "sais"]
type DataFormat = Literal["arm-cortex-m4", "aarch64", "xtensa-lx106"]


class InPlaceSegments(Struct, frozen=True):
    memory_size: int
    segment_size: int
    minimum_shift_size: int | None = None  # detools defaults 2 * segment_size; memory_size must be a multiple of segment_size

    def kwargs(self) -> dict[str, int | None]:
        return {"memory_size": self.memory_size, "segment_size": self.segment_size, "minimum_shift_size": self.minimum_shift_size}


class FirmwareLayout(Struct, frozen=True):
    data_format: DataFormat
    from_data_offset: tuple[int, int] = (0, 0)
    from_data: tuple[int, int] = (0, 0)
    from_code: tuple[int, int] = (0, 0)
    to_data_offset: tuple[int, int] = (0, 0)
    to_data: tuple[int, int] = (0, 0)
    to_code: tuple[int, int] = (0, 0)

    def kwargs(self) -> dict[str, object]:  # dense range pairs project to the flat detools offset kwargs at the edge
        return {
            "data_format": self.data_format,
            "from_data_offset_begin": self.from_data_offset[0], "from_data_offset_end": self.from_data_offset[1],
            "from_data_begin": self.from_data[0], "from_data_end": self.from_data[1],
            "from_code_begin": self.from_code[0], "from_code_end": self.from_code[1],
            "to_data_offset_begin": self.to_data_offset[0], "to_data_offset_end": self.to_data_offset[1],
            "to_data_begin": self.to_data[0], "to_data_end": self.to_data[1],
            "to_code_begin": self.to_code[0], "to_code_end": self.to_code[1],
        }


class DeltaKnobs(Struct, frozen=True):
    from_image: bytes
    parent_key: ContentKey
    patch_type: DeltaPatchType = "sequential"
    algorithm: DeltaAlgorithm = "bsdiff"
    compression: DeltaCompression = "zstd"
    suffix_array: DeltaSuffixArray = "divsufsort"
    match_score: int = 6
    match_block_size: int = 64
    heatshrink_window: int = 8
    heatshrink_lookahead: int = 7
    in_place: InPlaceSegments | None = None
    firmware: FirmwareLayout | None = None


def _delta_apply(k: DeltaKnobs, patch: bytes) -> bytes:
    match k.patch_type:  # the patch HEADER kind selects the reconstruction surface, never one apply_patch for all
        case "in-place":
            memory = k.in_place.memory_size if k.in_place is not None else len(k.from_image)
            mem = BytesIO(k.from_image.ljust(memory, b"\x00"))
            to_size = detools.apply_patch_in_place(mem, BytesIO(patch))  # bind to_size BEFORE getvalue(): the call mutates mem in place, so getvalue() must read the POST-patch buffer, never the pre-patch snapshot a subscript-order eval would capture
            return mem.getvalue()[:to_size]
        case "bsdiff":
            sink = BytesIO()
            detools.apply_patch_bsdiff(BytesIO(k.from_image), BytesIO(patch), sink)
            return sink.getvalue()
        case "sequential" | "hdiffpatch":
            sink = BytesIO()
            detools.apply_patch(BytesIO(k.from_image), BytesIO(patch), sink)
            return sink.getvalue()
        case _ as unreachable:
            assert_never(unreachable)


# the `detools.patch_info` per-kind info-tuple slot carrying the reconstructed to-image size — the self-describing
# header field the receipt reads instead of inferring the to-image length; the raw headerless `bsdiff` patch is absent.
_TO_SIZE_AT: Final[frozendict[str, int]] = frozendict({"sequential": 6, "in-place": 7, "hdiffpatch": 3})


def _header(k: DeltaKnobs, blob: bytes) -> tuple[int, bool] | None:
    # peek the self-describing patch header for the reconstructed `to_size` AND prove it agrees with the requested
    # codec and firmware band — `detools.patch_info` reads `(patch_size, compression, ..., to_size)` off the header
    # the create kernel wrote, so the receipt reads the stored fact rather than re-deriving it. The raw `bsdiff`
    # patch (BSDIFF40, headerless) has no such header (`patch_info` raises "Bad patch type"), so it returns None and
    # the round-trip length is the sole to-image evidence; `firmware` proof is `dfpatch_size > 0` on the sequential header.
    if k.patch_type == "bsdiff":
        return None
    kind, info = detools.patch_info(BytesIO(blob))
    firmware_proven = k.firmware is None or (kind == "sequential" and info[3] > 0)
    return info[_TO_SIZE_AT[kind]], info[1] == k.compression and firmware_proven


def _delta_in_process(payloads: tuple[bytes, ...], algo: CompressionAlgo, profile: CodecProfile) -> tuple[tuple[bytes, ...], BundleEvidence]:
    match profile:
        case CodecProfile(tag="delta", delta=DeltaKnobs() as k):
            patch, bands = BytesIO(), {**(k.in_place.kwargs() if k.in_place is not None else {}), **(k.firmware.kwargs() if k.firmware is not None else {})}
            detools.create_patch(  # use_mmap=False: the BytesIO ingress has no fileno(), and the hdiffpatch/match-blocks create kernels mmap the input with no heap fallback
                BytesIO(k.from_image), BytesIO(payloads[0]), patch, use_mmap=False,
                compression=k.compression, patch_type=k.patch_type, algorithm=k.algorithm, suffix_array_algorithm=k.suffix_array,
                match_score=k.match_score, match_block_size=k.match_block_size,
                heatshrink_window_sz2=k.heatshrink_window, heatshrink_lookahead_sz2=k.heatshrink_lookahead, **bands,
            )
            blob = patch.getvalue()
            recovered = _delta_apply(k, blob)  # the round-trip proves the patch reconstructs the to-image byte-identically
            header = _header(k, blob)  # header-read to_size + codec/firmware agreement, never an inferred input length
            frame_size = header[0] if header is not None else len(recovered)  # the reconstructed to-image size the patch header declares
            verified = int(recovered == payloads[0] and frame_size == len(recovered) and (header[1] if header is not None else True))  # round-trip AND self-describing-header agreement
            return (blob,), BundleEvidence.measure(algo, 0, 0, frame_size, verified, payloads, (blob,))
        case _:
            raise ValueError(f"<non-delta-profile:{profile.tag}>")  # the codec dispatch routes only the delta tag here; assert_never is invalid on the non-exhausted CodecProfile family


def _delta_unpack(blob: bytes, algo: CompressionAlgo, profile: CodecProfile) -> tuple[tuple[str, int, bytes], ...]:
    match profile:
        case CodecProfile(tag="delta", delta=DeltaKnobs() as k):
            payload = _delta_apply(k, blob)
            return ((f"from-{k.parent_key.hex}", len(payload), xxhash.xxh3_128_digest(payload)),)  # the uniform 16-byte xxh3_128 content digest the codec `BundleManifest.of` member-key fold consumes — the runtime XXH3_128 identity family, aligned with `_walked` and both `package/archive` arms
        case _:
            raise ValueError(f"<non-delta-profile:{profile.tag}>")  # the codec dispatch routes only the delta tag here; assert_never is invalid on the non-exhausted CodecProfile family
```

## [03]-[RESEARCH]

- [DELTA_PARENT_KEY] [RESOLVED]: the `DELTA` arm diffs a parent from-image against the to-image payload — `DeltaKnobs.from_image: bytes` carries the parent artifact bytes and `parent_key: ContentKey` the parent's runtime content key, so `detools.create_patch(BytesIO(from_image), BytesIO(payload), fpatch, ...)` writes the compressed patch the bundle stores and `_delta_apply` reconstructs the to-image, the `Bundle.delta(from_image, parent_key, payload, ...)` factory (declared at `package/codec#CODEC`) binding the parent at the call site so a content-addressed delta keyed by `parent_key` is the storage shape, threading the ARCHITECTURE `package/delta ← python:runtime [CONTENT_KEY]: DELTA parent-key from-image` seam. The recovered member keys `f"from-{parent_key.hex}"` (the runtime `ContentKey.hex` property, source-verified), so each recovered to-image traces its parent through the codec `BundleManifest.of` fold.
- [DELTA_CREATE_SURFACE] [RESOLVED]: `DeltaKnobs` owns the full `detools.create_patch` axis set so every patch type, algorithm, and compression it advertises is configurable — `suffix_array` (`divsufsort` faster / `sais` lower-memory, the bsdiff suffix-array construction), `match_score` (the hdiffpatch match heuristic, recommended 0-4 binary / 4-9 text), `match_block_size` (the match-blocks low-memory create), and `heatshrink_window`/`heatshrink_lookahead` (the heatshrink codec window), each riding its named `create_patch` kwarg and ignored by `detools` when the selected `(algorithm, patch_type, compression)` does not exercise it. The `compression='lzma'`/`patch_type='sequential'`/`algorithm='bsdiff'`/`suffix_array_algorithm='divsufsort'`/`match_score=6`/`match_block_size=64`/`heatshrink_window_sz2=8`/`heatshrink_lookahead_sz2=7` defaults and the `(algorithm, patch_type)` validity matrix verify against the folder `detools` `.api` `[03]` `create_patch` call shape and the `[04]` create/in-place/data-format/compression axes; an out-of-matrix pair (e.g. `(match-blocks, in-place)`) is the `detools` "Bad algorithm and patch type combination" raise the codec `async_boundary` converts at the seam.
- [DELTA_FIRMWARE] [RESOLVED]: the `FirmwareLayout` band carries the firmware-aware diff `detools` owns through `pyelftools` — `DataFormat` is the three-token architecture vocabulary (`"arm-cortex-m4"`/`"aarch64"`/`"xtensa-lx106"`, the `detools` `DATA_FORMATS` registry, source-verified), and the six `from_*`/`to_*` `tuple[int, int]` ranges project through `FirmwareLayout.kwargs()` to the twelve flat `from_data_offset_begin`/`…`/`to_code_end` `create_patch` kwargs. The band rides the `bsdiff`/`sequential` create path where `create_patch_sequential_data` encodes the dfpatch into the patch header and `apply_patch_sequential` reads it back, so the unpack arm re-supplies no offsets; an `in-place` patch with a dfpatch is the rejected combination because `detools`' in-place segment apply raises `NotImplementedError`, caught loudly by the pack round-trip. `data_format` and the `from_*`/`to_*` offset rows verify against the folder `detools` `.api` `[03]` `create_patch` call shape and the `[04]` data-format axis; the offsets are tool-resolved from an ELF input via `data_format_from_files` (which wraps the `detools.data_format.elf.from_file` code/data range reader over `pyelftools`, returning the `from_*` offset tuple), bound into the `FirmwareLayout` ranges, never hand-computed — `add_data_format_args` is the detools CLI argparse helper, not a library resolver, and never rides this arm.
- [DELTA_ROUNDTRIP_VERIFY] [RESOLVED]: the parent-relative patch is the one codec whose output cannot be trusted without applying it, so `_delta_in_process` re-applies the just-minted patch through `_delta_apply` for the byte-identical round-trip AND `_header` reads the self-describing patch header through `detools.patch_info(BytesIO(blob))` — the `(kind, info)` return whose `info[_TO_SIZE_AT[kind]]` is the reconstructed `to_size` (slot 6 sequential / 7 in-place / 3 hdiffpatch, all `assay`-verified against `detools 0.53.0`), `info[1]` the header compression name, and `info[3] > 0` the sequential firmware `dfpatch_size` proof — so `verified = int(recovered == payloads[0] and frame_size == len(recovered) and header_agrees)` witnesses reconstruction AND header self-consistency, and `frame_size` is the header-declared `to_size` rather than the inferred `len(payloads[0])`. The raw headerless `bsdiff` patch is the one kind `patch_info` rejects ("Bad patch type", `assay`-confirmed), so `_header` returns `None` and both facts fall back to the round-trip length. This fills the `core/receipt#RECEIPT` `ArtifactReceipt.Bundle` `frame_size`/`verified` slots the single-blob codecs leave inferred/zero — the delta-side counterpart to the `package/archive#ARCHIVE` container `test()` re-read, `level`/`dict_id` staying zero (the patch compression is header-encoded, not a single integer level). The `detools.patch_info` per-kind info tuple verifies against the folder `detools` `.api` `[03]` inspection rows; a delta that does not reconstruct byte-identically OR whose header contradicts the requested codec/firmware stores `verified = 0` rather than a silently-corrupt patch. The header-read compression name and firmware `dfpatch_info` exceed the flat eight-scalar `Bundle` case, so they fold INTO the `verified` verdict rather than widening the shared receipt case `package/codec#CODEC` holds unchanged; the recovered-to-image digest is the uniform 16-byte `xxhash.xxh3_128_digest` every `BundleManifest.of` recovery arm computes (`package/codec#CODEC` `[BUNDLE_INVERSE_VERB]` `_walked` AND both `package/archive#ARCHIVE` arms — the ZIP `reduce` fold and the `_Xxh3Factory` sink), aligning the delta arm onto the runtime `XXH3_128` identity family `ContentIdentity.of` uses, never a divergent per-arm 32-byte `sha256` that would break the one-digest triple.
- [DELTA_CODEC_COMPOSE] [RESOLVED]: the `DeltaCompression` axis selects the patch-payload codec `detools` frames, NOT a re-implemented codec — `bz2`/`crle`/`lzma`/`zstd`/`lz4`/`heatshrink`/`none` (the `detools` `COMPRESSIONS` registry, source-verified) routes to the admitted siblings `lz4`/`zstandard`/`heatshrink2` and stdlib `bz2`/`lzma` that `detools` selects through the `compression` call row, and `apply_patch` resolves the codec from the self-describing patch header so `_delta_apply` re-supplies neither the compression nor the algorithm. The `detools` runtime deps `bitstruct`/`heatshrink2`/`humanfriendly`/`lz4`/`pyelftools`/`zstandard` verify against the `detools` `.api` `[01]` `Requires-Dist` rows; `detools` is the superset diff engine subsuming `bsdiff4` (its `bsdiff` algorithm + `apply_patch_bsdiff`) and adding `hdiffpatch`, in-place segmentation, and firmware data-format awareness, so a sibling codec is only the payload compressor `detools` selects, never an independent patch path. The default `zstd` patch compression rides the same `zstandard` core the `package/codec#CODEC` `ZSTD` arm owns, so the delta payload and the single-blob zstd path share one codec.
