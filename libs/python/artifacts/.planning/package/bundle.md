# [PY_ARTIFACTS_BUNDLE]

`Bundle` is the package-plane vocabulary and port floor the three producer arms compose downward — the shared spec carrier, policy union, delta admission matrix, manifest, evidence, and worker port every `package/codec#CODEC`, `package/archive#ARCHIVE`, and `package/delta#DELTA` path imports while none imports another. It mints no node and runs no arm; it holds the shapes so the arms hold only behavior.

One discriminant rules the plane: `CodecProfile.tag` is the algorithm, `ALGO_OF` derives the `CompressionAlgo` label from it, and `Bundle` stores no second algorithm field — a bundle encoding one algorithm and executing another is unrepresentable, so dispatch, key material, evidence, and manifest labels all read one recoverable value. `Bundle.of` is the one polymorphic admission: a `CompressionAlgo` token resolves `DEFAULT_PROFILE`, a `CodecProfile` carries its own algorithm, and the delta case accepts exactly one payload, derives its sole parent from `DeltaKnobs.parent_key`, and crosses the `_delta_admitted` congruence gate — the `DELTA_MATRIX` `(algorithm, patch_type)` roster plus the in-place and firmware band rules — so every profile a kernel ever dispatches is already in-matrix and `pack` runs no combination validation.

`Bundle` owns the KEY-OVER-INPUT mint — `ContentIdentity.key` over canonical `msgpack` spec bytes (`profile.tag`, the `keyable` knob projection, per-payload `xxh3_128` digests), then one more `ContentIdentity.key` over `(spec_key, *parents)` when parents exist, the runtime's own order-sensitive merkle fold rather than a framed-by-luck hex concat — computable before any arm runs so `keyed` admission probes the cache seed and elision fires pre-production. Delta spec folds `from_image` down to its xxh3_128 digest — a changed base image flips the admitted node identity even under one `parent_key` — and nulls `parent_key`, whose identity rides the merkle fold content-addressed (a live `ContentKey.value` is a u128 msgpack cannot integer-encode), so every algorithm's pre-run key stays cheap. `BundleManifest.of` folds each recovered member through one `member-{algo}` `ContentIdentity.key` so addressing is identical across arms, and `BundleEvidence.receipt` is the ONE projection onto the flat `ArtifactReceipt.Bundle` case (`core/receipt#RECEIPT` the sole downward runtime edge, `receipt.slot == node.key` because the producer threads the same pre-run key). A `PackWorker` port carries both faces — the module-picklable `(pack, recover)` kernels the `THREAD`/`PROCESS` offload names by qualified path, and the `emit`/`packed`/`unpack` producer surface a composite consumer (`delivery/transmittal#TRANSMITTAL`) holds as one shape.

## [01]-[INDEX]

- [02]-[BUNDLE]: the `Bundle` carrier and pre-run key mint, the `CodecProfile` policy union with its derived `CompressionAlgo` label, the closed `Literal` vocabularies, the `InPlaceSegments`/`FirmwareLayout` delta bands and the `DELTA_MATRIX` admission gate, the manifest and evidence folds, and the `PackWorker` port.

## [02]-[BUNDLE]

- Owner: `Bundle` the frozen spec carrier (`payloads`, `profile`, `parents`) all three producers wrap, `algo` deriving from `profile.tag` through the one `ALGO_OF` correspondence; `CodecProfile` the policy union carrying one typed knob-struct per row — each a picklable `msgspec.Struct` (all-scalar, `frozen=True`) so a profile crosses the `PROCESS` lane as data, never a native handle. `Bundle.key` mints KEY-OVER-INPUT pre-run, so a re-issued identical member set at an identical profile is a `keyed`-admission hit before compression, and the packed blob's output address stays store-side lane evidence, never the elision key.
- Cases: the closed `Literal` axes carry package-constant families as neutral tokens; the VALUE dispatch rows binding tokens to provider constants (`STRATEGY_*`, `FILTER_*`, `MODE_*`, decode sentinels) live at arm scope on the owning sibling. `DEFAULT_PROFILE` is the per-algorithm policy table; `DELTA` is absent by design (a patch binds its parent image, so the delta profile is always explicit) and the algo-selector arm of `Bundle.of` resolves absence through `try_find` with a loud construction refusal, never a silent fallback. `Bundle.of` admits Zstandard log/length/thread/dictionary and LZ4 level values against their provider constants, Brotli quality/window/block, gzip level/mtime/thread/block, and ZIP level inside their provider ranges; no invalid native allocation parameter crosses the boundary. Stored ZIP fixes level at `0`; every 7z chain carries a unique terminal codec after only DELTA/BCJ preprocessors, and the `extreme` preset reaches only a terminal LZMA/LZMA2 codec, so no provider-shaped chain or ignored policy value reaches a worker. Archive profiles admit `names` only when empty or arity-equal, unique, and relative POSIX-safe, while 7z header encryption requires `password`; no ignored name, duplicate sink key, traversal member, or contradictory crypto policy enters `Archive.pack`. `_delta_admitted` is the delta congruence gate every construction path crosses: `DELTA_MATRIX` holds exactly the `(algorithm, patch_type)` kernels `detools` ships, the `in_place` band rides iff `patch_type == "in-place"` with a positive segment size, segment-aligned memory and minimum shift, and enough memory for both images, and the `firmware` band rides only `bsdiff`×`sequential` — an out-of-matrix pair or impossible memory layout refuses at construction, never inside a worker. `Bundle.of` also refuses delta arity other than one and a separate `parents` value because the profile owns the only parent edge; no ignored payload or parent knob reaches `Delta.pack`. `InPlaceSegments` and `FirmwareLayout` are the delta knob bands; `FirmwareLayout.of_elf` accepts `Path` symbols and calls `detools.data_format_from_files` once per image (the bundled `pyelftools` reader), so firmware offsets are provider-derived and the raw ctor stays the pre-resolved shape. ZIP encryption is one discriminant — password presence — because `stream-zip` writes exactly WinZip AE-2/AES-256 when a password is set: the pack behavior and the decode allow-list both derive from the same value, so a decode-only trust roster beside the pack knob cannot drift.
- Output: `BundleManifest.of` mints each `ManifestRow` key through the one `member-{algo}` fold so member addressing is identical across every arm; `BundleEvidence.measure` folds member count and `in_bytes`/`out_bytes` once (`entries` is member arity, never the joined-blob count) and `BundleEvidence.receipt(key)` is the ONE constructor onto `ArtifactReceipt.Bundle` — three producers, one receipt shape, zero per-page spread.
- Packages: `msgspec` (`Struct` knobs/manifest/evidence, `msgpack.Encoder` the canonical spec bytes — msgpack encodes a `Struct` natively and never consults an `enc_hook` for one, so `keyable` nulls the u128-bearing `ContentKey` leaf instead of trusting a hook, `structs.replace` the `keyable` blank), `expression` (`tagged_union`/`case`/`tag` the profile union, `Map.of_seq`/`try_find` the tables), `xxhash` (`xxh3_128_digest` the pre-run digests), runtime `identity` (`ContentIdentity.key` the sync mint, `ContentKey`), `zstandard`/`lz4.frame` (lazy parameter-bound constants at admission), `detools` (`data_format_from_files`, the `of_elf` resolver alone, lazy).
- Growth: a new algorithm is one `CompressionAlgo` row, one knob-struct, one `CodecProfile` case, one `ALGO_OF` row, one `DEFAULT_PROFILE` row, and one arm on the owning sibling; a new bounded knob value is one token on the owning `Literal` axis plus one arm-scope dispatch row; a new detools kernel pairing is one `DELTA_MATRIX` member; a new manifest column is one `ManifestRow` field; a new evidence fact is one `BundleEvidence` field threading `measure` and `receipt` — zero new verb, zero page-level surface.
- Boundary: no behavior arm, no provider dispatch values, no offload, no lane, no receipt MINT (the projection constructs the case; producers thread the key and return it on their rails), no artifacts-internal import above the spine — `core/receipt` the one downward runtime edge, `core/plan`/`faults` type-only under `TYPE_CHECKING`, codec/archive/delta composing this page but never each other. Runtime-only parallelism knobs (`threads`, `block_size`) stay in the profile deliberately: they perturb the output bytes on the zstd and threaded-gzip arms, so keying over them is the conservative miss, never a wrong hit. Member stamps stay container-level (`_EPOCH`, one `mode`) because byte-reproducibility rules the ZIP arm; `names` is the one per-member axis, and a per-member wall-clock stamp is the rejected key-churning form.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from datetime import datetime, timezone
from enum import StrEnum
from pathlib import Path, PurePosixPath
from typing import TYPE_CHECKING, Final, Literal, Protocol, assert_never

import xxhash
from expression import case, tag, tagged_union
from expression.collections import Map
from msgspec import Struct, msgpack, structs

from rasm.runtime.identity import ContentIdentity, ContentKey

from rasm.artifacts.core.receipt import ArtifactReceipt

if TYPE_CHECKING:
    from rasm.artifacts.core.plan import ArtifactWork
    from rasm.runtime.faults import RuntimeRail

lazy import detools  # reifies only inside FirmwareLayout.of_elf
lazy import lz4.frame
lazy import zstandard

# --- [TYPES] ----------------------------------------------------------------------------

type Lz4Block = Literal["default", "max64kb", "max256kb", "max1mb", "max4mb"]
type BrotliMode = Literal["generic", "text", "font"]
type ZstdDictMode = Literal["auto", "fulldict", "rawcontent"]
type ZstdStrategy = Literal["auto", "fast", "dfast", "greedy", "lazy", "lazy2", "btlazy2", "btopt", "btultra", "btultra2"]
type SevenZFilter = Literal[
    "lzma", "lzma2", "bzip2", "ppmd", "zstd", "brotli", "deflate", "copy", "delta", "x86", "arm", "armthumb", "powerpc", "sparc", "ia64"
]
type SevenZPreset = Literal["default", "extreme"]
type ZipMethod = Literal["auto", "zip64", "zip32", "store32", "store64"]
type DeltaPatchType = Literal["sequential", "in-place", "bsdiff", "hdiffpatch"]
type DeltaAlgorithm = Literal["bsdiff", "hdiffpatch", "match-blocks"]
type DeltaCompression = Literal["bz2", "crle", "lzma", "zstd", "lz4", "heatshrink", "none"]
type DeltaSuffixArray = Literal["divsufsort", "sais"]
type DataFormat = Literal["arm-cortex-m4", "aarch64", "xtensa-lx106"]
type MemberTriple = tuple[str, int, bytes]  # (name, size, xxh3_128 digest) — the uniform recovery row

# --- [CONSTANTS] ------------------------------------------------------------------------

_EPOCH: Final[datetime] = datetime(1980, 1, 1, tzinfo=timezone.utc)

# --- [MODELS] ---------------------------------------------------------------------------


class CompressionAlgo(StrEnum):
    ZSTD = "zstd"
    LZ4 = "lz4"
    BROTLI = "brotli"
    GZIP = "gzip"
    SEVEN_Z = "7z"
    ZIP_STREAM = "zip-stream"
    DELTA = "delta"


class ZstdKnobs(Struct, frozen=True, gc=False):
    level: int = 19
    threads: int = -1
    window_log: int = 0
    enable_ldm: bool = False
    write_checksum: bool = True
    dict_data: bytes | None = None
    dict_mode: ZstdDictMode = "auto"
    strategy: ZstdStrategy = "auto"  # "auto" lets from_level derive the matcher; STRATEGY_* has no zero member
    hash_log: int = 0  # 0 is the from_level auto-sentinel: unset -> level-derived
    chain_log: int = 0
    target_length: int = 0


class Lz4Knobs(Struct, frozen=True, gc=False):
    compression_level: int = 0
    block_size: Lz4Block = "default"
    content_checksum: bool = True
    block_checksum: bool = False
    block_linked: bool = True


class BrotliKnobs(Struct, frozen=True, gc=False):
    mode: BrotliMode = "generic"
    quality: int = 11
    lgwin: int = 22
    lgblock: int = 0


class GzipKnobs(Struct, frozen=True, gc=False):
    level: int = 9
    mtime: int = 0
    threads: int = -1  # gzip_ng_threaded block-fan workers; -1 = all logical cores
    block_size: int = 1 << 20  # per-block queue the threaded writer fans across workers


class SevenZKnobs(Struct, frozen=True):
    filters: tuple[SevenZFilter, ...] = ("lzma2",)
    preset: SevenZPreset = "default"
    header_encryption: bool = False
    password: str | None = None
    names: tuple[str, ...] = ()


class ZipStreamKnobs(Struct, frozen=True):
    method: ZipMethod = "auto"
    level: int = 9
    password: str | None = None
    names: tuple[str, ...] = ()
    modified_at: datetime = _EPOCH
    mode: int = 0o600


class InPlaceSegments(Struct, frozen=True):
    memory_size: int
    segment_size: int
    # `minimum_shift_size` defaults to 2 * segment_size and must divide by segment_size, like memory_size; the patch header
    # echoes the COMPUTED slide max(memory - segment * ceil(from/segment), minimum), never this knob verbatim.
    minimum_shift_size: int | None = None

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

    @classmethod
    def of_elf(
        cls,
        data_format: DataFormat,
        from_elf: Path,
        from_bin: Path,
        to_elf: Path,
        to_bin: Path,
        *,
        from_offset: int | None = None,
        to_offset: int | None = None,
    ) -> "FirmwareLayout":
        fdo_b, fdo_e, fc_b, fc_e, fd_b, fd_e = detools.data_format_from_files(data_format, from_elf, from_bin, from_offset)
        tdo_b, tdo_e, tc_b, tc_e, td_b, td_e = detools.data_format_from_files(data_format, to_elf, to_bin, to_offset)
        return cls(
            data_format=data_format,
            from_data_offset=(fdo_b, fdo_e),
            from_code=(fc_b, fc_e),
            from_data=(fd_b, fd_e),
            to_data_offset=(tdo_b, tdo_e),
            to_code=(tc_b, tc_e),
            to_data=(td_b, td_e),
        )

    def kwargs(self) -> dict[str, object]:
        return {
            "data_format": self.data_format,
            "from_data_offset_begin": self.from_data_offset[0],
            "from_data_offset_end": self.from_data_offset[1],
            "from_data_begin": self.from_data[0],
            "from_data_end": self.from_data[1],
            "from_code_begin": self.from_code[0],
            "from_code_end": self.from_code[1],
            "to_data_offset_begin": self.to_data_offset[0],
            "to_data_offset_end": self.to_data_offset[1],
            "to_data_begin": self.to_data[0],
            "to_data_end": self.to_data[1],
            "to_code_begin": self.to_code[0],
            "to_code_end": self.to_code[1],
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


@tagged_union(frozen=True)
class CodecProfile:
    tag: Literal["zstd", "lz4", "brotli", "gzip", "seven_z", "zip_stream", "delta"] = tag()
    zstd: ZstdKnobs = case()
    lz4: Lz4Knobs = case()
    brotli: BrotliKnobs = case()
    gzip: GzipKnobs = case()
    seven_z: SevenZKnobs = case()
    zip_stream: ZipStreamKnobs = case()
    delta: DeltaKnobs = case()

    @property
    def algo(self) -> CompressionAlgo:
        return ALGO_OF[self.tag]

    @property
    def keyable(self) -> Struct:
        # spec-key material: the active knob struct; the delta arm folds `from_image` DOWN to its xxh3_128 digest
        # (so a changed base image flips the bundle key even under one parent_key — KEY-OVER-INPUT holds over the
        # patch-generation input) and nulls `parent_key`, whose identity rides the parents merkle fold (patterns
        # row [06]: derived); msgpack encodes a Struct natively so a live u128 `ContentKey.value` overflows u64.
        match self:
            case CodecProfile(tag="delta", delta=k):
                return structs.replace(k, from_image=xxhash.xxh3_128_digest(k.from_image), parent_key=_SPEC_PARENT)
            case (
                CodecProfile(tag="zstd", zstd=k)
                | CodecProfile(tag="lz4", lz4=k)
                | CodecProfile(tag="brotli", brotli=k)
                | CodecProfile(tag="gzip", gzip=k)
                | CodecProfile(tag="seven_z", seven_z=k)
                | CodecProfile(tag="zip_stream", zip_stream=k)
            ):
                return k
            case _ as unreachable:
                assert_never(unreachable)


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
    def of(algo: CompressionAlgo, recovered: tuple[MemberTriple, ...]) -> "BundleManifest":
        return BundleManifest(
            algo, tuple(ManifestRow(name, ContentIdentity.key(f"member-{algo}", digest), algo, size) for name, size, digest in recovered)
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
    def measure(
        algo: CompressionAlgo, level: int, dict_id: int, frame_size: int, verified: int, payloads: tuple[bytes, ...], blob: bytes
    ) -> "BundleEvidence":
        return BundleEvidence(algo, level, dict_id, frame_size, len(payloads), verified, sum(map(len, payloads)), len(blob))

    def receipt(self, key: ContentKey, /) -> ArtifactReceipt:
        return ArtifactReceipt.Bundle(key, self.algo.value, self.level, self.dict_id, self.frame_size, self.entries, self.verified, self.ratio)


class Bundle(Struct, frozen=True):
    payloads: tuple[bytes, ...]
    profile: CodecProfile
    parents: tuple[ContentKey, ...] = ()

    @property
    def algo(self) -> CompressionAlgo:
        return self.profile.algo

    @property
    def key(self) -> ContentKey:
        # KEY-OVER-INPUT: one msgpack spec (tag, keyable knobs, per-member xxh3_128 digests) minted alone, then the
        # runtime's order-sensitive parent merkle fold over (spec_key, *parents) — never a hex concat.
        spec = _SPEC.encode((self.profile.tag, self.profile.keyable, tuple(xxhash.xxh3_128_digest(p) for p in self.payloads)))
        minted = ContentIdentity.key(f"bundle-{self.algo}", spec)
        return minted if not self.parents else ContentIdentity.key(f"bundle-{self.algo}", (minted, *self.parents))

    @staticmethod
    def of(selector: CompressionAlgo | CodecProfile, *payloads: bytes, parents: tuple[ContentKey, ...] = ()) -> "Bundle":
        match selector:
            case CodecProfile(
                tag="zstd",
                zstd=ZstdKnobs(
                    level=level,
                    threads=threads,
                    window_log=window_log,
                    hash_log=hash_log,
                    chain_log=chain_log,
                    target_length=target_length,
                    dict_data=dict_data,
                    dict_mode=dict_mode,
                ),
            ) if (
                not zstandard.MIN_COMPRESSION_LEVEL <= level <= zstandard.MAX_COMPRESSION_LEVEL
                or threads < -1
                or not (window_log == 0 or zstandard.WINDOWLOG_MIN <= window_log <= zstandard.WINDOWLOG_MAX)
                or not (hash_log == 0 or zstandard.HASHLOG_MIN <= hash_log <= zstandard.HASHLOG_MAX)
                or not (chain_log == 0 or zstandard.CHAINLOG_MIN <= chain_log <= zstandard.CHAINLOG_MAX)
                or not zstandard.TARGETLENGTH_MIN <= target_length <= zstandard.TARGETLENGTH_MAX
                or dict_data == b""
                or (dict_data is None and dict_mode != "auto")
            ):
                raise ValueError("<codec-policy:zstd-range>")
            case CodecProfile(tag="lz4", lz4=Lz4Knobs(compression_level=level)) if not (
                lz4.frame.COMPRESSIONLEVEL_MIN <= level <= lz4.frame.COMPRESSIONLEVEL_MAX
            ):
                raise ValueError("<codec-policy:lz4-level>")
            case CodecProfile(tag="brotli", brotli=BrotliKnobs(quality=quality, lgwin=lgwin, lgblock=lgblock)) if not (
                0 <= quality <= 11 and 10 <= lgwin <= 24 and (lgblock == 0 or 16 <= lgblock <= 24)
            ):
                raise ValueError("<codec-policy:brotli-range>")
            case CodecProfile(tag="gzip", gzip=GzipKnobs(level=level, mtime=mtime, threads=threads, block_size=block_size)) if not (
                0 <= level <= 9 and 0 <= mtime < 1 << 32 and (threads == -1 or threads > 0) and block_size > 0
            ):
                raise ValueError("<codec-policy:gzip-range>")
            case CodecProfile(tag="zip_stream", zip_stream=ZipStreamKnobs(level=level)) if not 0 <= level <= 9:
                raise ValueError("<archive-policy:zip-level>")
            case CodecProfile(tag="zip_stream", zip_stream=ZipStreamKnobs(method="store32" | "store64", level=level)) if level != 0:
                raise ValueError("<archive-policy:stored-level>")
            case CodecProfile(tag="seven_z", seven_z=SevenZKnobs(filters=filters)) if (
                not filters
                or filters[-1] not in _SEVEN_Z_CODECS
                or any(item not in _SEVEN_Z_PREPROCESSORS for item in filters[:-1])
                or len(frozenset(filters)) != len(filters)
            ):
                raise ValueError("<archive-policy:filter-chain>")
            case CodecProfile(tag="seven_z", seven_z=SevenZKnobs(filters=filters, preset="extreme")) if filters[-1] not in {
                "lzma",
                "lzma2",
            }:
                raise ValueError("<archive-policy:unused-preset>")
            case CodecProfile(tag="seven_z", seven_z=SevenZKnobs(header_encryption=True, password=None)):
                raise ValueError("<archive-encryption:header-requires-password>")
            case (
                CodecProfile(tag="seven_z", seven_z=SevenZKnobs(names=names))
                | CodecProfile(tag="zip_stream", zip_stream=ZipStreamKnobs(names=names))
            ) if len(names) not in (0, len(payloads)):
                raise ValueError(f"<archive-names:arity:{len(names)}x{len(payloads)}>")
            case (
                CodecProfile(tag="seven_z", seven_z=SevenZKnobs(names=names))
                | CodecProfile(tag="zip_stream", zip_stream=ZipStreamKnobs(names=names))
            ) if len(frozenset(names)) != len(names):
                raise ValueError("<archive-names:duplicate>")
            case (
                CodecProfile(tag="seven_z", seven_z=SevenZKnobs(names=names))
                | CodecProfile(tag="zip_stream", zip_stream=ZipStreamKnobs(names=names))
            ) if any(unsafe_member(name) for name in names):
                raise ValueError("<archive-names:unsafe>")
            case CodecProfile(tag="delta") if parents:
                raise ValueError("<delta-parents:profile-owned>")
            case CodecProfile(tag="delta") if len(payloads) != 1:
                raise ValueError(f"<delta-arity:{len(payloads)}>")
            case CodecProfile(tag="delta", delta=knobs) as profile:
                return Bundle(payloads=payloads, profile=profile, parents=(_delta_admitted(knobs, len(payloads[0])).parent_key,))
            case CodecProfile() as profile:
                return Bundle(payloads=payloads, profile=profile, parents=parents)
            case algo:
                resolved = DEFAULT_PROFILE.try_find(algo).default_with(lambda: _profile_required(algo))
                return Bundle.of(resolved, *payloads, parents=parents)


# --- [TABLES] ---------------------------------------------------------------------------

ALGO_OF: Final[Map[str, CompressionAlgo]] = Map.of_seq([
    ("zstd", CompressionAlgo.ZSTD),
    ("lz4", CompressionAlgo.LZ4),
    ("brotli", CompressionAlgo.BROTLI),
    ("gzip", CompressionAlgo.GZIP),
    ("seven_z", CompressionAlgo.SEVEN_Z),
    ("zip_stream", CompressionAlgo.ZIP_STREAM),
    ("delta", CompressionAlgo.DELTA),
])

DEFAULT_PROFILE: Final[Map[CompressionAlgo, CodecProfile]] = Map.of_seq([
    (CompressionAlgo.ZSTD, CodecProfile(zstd=ZstdKnobs())),
    (CompressionAlgo.LZ4, CodecProfile(lz4=Lz4Knobs())),
    (CompressionAlgo.BROTLI, CodecProfile(brotli=BrotliKnobs())),
    (CompressionAlgo.GZIP, CodecProfile(gzip=GzipKnobs())),
    (CompressionAlgo.SEVEN_Z, CodecProfile(seven_z=SevenZKnobs())),
    (CompressionAlgo.ZIP_STREAM, CodecProfile(zip_stream=ZipStreamKnobs())),
])

DELTA_MATRIX: Final[frozenset[tuple[DeltaAlgorithm, DeltaPatchType]]] = frozenset({
    ("bsdiff", "sequential"),
    ("bsdiff", "in-place"),
    ("bsdiff", "bsdiff"),
    ("hdiffpatch", "hdiffpatch"),
    ("match-blocks", "sequential"),
    ("match-blocks", "hdiffpatch"),
})

_SEVEN_Z_CODECS: Final[frozenset[SevenZFilter]] = frozenset({"lzma", "lzma2", "bzip2", "ppmd", "zstd", "brotli", "deflate", "copy"})
_SEVEN_Z_PREPROCESSORS: Final[frozenset[SevenZFilter]] = frozenset({"delta", "x86", "arm", "armthumb", "powerpc", "sparc", "ia64"})


# every knob leaf is msgpack-native once `keyable` nulls the delta parent key; the null key is the one spec
# sentinel a live u128 `ContentKey.value` cannot ride through msgpack's u64 integer ceiling.
_SPEC: Final[msgpack.Encoder] = msgpack.Encoder()
_SPEC_PARENT: Final[ContentKey] = ContentKey(value=0, fmt="spec-parent", byte_length=0)


def _delta_admitted(knobs: DeltaKnobs, target_size: int, /) -> DeltaKnobs:
    if (knobs.algorithm, knobs.patch_type) not in DELTA_MATRIX:
        raise ValueError(f"<delta-matrix:{knobs.algorithm}x{knobs.patch_type}>")
    if (knobs.in_place is not None) != (knobs.patch_type == "in-place"):
        raise ValueError("<delta-band:in-place>")
    # in-place layout floor: the from- and to-image SHARE one region (the sum-of-images bound is sequential
    # patching's, and demanding it here would defeat the mode), but the region must hold the to-image AND the
    # shift-displaced from-image — detools shifts from-data forward by at least the effective minimum shift
    # (2 * segment_size when the knob is omitted, detools' own default) before the to-segments write, so
    # `from + effective-shift` is the true occupancy the bare max(from, to) under-admits; one shift binding
    # serves the occupancy floor and the positivity/alignment checks for default and explicit values alike.
    if knobs.in_place is not None and (
        knobs.in_place.segment_size <= 0
        or (
            shift := 2 * knobs.in_place.segment_size if knobs.in_place.minimum_shift_size is None else knobs.in_place.minimum_shift_size
        )
        <= 0
        or shift % knobs.in_place.segment_size != 0
        or knobs.in_place.memory_size < max(target_size, len(knobs.from_image) + shift)
        or knobs.in_place.memory_size % knobs.in_place.segment_size != 0
    ):
        raise ValueError("<delta-band:in-place-layout>")
    if knobs.firmware is not None and (knobs.algorithm, knobs.patch_type) != ("bsdiff", "sequential"):
        raise ValueError("<delta-band:firmware>")
    return knobs


def unsafe_member(name: str, /) -> bool:
    # ONE relative-POSIX member-name law, pack and unpack alike: empty, backslash, NUL, absolute, and `..`
    # traversal forms refuse — `Bundle.of` guards authored names and the archive drains guard decoded hostile
    # names through this same predicate, so the two seams can never drift.
    return not name or "\\" in name or "\0" in name or PurePosixPath(name).is_absolute() or ".." in PurePosixPath(name).parts


def _profile_required(algo: CompressionAlgo) -> CodecProfile:
    raise ValueError(f"<profile-required:{algo}>")


# --- [SERVICES] -------------------------------------------------------------------------


class PackWorker(Protocol):
    # both faces of one packer: module-picklable kernels the THREAD/PROCESS offload carries by qualified name,
    # and the uniform producer surface a composite consumer holds as one shape.
    def pack(self, payloads: tuple[bytes, ...], profile: CodecProfile, /) -> tuple[bytes, BundleEvidence]: ...
    def recover(self, blob: bytes, profile: CodecProfile, /) -> tuple[MemberTriple, ...]: ...
    def emit(self, /) -> "ArtifactWork": ...
    async def packed(self, /) -> "RuntimeRail[tuple[bytes, BundleEvidence]]": ...
    async def unpack(self, blob: bytes, /) -> "RuntimeRail[BundleManifest]": ...
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
