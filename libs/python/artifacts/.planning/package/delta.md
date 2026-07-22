# [PY_ARTIFACTS_DELTA]

`Delta` is the binary diff/patch producer — incremental artifact deltas keyed by a parent content key. Where `package/codec#CODEC` compresses one payload against nothing and `package/archive#ARCHIVE` folds many into one container, `Delta` diffs ONE to-image payload against a parent from-image and stores only the compressed patch, so an artifact revising a near-identical predecessor ships the delta, not the whole blob. It composes the `package/bundle#BUNDLE` vocabulary downward and imports no sibling.

`emit() -> ArtifactWork` carries the producer contract, and the parent-keyed row is structural: the node's `parents` name the base bundle key (`DeltaKnobs.parent_key`, forced by `Bundle.of`'s delta arm), so the plan graph holds the base→patch dependency and a re-issued identical revision elides pre-run (`Admission(keyed=None)`, `receipt.slot == node.key`) — cheaply, because the spec key folds `from_image` down to its xxh3_128 digest — a changed base image flips the node identity — while the parent identity rides the merkle fold. `Delta.of` accepts the single `DeltaKnobs` policy value, the to-image payload, and the keyword-only `lane` the composition root projects; `Bundle.of` admits its full `detools.create_patch` axis against `DELTA_MATRIX`, band congruence, and in-place memory layout, so `pack` is total over admitted policies and runs no combination validation. Reconstruction keys on the patch header kind through `_delta_apply` — the same kernel re-applying the just-minted patch at pack time — and `_proved_size` applies the patch-kind, size, compression, in-place memory-band, and firmware data-format proofs in both pack and recovery. A failed round-trip, size, or header proof RAISES a `<delta-verify:*>` fault into the offload rail, so a known-invalid patch never reaches receipt projection: a parent-relative patch is the one codec whose output cannot be trusted without applying it, and `verified = 1` on the receipt is a witnessed fact, never a hedge. `detools` owns the superset diff engine — a hand-rolled bsdiff or suffix-array diff is the deleted form — and its create/apply bodies release the GIL, so the kernel crosses at `KernelTrait.RELEASING` on the thread arm.

## [01]-[INDEX]

- [02]-[DELTA]: the `Delta` producer over the `CompressionAlgo.DELTA` row — the parent-binding `of`, the `emit`/`packed`/`unpack` node contract, the `PackWorker` port kernels, and the `_delta_apply` header-keyed reconstruction shared by round-trip verify and recovery.

## [02]-[DELTA]

- Owner: `Delta` the one diff/patch producer wrapping the `Bundle` carrier with a `delta`-case profile and its `lane: LanePolicy`; `DeltaKnobs`/`InPlaceSegments`/`FirmwareLayout`, the delta `Literal` axes, and the `DELTA_MATRIX` admission gate are bundle-page vocabulary, so a delta bundle is one profile row on the one union, never a parallel owner. `Delta.pack`/`Delta.recover` are the `PackWorker` port kernels over `(payloads, profile)`, total over the one row.
- Cases: the admitted `(algorithm, patch_type)` pair selects the `detools` create kernel, each combination reading only the axes it exercises — an out-of-matrix pair, a bandless or impossible-memory `in-place`, and a firmware band off `bsdiff`×`sequential` all refused by `Bundle.of`'s `_delta_admitted` gate before any arm runs, never surfaced by provider execution; `_delta_apply` is the ONE header-keyed reconstruction kernel both legs share — never one `apply_patch` for the self-describing, in-place, and headerless-`BSDIFF40` kinds — reading any firmware dfpatch back so recovery re-supplies no offsets. A `DeltaCompression` token frames the patch payload through admitted `lz4`/`zstandard`/`heatshrink2` plus stdlib, resolved from the header so `_delta_apply` re-supplies neither codec nor algorithm.
- Entry: `Delta.of(policy, payload, lane=lane)` binds the parent at construction — the policy carries `from_image` (the only side-input either leg needs) and `parent_key`, and the node reads `parents=(parent_key,)`, so a content-addressed delta keyed by its parent is the storage AND the graph shape; `packed` offloads `Delta.pack` for the byte-holding consumer, `_emit` maps it onto the receipt, and `unpack` names the recovered member `f"from-{parent_key.hex}"` so it traces its parent.
- Output: `_proved_size` reads the to-image size off the patch header via `detools.patch_info` (slot per kind through `_TO_SIZE_AT`, the headerless `bsdiff` patch falling back to the reconstructed length), requires the header kind, compression, effective in-place memory/segment/slide band (the header echoes the computed slide `max(memory - segment * ceil(from/segment), minimum)`, never the `minimum_shift_size` knob verbatim), firmware data-format token, and reconstructed length to agree in both pack and recovery, and leaves `level`/`dict_id` zero since compression is header-encoded. Pack adds the first ordered gate — byte-identical round-trip — before `_proved_size` proves header identity, selects its size slot, and proves reconstructed length; each component fails as `<delta-verify:round-trip|header|size>` before `BundleEvidence`/`ArtifactReceipt` materializes.
- Packages: `detools` (lazy — `create_patch`/`apply_patch*`/`patch_info`; runtime deps `bitstruct`/`heatshrink2`/`lz4`/`pyelftools`/`zstandard` ride it), `xxhash` (the recovered-image digest), `expression` (`Map.of_seq` the header-slot row), `msgspec` (`Struct`), runtime `identity`/`faults`/`lanes`/`resilience`, `rasm.artifacts.core.plan`/`core.receipt`/`package.bundle`.
- Growth: a new patch type is one bundle-page `DeltaPatchType` token, its `DELTA_MATRIX` pairings, plus one `_delta_apply` arm; a new diff algorithm/codec/architecture/suffix-array constructor is one token on its bundle-page axis; a new tuning knob is one named `DeltaKnobs` field with `Delta.of(policy, payload, lane=lane)` unchanged — the in-place and firmware bands are the anticipatory collapse already absorbed, zero new verb beside `emit`/`packed`/`unpack`.
- Boundary: no sibling import, no vocabulary re-own, no folder-minted limiter or retry caller, no CLI argparse plumbing (`data_format_args` is never a library resolver), no corpus modality (a corpus diff is N parent-bound nodes, never one), no receipt-case widening, no zero-`verified` receipt standing in for a failed proof (failure rides the rail, never a receipt field).

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from io import BytesIO
from typing import Final

import xxhash
from expression.collections import Map
from msgspec import Struct

from rasm.runtime.faults import RuntimeRail
from rasm.runtime.lanes import LanePolicy
from rasm.runtime.workers import Kernel, KernelTrait

from rasm.artifacts.core.plan import Admission, ArtifactWork
from rasm.artifacts.core.receipt import ArtifactReceipt
from rasm.artifacts.package.bundle import (
    Bundle,
    BundleEvidence,
    BundleManifest,
    CodecProfile,
    DeltaKnobs,
    InPlaceSegments,
    MemberTriple,
)

lazy import detools

# --- [TABLES] ---------------------------------------------------------------------------

_BSDIFF_MAGIC: Final[bytes] = b"BSDIFF40"  # the classic magic a raw detools bsdiff patch opens on; no detools header follows
_TO_SIZE_AT: Final[Map[str, int]] = Map.of_seq([("sequential", 6), ("in-place", 7), ("hdiffpatch", 3)])

# --- [MODELS] ---------------------------------------------------------------------------


class Delta(Struct, frozen=True):
    # `lane` arrives projected via LanePolicy.of(context) at the composition root — a capacity literal has no owner.
    bundle: Bundle
    lane: LanePolicy

    @staticmethod
    def of(policy: DeltaKnobs, payload: bytes, /, *, lane: LanePolicy) -> "Delta":
        return Delta(bundle=Bundle.of(CodecProfile(delta=policy), payload), lane=lane)

    def emit(self, /) -> ArtifactWork:
        return ArtifactWork(key=self.bundle.key, work=self._emit, parents=self.bundle.parents, admission=Admission(keyed=None), cost=self._cost)

    async def packed(self, /) -> RuntimeRail[tuple[bytes, BundleEvidence]]:
        return await self.lane.offload(Kernel.of(Delta.pack, KernelTrait.RELEASING), self.bundle.payloads, self.bundle.profile)

    async def _emit(self, /) -> RuntimeRail[ArtifactReceipt]:
        return (await self.packed()).map(lambda pe: pe[1].receipt(self.bundle.key))

    async def unpack(self, blob: bytes, /) -> RuntimeRail[BundleManifest]:
        rows = await self.lane.offload(Kernel.of(Delta.recover, KernelTrait.RELEASING), blob, self.bundle.profile)
        return rows.map(lambda recovered: BundleManifest.of(self.bundle.algo, recovered))

    @property
    def _cost(self) -> float:
        return float(sum(map(len, self.bundle.payloads)) or 1)

    @staticmethod
    def pack(payloads: tuple[bytes, ...], profile: CodecProfile, /) -> tuple[bytes, BundleEvidence]:
        match profile:
            case CodecProfile(tag="delta", delta=DeltaKnobs() as k):
                patch = BytesIO()
                bands = {**(k.in_place.kwargs() if k.in_place is not None else {}), **(k.firmware.kwargs() if k.firmware is not None else {})}
                detools.create_patch(
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
                recovered = _delta_apply(k, blob)
                if recovered != payloads[0]:
                    raise ValueError("<delta-verify:round-trip>")
                frame_size = _proved_size(k, blob, recovered)
                return blob, BundleEvidence.measure(profile.algo, 0, 0, frame_size, 1, payloads, blob)
            case _:
                raise ValueError(f"<non-delta-profile:{profile.tag}>")

    @staticmethod
    def recover(blob: bytes, profile: CodecProfile, /) -> tuple[MemberTriple, ...]:
        match profile:
            case CodecProfile(tag="delta", delta=DeltaKnobs() as k):
                payload = _delta_apply(k, blob)
                _proved_size(k, blob, payload)
                return ((f"from-{k.parent_key.hex}", len(payload), xxhash.xxh3_128_digest(payload)),)
            case _:
                raise ValueError(f"<non-delta-profile:{profile.tag}>")


# --- [OPERATIONS] -----------------------------------------------------------------------


def _reconstructed_kind(patch: bytes, /) -> str:
    # self-describing header names its own kind; a raw bsdiff patch carries the BSDIFF40 magic instead, so the
    # magic — never a bare `patch_info` refusal — is the bsdiff discriminant, and a blob that satisfies neither
    # refuses as `<delta-verify:kind>` before any kernel dispatches on it.
    if patch[: len(_BSDIFF_MAGIC)] == _BSDIFF_MAGIC:
        return "bsdiff"
    try:
        kind, _info = detools.patch_info(BytesIO(patch))
    except detools.Error as refused:
        raise ValueError("<delta-verify:kind>") from refused
    return kind


def _delta_apply(k: DeltaKnobs, patch: bytes, /) -> bytes:
    # dispatch is proven against the RECONSTRUCTED patch kind before any kernel touches the from-image, so a blob
    # whose header disagrees with the knobs — a bsdiff blob smuggled under a sequential profile included — refuses
    # here rather than reconstructing garbage; each arm keeps its kernel-specific output behavior.
    if (kind := _reconstructed_kind(patch)) != k.patch_type:
        raise ValueError(f"<delta-verify:kind:{kind}>")
    match k:
        case DeltaKnobs(patch_type="in-place", in_place=InPlaceSegments(memory_size=memory)):
            mem = BytesIO(k.from_image.ljust(memory, b"\x00"))
            to_size = detools.apply_patch_in_place(mem, BytesIO(patch))
            return mem.getvalue()[:to_size]
        case DeltaKnobs(patch_type="bsdiff"):
            sink = BytesIO()
            detools.apply_patch_bsdiff(BytesIO(k.from_image), BytesIO(patch), sink)
            return sink.getvalue()
        case DeltaKnobs(patch_type="sequential" | "hdiffpatch"):
            sink = BytesIO()
            detools.apply_patch(BytesIO(k.from_image), BytesIO(patch), sink)
            return sink.getvalue()
        case DeltaKnobs():
            # reachable only for a bandless in-place knob a hand-built profile smuggled past `_delta_admitted`
            raise ValueError("<delta-band:in-place>")


def _proved_size(k: DeltaKnobs, blob: bytes, recovered: bytes, /) -> int:
    if k.patch_type == "bsdiff":
        # `_delta_apply` already proved the BSDIFF40 magic (the bsdiff discriminant); a raw patch carries no
        # `to_size` field, so the recovered length is the only size evidence.
        return len(recovered)
    kind, info = detools.patch_info(BytesIO(blob))
    if kind != k.patch_type or kind not in _TO_SIZE_AT or info[1] != k.compression:
        raise ValueError("<delta-verify:header>")
    match kind, k.in_place, k.firmware:
        case "in-place", InPlaceSegments(memory_size=memory, segment_size=segment, minimum_shift_size=shift), None:
            # header echoes the COMPUTED slide — all whole segments the from-image leaves free, floored at the
            # minimum (2 * segment default) — never the knob verbatim.
            slide = max(memory - segment * -(-len(k.from_image) // segment), shift if shift is not None else 2 * segment)
            if info[3:6] != (memory, segment, slide):
                raise ValueError("<delta-verify:header>")
        case "sequential", None, firmware:
            if info[4] != (firmware.data_format if firmware is not None else None):
                raise ValueError("<delta-verify:header>")
        case "hdiffpatch", None, None:
            pass
        case _:
            raise ValueError("<delta-verify:header>")
    frame_size = info[_TO_SIZE_AT[kind]]
    if frame_size != len(recovered):
        raise ValueError("<delta-verify:size>")
    return frame_size
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
