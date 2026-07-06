# [PY_ARTIFACTS_DELTA]

The binary diff/patch PRODUCER — incremental artifact deltas keyed by a parent content key, composing the `package/bundle#BUNDLE` vocabulary downward and importing no sibling. Where `package/codec#CODEC` compresses one payload against nothing and `package/archive#ARCHIVE` folds many payloads into one container, `Delta` diffs ONE to-image payload against a parent from-image and stores only the compressed patch, so an artifact revising a near-identical predecessor ships the delta, not the whole blob. The entry is `emit() -> ArtifactWork` per the one producer contract, and the parent-keyed row is structural: the node's `parents` name the base bundle key (`DeltaKnobs.parent_key`), so the plan graph carries the base→patch dependency and a re-issued identical revision elides pre-run (`Admission(keyed=None)`, `receipt.slot == node.key`). `DeltaKnobs` owns the FULL `detools.create_patch` surface as named axes — the `(algorithm, patch_type)` create-kernel matrix, `suffix_array` construction, the `match_score`/`match_block_size`/`heatshrink_*` tuning scalars, the `InPlaceSegments` flash band, and the `FirmwareLayout` ELF data-format band (all bundle-page vocabulary) — and reconstruction keys on the patch HEADER kind through `_delta_apply` (`apply_patch` for the self-describing `sequential`/`hdiffpatch` header, `apply_patch_in_place` for the memory image, `apply_patch_bsdiff` for the raw `BSDIFF40`), with the same kernel re-applying the just-minted patch at pack time so `verified` witnesses a real round-trip plus header self-consistency, never a stored patch trusted blind. `detools` owns the superset diff engine — a hand-rolled bsdiff or suffix-array diff is the deleted form; the create/apply bodies release the GIL, so the offload rides `Modality.THREAD` under `retry=RetryClass.OCCT`.

## [01]-[INDEX]

- [02]-[DELTA]: the `Delta` producer over the `CompressionAlgo.DELTA` row — `of` the parent-binding construction, `emit()`/`_emit` the node contract with the parent-keyed `parents` row, `unpack` the reconstruction inverse, `Delta.pack`/`Delta.recover` the `PackWorker` port kernels, the `_delta_apply` patch-header-keyed reconstruction kernel shared by round-trip verify and recovery, the `_header`/`_TO_SIZE_AT` `detools.patch_info` header peek folding header-read `to_size`/compression/firmware-`dfpatch` proof into the two-part `verified` verdict; `detools` `create_patch`(`patch_type`/`algorithm`/`compression`/`suffix_array_algorithm`/`match_score`/`match_block_size`/`heatshrink_*`/data-format bands)/`apply_patch`/`apply_patch_in_place`/`apply_patch_bsdiff`/`patch_info` and the `xxhash.xxh3_128_digest` recovered-image digest settled against the both-tier `.api`, contributing the one `core/receipt#RECEIPT` `ArtifactReceipt.Bundle` case and a `core/plan#PLAN` `ArtifactWork` node.

## [02]-[DELTA]

- Owner: `Delta` the one diff/patch producer wrapping the `package/bundle#BUNDLE` `Bundle` carrier with a `delta`-case profile; `DeltaKnobs`/`InPlaceSegments`/`FirmwareLayout` and the five `Literal` axes are bundle-page vocabulary — this page owns only the create/apply arms, so a delta bundle is one profile row on the one union, never a parallel delta owner. `Delta.pack`/`Delta.recover` are the page's `PackWorker` port kernels, total over the one row.
- Entry: `Delta.of(from_image, parent_key, payload, ...)` binds the parent at construction — the profile carries `from_image` (the only side-input either leg needs) and the built node reads `parents=(parent_key,)` off the carrier, so a content-addressed delta keyed by its parent is the storage AND the graph shape (the runtime `[CONTENT_KEY]` DELTA seam). `emit()` returns ONE `ArtifactWork(key=self.bundle.key, work=self._emit, parents=(parent_key,), admission=Admission(keyed=None), cost=byte-volume)`; `_emit` offloads `Delta.pack` and maps the rail onto `evidence.receipt(self.bundle.key)`; `unpack(blob)` reconstructs the to-image row through `Delta.recover` and `BundleManifest.of`, the recovered member named `f"from-{parent_key.hex}"` so it traces its parent.
- Cases: the `(algorithm, patch_type)` pair selects the create kernel through the `detools` matrix — `bsdiff`×`sequential` (default), `bsdiff`×`in-place` (flash-segmented, reading the `in_place` band), `bsdiff`×`bsdiff` (raw `BSDIFF40`), `hdiffpatch`×`hdiffpatch` (tuned by `match_score`), `match-blocks`×`sequential`/`hdiffpatch` (low-memory, tuned by `match_block_size`) — an out-of-matrix pair is the `detools` "Bad algorithm and patch type combination" raise the offload boundary rails. `detools` reads only the axes the selected combination exercises, so every axis carries its default and a tuning value is one named field. `use_mmap` is pinned `False` at the call, a boundary fact not a knob: the `BytesIO` ingress has no `fileno()` and the `hdiffpatch`/`match-blocks` kernels mmap with no heap fallback.
- Apply: `_delta_apply` is the ONE header-keyed reconstruction kernel both legs share — `apply_patch` peeks the self-describing header and routes sequential vs hdiffpatch internally (reading any firmware dfpatch back, so recovery re-supplies no offsets); `apply_patch_in_place` mutates the `memory_size`-padded parent image and returns the `to_size` prefix; `apply_patch_bsdiff` handles the headerless raw patch — never one `apply_patch` standing in for three header kinds. The firmware band rides only the `bsdiff`×`sequential` path (`create_patch_sequential_data` encodes the dfpatch); a `FirmwareLayout` on an `in-place` patch is the rejected combination (`detools` raises `NotImplementedError`), surfaced loudly by the pack-time round-trip rather than a corrupt stored patch.
- Compression: the `DeltaCompression` axis selects the patch-payload codec `detools` frames (`bz2`/`crle`/`lzma`/`zstd`/`lz4`/`heatshrink`/`none`) — the backends are the admitted siblings `lz4`/`zstandard`/`heatshrink2` plus stdlib, never a re-implemented codec; `apply_patch` resolves the codec from the header, so `_delta_apply` re-supplies neither compression nor algorithm; the default `zstd` rides the same `zstandard` core the codec `ZSTD` arm owns.
- Receipt: `frame_size` carries the reconstructed to-image size READ OFF THE PATCH HEADER through `detools.patch_info` (the `to_size` the create kernel wrote — slot per kind via `_TO_SIZE_AT`; the headerless raw `bsdiff` patch has no header, so `_header` returns `None` and the round-trip length is the fallback), never re-inferred from the input; `level`/`dict_id` stay zero (the patch compression is header-encoded, not an integer level); `verified` is the TWO-PART proof — byte-identical round-trip AND a header that agrees with itself (header `to_size` equals the recovered length, header compression equals the requested axis, a firmware pack's `dfpatch_size > 0`) — the delta-side counterpart of the archive `test()` re-read, because the parent-relative patch is the one codec whose output cannot be trusted without applying it. A delta that fails either part stores `verified = 0`, read off the receipt rather than discovered on a future reconstruction; the header-read facts richer than the flat eight-scalar case fold INTO the verdict, never a case widening.
- Growth: a new patch type is one bundle-page `DeltaPatchType` token plus one `_delta_apply` arm; a new diff algorithm/codec/architecture/suffix-array constructor is one token on its bundle-page axis; a new tuning knob is one named `DeltaKnobs` field; the in-place and firmware bands are the anticipatory collapse already absorbed — zero new verb beside the `emit`/`unpack` pair.
- Packages: `detools` (lazy — `create_patch`/`apply_patch*`/`patch_info`; runtime deps `bitstruct`/`heatshrink2`/`lz4`/`pyelftools`/`zstandard` ride it), `xxhash` (the recovered-image digest on the runtime identity family), `expression` (`Map.of_seq` the header-slot row), `msgspec` (`Struct`), runtime `identity`/`faults`/`lanes`/`resilience`, `artifacts.core.plan`/`core.receipt`/`package.bundle`.
- Boundary: no sibling import, no vocabulary re-own, no folder-minted limiter/retry caller, no CLI argparse plumbing (`data_format_args` is never a library resolver), no corpus modality (a corpus diff is N parent-bound nodes, never one), no receipt-case widening.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
import os
from io import BytesIO
from typing import Final, assert_never

import xxhash
from expression.collections import Map
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
    CodecProfile,
    CompressionAlgo,
    DeltaAlgorithm,
    DeltaCompression,
    DeltaKnobs,
    DeltaPatchType,
    DeltaSuffixArray,
    FirmwareLayout,
    InPlaceSegments,
    MemberTriple,
)

lazy import detools

# --- [CONSTANTS] ------------------------------------------------------------------------

# the detools create/apply bodies release the GIL: THREAD modality, the runtime THREAD_BAND owning the limiter.
_PACK_LANE: Final[LanePolicy] = LanePolicy(capacity=os.process_cpu_count() or 1)

# --- [TABLES] ---------------------------------------------------------------------------

# the `detools.patch_info` per-kind info-tuple slot carrying the reconstructed to-image size — the self-describing
# header field the receipt reads instead of inferring; the raw headerless `bsdiff` patch is absent by design.
_TO_SIZE_AT: Final[Map[str, int]] = Map.of_seq([("sequential", 6), ("in-place", 7), ("hdiffpatch", 3)])

# --- [MODELS] ---------------------------------------------------------------------------


class Delta(Struct, frozen=True):
    bundle: Bundle

    @staticmethod
    def of(
        from_image: bytes,
        parent_key: ContentKey,
        payload: bytes,
        *,
        patch_type: DeltaPatchType = "sequential",
        algorithm: DeltaAlgorithm = "bsdiff",
        compression: DeltaCompression = "zstd",
        suffix_array: DeltaSuffixArray = "divsufsort",
        match_score: int = 6,
        match_block_size: int = 64,
        heatshrink_window: int = 8,
        heatshrink_lookahead: int = 7,
        in_place: InPlaceSegments | None = None,
        firmware: FirmwareLayout | None = None,
    ) -> "Delta":
        # the parent binds at construction: the profile carries the from-image side-input and the node graph
        # carries the base bundle key as the one parent edge — the parent-keyed delta row.
        knobs = DeltaKnobs(
            from_image=from_image,
            parent_key=parent_key,
            patch_type=patch_type,
            algorithm=algorithm,
            compression=compression,
            suffix_array=suffix_array,
            match_score=match_score,
            match_block_size=match_block_size,
            heatshrink_window=heatshrink_window,
            heatshrink_lookahead=heatshrink_lookahead,
            in_place=in_place,
            firmware=firmware,
        )
        return Delta(bundle=Bundle.of(CompressionAlgo.DELTA, payload, profile=CodecProfile(delta=knobs), parents=(parent_key,)))

    def emit(self, /) -> ArtifactWork:
        return ArtifactWork(key=self.bundle.key, work=self._emit, parents=self.bundle.parents, admission=Admission(keyed=None), cost=self._cost)

    async def _emit(self, /) -> RuntimeRail[ArtifactReceipt]:
        packed = await _PACK_LANE.offload(
            Delta.pack, self.bundle.payloads, self.bundle.algo, self.bundle.profile, modality=Modality.THREAD, retry=RetryClass.OCCT
        )
        return packed.map(lambda pe: pe[1].receipt(self.bundle.key))

    async def unpack(self, blob: bytes, /) -> RuntimeRail[BundleManifest]:
        rows = await _PACK_LANE.offload(
            Delta.recover, blob, self.bundle.algo, self.bundle.profile, modality=Modality.THREAD, retry=RetryClass.OCCT
        )
        return rows.map(lambda recovered: BundleManifest.of(self.bundle.algo, recovered))

    @property
    def _cost(self) -> float:
        return float(sum(map(len, self.bundle.payloads)) or 1)  # byte-volume CPM weight

    @staticmethod
    def pack(payloads: tuple[bytes, ...], algo: CompressionAlgo, profile: CodecProfile, /) -> tuple[bytes, BundleEvidence]:
        match profile:
            case CodecProfile(tag="delta", delta=DeltaKnobs() as k):
                patch = BytesIO()
                bands = {**(k.in_place.kwargs() if k.in_place is not None else {}), **(k.firmware.kwargs() if k.firmware is not None else {})}
                detools.create_patch(  # use_mmap=False: BytesIO has no fileno(); hdiffpatch/match-blocks mmap with no heap fallback
                    BytesIO(k.from_image),
                    BytesIO(payloads[0]),
                    patch,
                    use_mmap=False,
                    compression=k.compression,
                    patch_type=k.patch_type,
                    algorithm=k.algorithm,
                    suffix_array_algorithm=k.suffix_array,
                    match_score=k.match_score,
                    match_block_size=k.match_block_size,
                    heatshrink_window_sz2=k.heatshrink_window,
                    heatshrink_lookahead_sz2=k.heatshrink_lookahead,
                    **bands,
                )
                blob = patch.getvalue()
                recovered = _delta_apply(k, blob)  # the round-trip proves the patch reconstructs byte-identically
                header = _header(k, blob)  # header-read to_size + codec/firmware agreement, never an inferred input length
                frame_size = header[0] if header is not None else len(recovered)
                verified = int(recovered == payloads[0] and frame_size == len(recovered) and (header[1] if header is not None else True))
                return blob, BundleEvidence.measure(algo, 0, 0, frame_size, verified, payloads, (blob,))
            case _:
                raise ValueError(f"<non-delta-profile:{profile.tag}>")  # the codec/container tags resolve on siblings; the family is not exhausted here

    @staticmethod
    def recover(blob: bytes, algo: CompressionAlgo, profile: CodecProfile, /) -> tuple[MemberTriple, ...]:
        match profile:
            case CodecProfile(tag="delta", delta=DeltaKnobs() as k):
                payload = _delta_apply(k, blob)
                # the uniform 16-byte xxh3_128 triple; the member name traces the parent through ContentKey.hex
                return ((f"from-{k.parent_key.hex}", len(payload), xxhash.xxh3_128_digest(payload)),)
            case _:
                raise ValueError(f"<non-delta-profile:{profile.tag}>")


# --- [OPERATIONS] -----------------------------------------------------------------------


def _delta_apply(k: DeltaKnobs, patch: bytes) -> bytes:
    match k.patch_type:  # the patch HEADER kind selects the reconstruction surface, never one apply_patch for all
        case "in-place":
            memory = k.in_place.memory_size if k.in_place is not None else len(k.from_image)
            mem = BytesIO(k.from_image.ljust(memory, b"\x00"))
            # bind to_size BEFORE getvalue(): the call mutates mem in place, so the read must see the post-patch buffer
            to_size = detools.apply_patch_in_place(mem, BytesIO(patch))
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


def _header(k: DeltaKnobs, blob: bytes) -> tuple[int, bool] | None:
    # peek the self-describing header: `patch_info` reads (patch_size, compression, ..., to_size) off the bytes the
    # create kernel wrote; the raw bsdiff patch has no header ("Bad patch type"), so the round-trip is its sole
    # evidence; firmware proof is `dfpatch_size > 0` on the sequential header.
    if k.patch_type == "bsdiff":
        return None
    kind, info = detools.patch_info(BytesIO(blob))
    firmware_proven = k.firmware is None or (kind == "sequential" and info[3] > 0)
    return info[_TO_SIZE_AT[kind]], info[1] == k.compression and firmware_proven
```
