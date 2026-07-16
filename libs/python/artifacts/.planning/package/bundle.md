# [PY_ARTIFACTS_BUNDLE]

`Bundle` is the package-plane vocabulary and port floor the three producer arms compose downward — the shared spec carrier, algorithm axis, policy union, manifest, evidence, and worker port every `package/codec#CODEC`, `package/archive#ARCHIVE`, and `package/delta#DELTA` path imports while none imports another. It mints no node and runs no arm; it holds the shapes so the arms hold only behavior.

`Bundle` owns the KEY-OVER-INPUT mint — `ContentIdentity.of` over canonical `msgpack` spec bytes (`algo`, `profile`, per-payload `xxh3_128` digests) ⊕ parent-key bytes, computable before any arm runs so `keyed` admission probes the cache seed and elision fires pre-production. `BundleManifest.of` folds each recovered member through one `member-{algo}` `ContentIdentity.of` so addressing is identical across arms, and `BundleEvidence.receipt` is the ONE projection onto the flat `ArtifactReceipt.Bundle` case (`core/receipt#RECEIPT` the sole downward edge, `receipt.slot == node.key` because the producer threads the same pre-run key). A `PackWorker` `(pack, recover)` port, each arm satisfying it with module-picklable kernels, is why the runtime `THREAD`/`PROCESS` offload carries any arm by qualified name and a composite consumer (`delivery/transmittal#TRANSMITTAL`) holds any packer as one shape.

## [01]-[INDEX]

- [02]-[BUNDLE]: the `Bundle` carrier and pre-run key mint, the `CompressionAlgo` axis and `CodecProfile` knob union, the closed `Literal` vocabularies, the `InPlaceSegments`/`FirmwareLayout` delta bands, the manifest and evidence folds, and the `PackWorker` port.

## [02]-[BUNDLE]

- Owner: `Bundle` the frozen spec carrier (`payloads`, `algo`, `profile`, `parents`) all three producers wrap; `CompressionAlgo` the closed algorithm axis; `CodecProfile` the policy union carrying one typed knob-struct per row — each a picklable `msgspec.Struct` (all-scalar, `frozen=True`) so a profile crosses the `PROCESS` lane as data, never a native handle. `Bundle.key` mints KEY-OVER-INPUT pre-run, so a re-issued identical member set at an identical profile is a `keyed`-admission hit before compression, and the packed blob's output address stays store-side lane evidence, never the elision key.
- Cases: the closed `Literal` axes carry package-constant families as neutral tokens; the VALUE dispatch rows binding tokens to provider constants (`STRATEGY_*`, `FILTER_*`, `MODE_*`, encryption sentinels) live at arm scope on the owning sibling. `DEFAULT_PROFILE` is the per-algorithm policy table; `DELTA` is absent by design (a patch binds its parent image, so `Delta.of` always supplies the profile) and `Bundle.of` resolves absence through `try_find` with a loud construction refusal, never a silent fallback. `InPlaceSegments` and `FirmwareLayout` are the delta knob bands; `FirmwareLayout.of_elf` calls `detools.data_format_from_files` once per image (the bundled `pyelftools` reader), so firmware offsets are provider-derived and the raw ctor stays the pre-resolved shape.
- Output: `BundleManifest.of` mints each `ManifestRow` key through the one `member-{algo}` fold so member addressing is identical across every arm; `BundleEvidence.measure` folds member count and `in_bytes`/`out_bytes` once (`entries` is member arity, never the joined-blob count) and `BundleEvidence.receipt(key)` is the ONE constructor onto `ArtifactReceipt.Bundle` — three producers, one receipt shape, zero per-page spread.
- Packages: `msgspec` (`Struct` knobs/manifest/evidence, `msgpack.encode` the canonical spec bytes), `expression` (`tagged_union`/`case`/`tag` the profile union, `Map.of_seq`/`try_find` the policy table), `xxhash` (`xxh3_128_digest` the pre-run digests), runtime `identity` (`ContentIdentity.of`/`ContentKey`), `detools` (`data_format_from_files`, the `of_elf` resolver alone, lazy).
- Growth: a new algorithm is one `CompressionAlgo` row, one knob-struct, one `CodecProfile` case, one `DEFAULT_PROFILE` row, and one arm on the owning sibling; a new bounded knob value is one token on the owning `Literal` axis plus one arm-scope dispatch row; a new manifest column is one `ManifestRow` field; a new evidence fact is one `BundleEvidence` field threading `measure` and `receipt` — zero new verb, zero page-level surface.
- Boundary: no behavior arm, no provider dispatch values, no offload, no lane, no receipt MINT (the projection constructs the case; producers thread the key and return it on their rails), no artifacts-internal import above the spine — `core/receipt` the one downward edge, codec/archive/delta composing this page but never each other.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from datetime import datetime, timezone
from enum import StrEnum
from typing import Final, Literal, Protocol

import xxhash
from expression import case, tag, tagged_union
from expression.collections import Map
from msgspec import Struct, msgpack

from rasm.runtime.identity import ContentIdentity, ContentKey

from artifacts.core.receipt import ArtifactReceipt

lazy import detools  # reifies only inside FirmwareLayout.of_elf

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
type ZipMechanism = Literal["none", "zipcrypto", "ae1", "ae2", "aes128", "aes192", "aes256"]
type DeltaPatchType = Literal["sequential", "in-place", "bsdiff", "hdiffpatch"]
type DeltaAlgorithm = Literal["bsdiff", "hdiffpatch", "match-blocks"]
type DeltaCompression = Literal["bz2", "crle", "lzma", "zstd", "lz4", "heatshrink", "none"]
type DeltaSuffixArray = Literal["divsufsort", "sais"]
type DataFormat = Literal["arm-cortex-m4", "aarch64", "xtensa-lx106"]
type MemberTriple = tuple[str, int, bytes]  # (name, size, xxh3_128 digest) — the uniform recovery row

# --- [CONSTANTS] ------------------------------------------------------------------------

# fixed ZIP member stamp so an unencrypted container is byte-reproducible and its content key dedups across runs.
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


class BrotliKnobs(Struct, frozen=True, gc=False):
    mode: BrotliMode = "generic"
    quality: int = 11
    lgwin: int = 22
    lgblock: int = 0


class GzipKnobs(Struct, frozen=True, gc=False):
    level: int = 9
    mtime: float = 0.0
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
    mechanisms: tuple[ZipMechanism, ...] = ("none", "ae2", "aes256")
    password: str | None = None
    names: tuple[str, ...] = ()
    modified_at: datetime = _EPOCH
    mode: int = 0o600


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

    @classmethod
    def of_elf(
        cls,
        data_format: DataFormat,
        from_elf: str,
        from_bin: str,
        to_elf: str,
        to_bin: str,
        *,
        from_offset: int | None = None,
        to_offset: int | None = None,
    ) -> "FirmwareLayout":
        # one call per image returns (data_offset_begin, data_offset_end, code_begin, code_end, data_begin, data_end).
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

    def kwargs(self) -> dict[str, object]:  # dense range pairs project to the flat detools offset kwargs at the edge
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
            algo, tuple(ManifestRow(name, ContentIdentity.of(f"member-{algo}", digest), algo, size) for name, size, digest in recovered)
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
        algo: CompressionAlgo, level: int, dict_id: int, frame_size: int, verified: int, payloads: tuple[bytes, ...], blobs: tuple[bytes, ...]
    ) -> "BundleEvidence":
        # entries = member arity (len payloads), never the joined-blob count
        return BundleEvidence(algo, level, dict_id, frame_size, len(payloads), verified, sum(map(len, payloads)), sum(map(len, blobs)))

    def receipt(self, key: ContentKey, /) -> ArtifactReceipt:
        # threads the caller's pre-run key so receipt.slot == node.key
        return ArtifactReceipt.Bundle(key, self.algo.value, self.level, self.dict_id, self.frame_size, self.entries, self.verified, self.ratio)


class Bundle(Struct, frozen=True):
    payloads: tuple[bytes, ...]
    algo: CompressionAlgo
    profile: CodecProfile
    parents: tuple[ContentKey, ...] = ()

    @property
    def key(self) -> ContentKey:
        # canonical spec bytes (algo, profile, per-member xxh3_128 digests) ⊕ parent keys, minted pre-run for keyed-admission elision.
        spec = msgpack.encode((self.algo.value, self.profile, tuple(xxhash.xxh3_128_digest(p) for p in self.payloads)))
        return ContentIdentity.of(f"bundle-{self.algo}", spec + b"".join(k.hex.encode() for k in self.parents))

    @staticmethod
    def of(
        algo: CompressionAlgo, *payloads: bytes, profile: CodecProfile | None = None, parents: tuple[ContentKey, ...] = ()
    ) -> "Bundle":
        resolved = profile if profile is not None else DEFAULT_PROFILE.try_find(algo).default_with(lambda: _profile_required(algo))
        return Bundle(payloads=payloads, algo=algo, profile=resolved, parents=parents)


# --- [TABLES] ---------------------------------------------------------------------------

DEFAULT_PROFILE: Final[Map[CompressionAlgo, CodecProfile]] = Map.of_seq([
    (CompressionAlgo.ZSTD, CodecProfile(zstd=ZstdKnobs())),
    (CompressionAlgo.LZ4, CodecProfile(lz4=Lz4Knobs())),
    (CompressionAlgo.BROTLI, CodecProfile(brotli=BrotliKnobs())),
    (CompressionAlgo.GZIP, CodecProfile(gzip=GzipKnobs())),
    (CompressionAlgo.SEVEN_Z, CodecProfile(seven_z=SevenZKnobs())),
    (CompressionAlgo.ZIP_STREAM, CodecProfile(zip_stream=ZipStreamKnobs())),
])


def _profile_required(algo: CompressionAlgo) -> CodecProfile:
    raise ValueError(f"<profile-required:{algo}>")  # loud construction refusal


# --- [SERVICES] -------------------------------------------------------------------------


class PackWorker(Protocol):
    # module-picklable kernels so the THREAD/PROCESS offload carries pack/recover by qualified name.
    def pack(self, payloads: tuple[bytes, ...], algo: CompressionAlgo, profile: CodecProfile, /) -> tuple[bytes, BundleEvidence]: ...
    def recover(self, blob: bytes, algo: CompressionAlgo, profile: CodecProfile, /) -> tuple[MemberTriple, ...]: ...
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
