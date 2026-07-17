# [PY_ARTIFACTS_DETECT]

`Detect` is the ingest-boundary format-identification owner: one polymorphic `of` admits a `Source` or an `Iterable[Source]` and sniffs each payload into a typed `DetectIdentity` — MIME, description, charset, valid-extension tuple, the derived `MediaClass` routing discriminant, the orthogonal `Container` structural discriminant (the ZIP/OLE/TAR/compression kind a consumer routes a second-pass unpacker on), the multi-match set, the ingress-declared `claimed` type, the `Trust` declared-vs-sniffed verdict, the sniff `confidence`, the resolving `engine`, byte length, and libmagic version. It is the DUAL-ENGINE owner over ONE surface: `puremagic` is the in-process pure-Python DEFAULT whose `single_deep_scan` resolves the `PK\x03\x04` ZIP and `\xd0\xcf\x11\xe0` CFBF containers to the EXACT OOXML/ODF/EPUB/USDZ + legacy-Office subtype (closing the `application/zip`/`application/CDFV2` floor) and whose float `confidence` is the native ambiguity signal `libmagic` can only approximate through `keep_going`; `python-magic`/libmagic is the broad-leaf-signature fallback ONLY where its compiled database recognizes a niche signature `magic_data.json` lacks — a stronger-on-a-distinct-axis retention, never an overlap.

`DetectEngine` (`PUREMAGIC`/`LIBMAGIC`/`LAYERED`) composes the arms over the caller-threaded `LanePolicy.offload` seam: `puremagic` rides the loader path as a `KernelTrait.RELEASING` thread kernel, while `libmagic` — a Forge-provisioned host dependency off the loader path — crosses as `KernelTrait.HOSTILE` onto the warm process pool under its trait-row worker-death retry; `LAYERED` escalates ONLY a resolved-but-`UNKNOWN` verdict to libmagic, so a confident sniff never pays the crossing and a provisioning fault keeps the honest `UNKNOWN`. `Detect` sits STRATUM 1 — it imports nothing artifacts-internal, and `graphic/raster/io`, `document/lens`, and `document/emit` import it downward. Every detection resolves the `DetectIdentity` the per-format reader dispatch reads through `MediaClass`, minting no `ContentKey` (the `evidence/identity#IDENTITY` owner's concern) and reading no descriptive metadata inside the container (`exchange/metadata#METADATA`'s).

## [01]-[INDEX]

- [01]-[DETECT]: the dual-engine format-ID owner — `puremagic` in-process default and `libmagic` process-crossing fallback under one `DetectEngine` axis over the caller-threaded `LanePolicy` seam — sniffing a `Source` into the typed `DetectIdentity` the consumers route on.

## [02]-[DETECT]

- Owner: `Detect` carries the offload seam, the `DetectEngine` axis, a `DetectProfile`, a `DetectPolicy`, a `deepscan` toggle, and a `Disposition` batch-combination policy. `Source` is the closed `Buffer`/`File` admission family — a `File`'s path is read WORKER-SIDE so the parent never pickles the payload across the process seam, and each `@beartype(conf=FAULT_CONF)`-woven factory lifts a malformed `bytes`/`Path` ingress onto the fault rail rather than a native `TypeError` deep in the sniff. `DetectProfile` is the facet-cookie selector on the libmagic arm alone — the `puremagic` arm resolves the full identity in one roster call. `MagicFacet` pins exactly one `Magic` cookie flag per facet through `_FACET_FLAG` because libmagic cooks one string per call, never a `tuple[bool, bool, bool]` triple. `DetectPolicy` folds the `DetectFlag` set, the `MagicParam` recursion/ELF caps, the `CheckClass` test-class narrowing, and the custom `.mgc` `magic_db` into ONE value instead of per-call flag arguments. `MediaClass` (routing) and `Container` (structural) are orthogonal discriminants both from the one sniffed MIME via `_classified`'s exact-then-longest-prefix fold, so the longest registered prefix wins and an OOXML compound subtype routes correctly even when the deep-scan appends the `.document`/`.sheet` suffix. `Trust` folds `media_class`/`container`/multi-match/`extensions`/`claimed` AND `confidence` in one pass — a sub-`_CONFIDENCE_FLOOR` top match collapses to `UNKNOWN`, two distinct strong matches to `AMBIGUOUS` — so a spoofed, polyglot, or low-confidence payload is a stated verdict, not silently-discarded evidence. `DetectIdentity` IS its own wire/egress projection a consumer renders through `msgspec.to_builtins` directly, never a forwarding `facts()` hop.
- Cases: `DetectEngine` — `PUREMAGIC` (`_pure_detect` over the roster on the `RELEASING` thread arm, deep-scan folding the exact ZIP/CFBF subtype) · `LIBMAGIC` (`_gated_detect` over the flag-pinned `Magic` cookie as a `HOSTILE` kernel) · `LAYERED` (`_pure` first, `_libmagic` escalated ONLY on a `Trust.UNKNOWN` verdict, `or_else_with` keeping the puremagic `UNKNOWN` when the libmagic worker itself faults) — one total `match` in `_railed`. `DetectProfile` rows select the `_PROFILE_FACETS` facet set (`IDENTITY` cooking `MIME`/`DESCRIPTION`/`ENCODING`/`EXTENSION` each in its own cookie in one libmagic crossing). `MagicParam` caps and `CheckClass` narrowing apply through `setparam`/`magic_setflags` to harden untrusted ingest against unbounded recursion and ELF-table bombs. `MediaClass` routes: `MODEL` sends IANA `model/*` to `scene/stage#STAGE`/`scene/export#EXPORT`, `EBOOK` sends epub/mobi to `pymupdf`, `DATA` sends json/xml/yaml/toml/csv to the structured readers a bare `text/` prefix floors to `TEXT`. `Container` is the parallel structural axis (`ZIP`/`OLE`/per-compression) `media_class` cannot answer, so a consumer routes reader on one and second-pass unpacker on the other without a re-sniff. `Trust`: `UNKNOWN` for the octet-stream floor, the `MediaClass.UNKNOWN` sniff, or a sub-floor confidence; `AMBIGUOUS` for a polyglot; `MISMATCH` for a cross-class claim or a same-class `guess_all_extensions` disjoint from the sniffed `extensions`; `IDENTIFIED` for an agreeing sniff or a generic-container claim (`application/zip` for a `.docx`) whose `Container` matches the sniffed one — the ARCHIVE-gated generalization the cross-class check otherwise false-flags, needed less now the deep-scan resolves the exact subtype.
- Auto: `_pure_detect` reads `_pure_roster` — a `Buffer` spills to a bounded `NamedTemporaryFile` so its ZIP/CFBF central directory resolves the exact subtype at `confidence == 1.0`, a `File` deep-scans natively, and either drops to the no-I/O `identify_all` head+foot when `deepscan` is off — traps `PureError`/`PureValueError` to an empty roster (an unmatched payload is the gate's honest `UNKNOWN`), takes the top head, and folds the charset through `_charset` (gated to the text-family class) into the `encoding` the libmagic `mime_encoding` cook alone set before, so BOTH arms converge on a populated `encoding`. `_gated_detect` reads `magic.version()` once (the `EXTENSION` facet gated `>= _EXTENSION_MIN`), cooks each facet through the memoised `_cookie`, and trusts its single match at `confidence=1.0` since libmagic carries no confidence. `_handle509Bug`'s null-result quirk returns `application/octet-stream` (a valid unknown-content MIME classified `UNKNOWN`), never an escaping exception.
- Receipt: `Detect` is the format-ID GATE, not a producer — the S1 seams `graphic/raster/io`, `document/lens`, and `document/emit` all import detect DOWNWARD, with no `→ core/receipt` or `→ core/plan` edge, so it contributes no `ArtifactReceipt` and mints no `ContentKey` (the `evidence/identity#IDENTITY` hash a producing owner mints over emitted bytes, which `Detect` neither computes nor folds into). Each detection resolves the `DetectIdentity` the document/PDF/image/scene owners read before per-format dispatch. `MediaClass` is the consumers' routing discriminant (`PDF`→`pymupdf`, `WORD`→`python-docx`, `SPREADSHEET`→`openpyxl`, `MODEL`→`scene/stage#STAGE`, `DATA`→`msgspec`/`lxml`, …) — the deep-scan splitting `.docx`/`.xlsx`/`.pptx` to their exact subtypes so one OOXML class never fans to three packages; the consumers own the `MediaClass`→reader table, this owner only the classification, and the descriptive metadata INSIDE the container is `exchange/metadata#METADATA`'s concern.
- Packages: `puremagic` (`magic_file`/`magic_string`/`magic_stream` the confidence-ranked roster, unknown resolving `[]` under `raise_on_none=False` never a raise; `main.file_details`/`string_details`/`identify_all` the no-I/O head+foot; `single_deep_scan` the exact OOXML/CFBF subtype through the `zip_scanner`/`cfbf_scanner`; `ext_from_filename`/`from_extension` the ext↔MIME reverse-lookup, `PureError` on an unregistered ext; `text_scanner.decode_any` returning `(text, encoding)` and raising `TypeError` on an undecodable head; the two roster-trapped faults `PureError`(LookupError)/`PureValueError`(ValueError); the bundled `magic_data.json` needs no `.mgc`); `python-magic` (`Magic(...)` — the combined `mime=True, mime_encoding=True` cook returns the fused `text/plain; charset=utf-8` form the one-flag-per-facet law forbids; `version()` raising `NotImplementedError` on an ancient lib is the `_EXTENSION_MIN` gate; `magic_setflags` the raw-bit `MAGIC_NO_CHECK_*` binding no constructor boolean covers; `import magic` raises `ImportError` off the loader path so it is reified worker-side only; `Magic.from_descriptor` excluded — a parent-process fd never crosses the process seam); runtime (`LanePolicy.offload`, `faults.traversed`, `FAULT_CONF`); `pydantic-settings`; `expression`; `msgspec`; stdlib `mimetypes` (`guess_file_type`/`guess_all_extensions` the thin fallback and same-class spoof set).
- Growth: a new engine is one `DetectEngine` member plus one `_railed` arm; a new facet one `MagicFacet` plus one `_FACET_FLAG` row; a new profile one `DetectProfile` plus one `_PROFILE_FACETS` row; a new libmagic flag one `DetectFlag`; a tuning param one `MagicParam`; a test-class narrowing one `CheckClass`; a routing branch one `MediaClass` plus one `_MEDIA_CLASS` row; a container kind one `Container` plus one `_CONTAINER` row; a source kind one `Source` case; an ingest verdict one `Trust` member plus one `_trust` arm; an identity fact one `DetectIdentity` field; a deployment knob one `DetectSettings` field plus one `detector()` projection; the singular/batch modality rides the existing `of(Source | Iterable[Source])`; zero new surface.
- Boundary: `Detect` owns content sniffing only — no `ContentKey`, no `ArtifactReceipt`, and no descriptive-metadata read inside the container (`exchange/metadata#METADATA`'s concern). `puremagic`'s default rides the loader path IN-PROCESS as a `RELEASING` kernel (no native dependency to reify, so the process crossing and pickle seam the libmagic arm pays both drop), while libmagic — off the loader path — crosses as `HOSTILE` under its trait-row worker-death retry. Not a single-engine libmagic-only owner: `puremagic` is the categorical-best default and libmagic the distinct-axis fallback. `lane.offload` owns isolation/band/retry/boundary, never a folder-minted limiter beside it. `DetectSettings` admits the deployment env once, never raw `os.environ` reads.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
import mimetypes
from collections.abc import Iterable
from dataclasses import dataclass, field
from enum import StrEnum
from functools import cache, reduce
from operator import or_
from pathlib import Path
from tempfile import NamedTemporaryFile
from typing import Final, Literal, assert_never, overload

import anyio
from anyio import TaskHandle
from beartype import beartype
from builtins import frozendict
from expression import case, tag, tagged_union
from expression.collections import Block, Map
from expression.extra.result import catch
from msgspec import Struct
from pydantic_settings import BaseSettings, SettingsConfigDict

from rasm.runtime.faults import FAULT_CONF, BoundaryFault, Disposition, RuntimeRail, traversed
from rasm.runtime.lanes import LanePolicy
from rasm.runtime.workers import Kernel, KernelTrait

lazy from puremagic import (
    PureError,
    PureMagicWithConfidence,
    ext_from_filename,
    from_extension,
    magic_file,
)  # pure-Python default, on the loader path
lazy from puremagic.main import PureValueError, file_details, identify_all, string_details
lazy from puremagic.scanners import text_scanner  # in-process charset facet, symmetric with the libmagic mime_encoding cook
lazy import magic  # libmagic native dep, off the loader path; reified worker-side in the PROCESS offload


# --- [TYPES] ----------------------------------------------------------------------------
class DetectEngine(StrEnum):
    PUREMAGIC = "puremagic"  # pure-Python default on the RELEASING thread kernel, confidence roster + deep-scan exact subtypes
    LIBMAGIC = "libmagic"  # native libmagic fallback, broad leaf-signature database, HOSTILE process kernel
    LAYERED = "layered"  # puremagic default → libmagic escalation on a Trust.UNKNOWN verdict


class MagicFacet(StrEnum):
    MIME = "mime"
    ENCODING = "encoding"
    EXTENSION = "extension"
    DESCRIPTION = "description"


class DetectFlag(StrEnum):
    UNCOMPRESS = "uncompress"
    KEEP_GOING = "keep_going"
    RAW = "raw"


class MagicParam(StrEnum):
    INDIR_MAX = "MAGIC_PARAM_INDIR_MAX"
    NAME_MAX = "MAGIC_PARAM_NAME_MAX"
    REGEX_MAX = "MAGIC_PARAM_REGEX_MAX"
    BYTES_MAX = "MAGIC_PARAM_BYTES_MAX"
    ELF_NOTES_MAX = "MAGIC_PARAM_ELF_NOTES_MAX"
    ELF_PHNUM_MAX = "MAGIC_PARAM_ELF_PHNUM_MAX"
    ELF_SHNUM_MAX = "MAGIC_PARAM_ELF_SHNUM_MAX"


# member values are the `magic.MAGIC_NO_CHECK_*` module ordinals; `getattr(magic, value)` resolves the raw
# bit the cookie disables through `magic_setflags` (no `Magic` constructor boolean exists for these).
class CheckClass(StrEnum):
    COMPRESS = "MAGIC_NO_CHECK_COMPRESS"
    TAR = "MAGIC_NO_CHECK_TAR"
    SOFT = "MAGIC_NO_CHECK_SOFT"
    APPTYPE = "MAGIC_NO_CHECK_APPTYPE"
    ELF = "MAGIC_NO_CHECK_ELF"
    ASCII = "MAGIC_NO_CHECK_ASCII"
    TROFF = "MAGIC_NO_CHECK_TROFF"
    FORTRAN = "MAGIC_NO_CHECK_FORTRAN"
    TOKENS = "MAGIC_NO_CHECK_TOKENS"


class DetectProfile(StrEnum):
    MIME = "mime"
    DESCRIBE = "describe"
    IDENTITY = "identity"


class MediaClass(StrEnum):
    PDF = "pdf"
    EBOOK = "ebook"
    WORD = "word"
    SPREADSHEET = "spreadsheet"
    PRESENTATION = "presentation"
    OFFICE_ODF = "office-odf"
    OFFICE_LEGACY = "office-legacy"
    ENCRYPTED = "encrypted"
    VECTOR = "vector"
    IMAGE = "image"
    AUDIO = "audio"
    VIDEO = "video"
    MODEL = "model"
    ARCHIVE = "archive"
    FONT = "font"
    DATA = "data"
    TEXT = "text"
    UNKNOWN = "unknown"

    @staticmethod
    def of(mime: str, /) -> "MediaClass":
        return _classified(mime, _MEDIA_CLASS, MediaClass.UNKNOWN)


class Container(StrEnum):  # the structural container/compression kind orthogonal to MediaClass — which unpacker a second-pass peek routes to
    NONE = "none"  # a leaf format with no wrapping structure
    ZIP = "zip"  # the OOXML/ODF/EPUB/USDZ/CBZ + bare-zip family a docx/xlsx/epub sits inside
    OLE = "ole"  # the CFB/CDFV2 compound the legacy Office + MSI family sits inside
    TAR = "tar"
    SEVENZIP = "sevenzip"
    GZIP = "gzip"
    BZIP2 = "bzip2"
    XZ = "xz"
    ZSTD = "zstd"
    LZ4 = "lz4"

    @staticmethod
    def of(mime: str, /) -> "Container":
        return _classified(mime, _CONTAINER, Container.NONE)


class Trust(StrEnum):  # the declared-vs-sniffed ingest verdict the gate folds over its own evidence
    IDENTIFIED = "identified"  # one confident content match, agreeing with the claim or unclaimed
    AMBIGUOUS = "ambiguous"  # two distinct strong matches — a polyglot (the puremagic confidence tail)
    MISMATCH = "mismatch"  # the sniffed class disagrees with the declared content-type — spoofing
    UNKNOWN = "unknown"  # the octet-stream floor, an UNKNOWN class, or a sub-floor confidence


@tagged_union(frozen=True)
class Source:
    tag: Literal["buffer", "file"] = tag()
    buffer: tuple[bytes, str] = case()  # (payload, declared content-type — "" when the ingress claims none)
    file: tuple[Path, str] = case()  # (path, declared content-type — "" falls back to the extension MIME)

    @staticmethod
    @beartype(conf=FAULT_CONF)
    def Buffer(payload: bytes, /, *, claimed: str = "") -> "Source":
        return Source(buffer=(payload, claimed))

    @staticmethod
    @beartype(conf=FAULT_CONF)
    def File(path: Path, /, *, claimed: str = "") -> "Source":
        return Source(file=(path, claimed))

    @property
    def length(self) -> int:
        match self:
            case Source(tag="buffer", buffer=(payload, _)):
                return len(payload)
            case Source(tag="file", file=(path, _)):
                return path.stat().st_size
            case _ as unreachable:
                assert_never(unreachable)

    @property
    def claimed(self) -> str:  # the explicit ingress claim, or for a File the puremagic ext<->MIME resolution
        match self:
            case Source(tag="buffer", buffer=(_, declared)):
                return declared
            case Source(tag="file", file=(path, declared)):
                return declared or _claim_mime(path)
            case _ as unreachable:
                assert_never(unreachable)


# --- [CONSTANTS] ------------------------------------------------------------------------
_EXTENSION_MIN: Final[int] = 524  # libmagic floor for MAGIC_EXTENSION
_CONTINUE_SEP: Final[str] = "\n- "  # libmagic MAGIC_CONTINUE multi-match separator
_CONFIDENCE_FLOOR: Final[float] = 0.3  # puremagic strong-signature floor: header/footer/deep-scan matches clear it, extension-only guesses fall below
_CHARSET_HEAD: Final[int] = 4096  # bounded leading-byte budget for the in-process charset sniff, matching the libmagic mime_encoding read

_FACET_FLAG: Final[Map[MagicFacet, str]] = Map.of_seq([
    (MagicFacet.MIME, "mime"),
    (MagicFacet.ENCODING, "mime_encoding"),
    (MagicFacet.EXTENSION, "extension"),
])

_PROFILE_FACETS: Final[Map[DetectProfile, tuple[MagicFacet, ...]]] = Map.of_seq([
    (DetectProfile.MIME, (MagicFacet.MIME,)),
    (DetectProfile.DESCRIBE, (MagicFacet.DESCRIPTION,)),
    (DetectProfile.IDENTITY, (MagicFacet.MIME, MagicFacet.DESCRIPTION, MagicFacet.ENCODING, MagicFacet.EXTENSION)),
])

_MEDIA_CLASS: Final[Map[str, MediaClass]] = Map.of_seq([
    ("application/pdf", MediaClass.PDF),
    ("application/epub+zip", MediaClass.EBOOK),
    ("application/x-mobipocket-ebook", MediaClass.EBOOK),
    ("application/encrypted", MediaClass.ENCRYPTED),
    ("application/x-ole-storage", MediaClass.OFFICE_LEGACY),
    ("application/CDFV2", MediaClass.OFFICE_LEGACY),
    ("application/msword", MediaClass.OFFICE_LEGACY),
    ("application/vnd.ms-excel", MediaClass.OFFICE_LEGACY),
    ("application/vnd.ms-powerpoint", MediaClass.OFFICE_LEGACY),
    ("image/svg+xml", MediaClass.VECTOR),
    ("application/zip", MediaClass.ARCHIVE),
    ("application/x-7z-compressed", MediaClass.ARCHIVE),
    ("application/gzip", MediaClass.ARCHIVE),
    ("application/x-tar", MediaClass.ARCHIVE),
    ("application/x-bzip2", MediaClass.ARCHIVE),
    ("application/x-xz", MediaClass.ARCHIVE),
    ("application/zstd", MediaClass.ARCHIVE),
    ("application/x-lz4", MediaClass.ARCHIVE),
    ("application/vnd.openxmlformats-officedocument.wordprocessingml", MediaClass.WORD),
    ("application/vnd.openxmlformats-officedocument.spreadsheetml", MediaClass.SPREADSHEET),
    ("application/vnd.openxmlformats-officedocument.presentationml", MediaClass.PRESENTATION),
    ("application/vnd.oasis.opendocument", MediaClass.OFFICE_ODF),
    ("application/vnd.ms-opentype", MediaClass.FONT),
    ("application/font", MediaClass.FONT),
    ("application/json", MediaClass.DATA),
    ("application/xml", MediaClass.DATA),
    ("text/xml", MediaClass.DATA),
    ("application/yaml", MediaClass.DATA),
    ("application/x-yaml", MediaClass.DATA),
    ("text/yaml", MediaClass.DATA),
    ("application/toml", MediaClass.DATA),
    ("text/csv", MediaClass.DATA),
    ("application/csv", MediaClass.DATA),
    ("model/gltf-binary", MediaClass.MODEL),
    ("model/gltf+json", MediaClass.MODEL),
    ("model/vnd.usdz+zip", MediaClass.MODEL),
    ("model/stl", MediaClass.MODEL),
    ("model/obj", MediaClass.MODEL),
    ("model/3mf", MediaClass.MODEL),
    ("image/", MediaClass.IMAGE),
    ("audio/", MediaClass.AUDIO),
    ("video/", MediaClass.VIDEO),
    ("model/", MediaClass.MODEL),
    ("font/", MediaClass.FONT),
    ("text/", MediaClass.TEXT),
])

# structural container the sniffed MIME sits inside, orthogonal to MediaClass: a `.docx` is
# (WORD, ZIP), a legacy `.doc` is (OFFICE_LEGACY, OLE), a bare archive is (ARCHIVE, its compression
# kind) — the discriminant a consumer routes a second-pass unpacker (stream-unzip/olefile/py7zr/…) on.
_CONTAINER: Final[Map[str, Container]] = Map.of_seq([
    ("application/zip", Container.ZIP),
    ("application/epub+zip", Container.ZIP),
    ("application/vnd.comicbook+zip", Container.ZIP),
    ("application/vnd.openxmlformats-officedocument.wordprocessingml", Container.ZIP),
    ("application/vnd.openxmlformats-officedocument.spreadsheetml", Container.ZIP),
    ("application/vnd.openxmlformats-officedocument.presentationml", Container.ZIP),
    ("application/vnd.oasis.opendocument", Container.ZIP),
    ("model/vnd.usdz+zip", Container.ZIP),
    ("application/x-ole-storage", Container.OLE),
    ("application/CDFV2", Container.OLE),
    ("application/msword", Container.OLE),
    ("application/vnd.ms-excel", Container.OLE),
    ("application/vnd.ms-powerpoint", Container.OLE),
    ("application/x-tar", Container.TAR),
    ("application/x-7z-compressed", Container.SEVENZIP),
    ("application/gzip", Container.GZIP),
    ("application/x-bzip2", Container.BZIP2),
    ("application/x-xz", Container.XZ),
    ("application/zstd", Container.ZSTD),
    ("application/x-lz4", Container.LZ4),
])


# --- [MODELS] ---------------------------------------------------------------------------
@dataclass(frozen=True, slots=True, kw_only=True)
class DetectPolicy:  # the libmagic-arm behavior value; the puremagic default reads only `deepscan` off the owner
    flags: frozenset[DetectFlag] = field(default_factory=frozenset)
    params: frozendict[MagicParam, int] = field(default_factory=frozendict)
    no_check: frozenset[CheckClass] = field(default_factory=frozenset)
    magic_db: Path | None = None


class DetectIdentity(Struct, frozen=True, gc=False):
    mime: str = ""
    description: str = ""
    encoding: str = ""
    extensions: tuple[str, ...] = ()
    media_class: MediaClass = MediaClass.UNKNOWN
    container: Container = Container.NONE
    matches: tuple[str, ...] = ()
    claimed: str = ""
    trust: Trust = Trust.UNKNOWN
    confidence: float = 0.0
    engine: DetectEngine = DetectEngine.PUREMAGIC
    byte_length: int = 0
    libmagic_version: int = 0


# --- [SERVICES] -------------------------------------------------------------------------
class DetectSettings(BaseSettings):  # admitted once at the composition root; the deployment env → configured Detect
    model_config = SettingsConfigDict(env_prefix="RASM_DETECT_", frozen=True, extra="forbid")
    engine: DetectEngine = DetectEngine.LAYERED
    deepscan: bool = True
    magic_db: Path | None = None  # the discovery-env → configured-path → bundled-fallback libmagic .mgc database

    def detector(self, lane: LanePolicy, /) -> "Detect":
        return Detect(lane=lane, engine=self.engine, deepscan=self.deepscan, policy=DetectPolicy(magic_db=self.magic_db))


@dataclass(frozen=True, slots=True, kw_only=True)
class Detect:
    lane: LanePolicy  # the caller-threaded offload seam — isolation, band, retry, and boundary are runtime-owned
    engine: DetectEngine = DetectEngine.LAYERED
    profile: DetectProfile = DetectProfile.IDENTITY
    policy: DetectPolicy = field(default_factory=DetectPolicy)
    disposition: Disposition = Disposition.ABORT
    deepscan: bool = True

    @overload
    async def of(self, source: Source, /) -> RuntimeRail[DetectIdentity]: ...
    @overload
    async def of(
        self, source: Iterable[Source], /
    ) -> RuntimeRail[Block[DetectIdentity]] | RuntimeRail[tuple[Block[DetectIdentity], Block[BoundaryFault]]]: ...
    async def of(
        self, source: Source | Iterable[Source], /
    ) -> RuntimeRail[DetectIdentity] | RuntimeRail[Block[DetectIdentity]] | RuntimeRail[tuple[Block[DetectIdentity], Block[BoundaryFault]]]:
        match source:
            case Source() as one:
                return await self._railed(one)
            case sources:
                return await self._many(Block.of_seq(sources))

    async def _railed(self, source: Source, /) -> RuntimeRail[DetectIdentity]:
        match self.engine:
            case DetectEngine.PUREMAGIC:
                return await self._pure(source)
            case DetectEngine.LIBMAGIC:
                return await self._libmagic(source)
            case DetectEngine.LAYERED:
                return await self._layered(source)
            case _ as unreachable:
                assert_never(unreachable)

    async def _pure(self, source: Source, /) -> RuntimeRail[DetectIdentity]:
        # in-process THREAD_BAND parse; no retry row — nothing transient to recover
        return await self.lane.offload(Kernel.of(_pure_detect, KernelTrait.RELEASING), source, self.deepscan)

    async def _libmagic(self, source: Source, /) -> RuntimeRail[DetectIdentity]:
        # OCCT recovers a transient worker death, never a deterministic crash; the MagicParam caps are the bomb defense
        return await self.lane.offload(Kernel.of(_gated_detect, KernelTrait.HOSTILE), source, self.profile, self.policy)

    async def _layered(self, source: Source, /) -> RuntimeRail[DetectIdentity]:
        # escalate ONLY a resolved-but-UNKNOWN verdict; a libmagic provisioning fault keeps the puremagic UNKNOWN
        primary = await self._pure(source)
        resolved = primary.to_option()
        if resolved.is_none() or resolved.value.trust is not Trust.UNKNOWN:
            return primary
        return (await self._libmagic(source)).or_else_with(lambda _fault: primary)

    async def _many(
        self, sources: Block[Source], /
    ) -> RuntimeRail[Block[DetectIdentity]] | RuntimeRail[tuple[Block[DetectIdentity], Block[BoundaryFault]]]:
        async with anyio.create_task_group() as group:
            handles: Block[TaskHandle[RuntimeRail[DetectIdentity]]] = sources.map(lambda one: group.start_soon(self._railed, one))
        return traversed(handles.map(lambda handle: handle.return_value), by=self.disposition)


# --- [OPERATIONS] -----------------------------------------------------------------------
def _classified[E](mime: str, table: Map[str, E], default: E, /) -> E:
    # exact probe first, then the longest registered prefix the mime extends — the compound subtype
    # (`...wordprocessingml` catching the appended `.document`/`.sheet`) wins, no shorter prefix shadowing it.
    widest = lambda: max((key for key in table if mime.startswith(key)), key=len, default="")
    return table.try_find(mime).default_with(lambda: table[prefix] if (prefix := widest()) else default)


def _trust(
    mime: str,
    media_class: MediaClass,
    container: Container,
    extensions: tuple[str, ...],
    matches: tuple[str, ...],
    claimed: str,
    confidence: float,
    /,
) -> Trust:
    declared = MediaClass.of(claimed) if claimed else MediaClass.UNKNOWN
    claimed_container = Container.of(claimed) if claimed else Container.NONE
    # claim's valid extensions, dot-stripped to the sniffed form
    claimed_exts = frozenset(suffix.lstrip(".") for suffix in mimetypes.guess_all_extensions(claimed))
    return (
        Trust.UNKNOWN
        if media_class is MediaClass.UNKNOWN or mime in ("", "application/octet-stream") or confidence < _CONFIDENCE_FLOOR
        else Trust.AMBIGUOUS
        if len(matches) > 1  # two distinct strong matches — a polyglot
        else Trust.IDENTIFIED
        # a generic-container claim naming the sniffed container is a generalization, not a spoof
        if declared is MediaClass.ARCHIVE and claimed_container is container is not Container.NONE
        else Trust.MISMATCH
        if declared is not MediaClass.UNKNOWN and declared is not media_class  # cross-class spoof
        else Trust.MISMATCH
        if declared is media_class and extensions and claimed_exts and claimed_exts.isdisjoint(extensions)  # same-class extension/label spoof
        else Trust.IDENTIFIED
    )


def _claim_mime(path: Path, /) -> str:
    # File claim over puremagic's richer ext<->MIME table: `ext_from_filename` recovers a compound
    # extension the stdlib split drops, `from_extension` maps it (`PureError` trapped to the stdlib fallback).
    ext = ext_from_filename(path)
    stdlib = mimetypes.guess_file_type(path)[0] or ""
    return catch(exception=PureError)(from_extension)(ext).default_value(stdlib) if ext else stdlib


def _charset(source: Source, media_class: MediaClass, /) -> str:
    # in-process charset facet symmetric with the libmagic `mime_encoding` cook: `text_scanner.decode_any`
    # over a bounded head, `TypeError`-trapped (undecodable binary narrows to ""), gated to the text-family class.
    if media_class not in (MediaClass.TEXT, MediaClass.DATA):
        return ""
    match source:
        case Source(tag="buffer", buffer=(payload, _)):
            head = payload[:_CHARSET_HEAD]
        case Source(tag="file", file=(path, _)):
            with path.open("rb") as handle:  # Exemption: bounded head read for the charset sniff, worker-side blocking I/O
                head = handle.read(_CHARSET_HEAD)
        case _ as unreachable:
            assert_never(unreachable)
    return catch(exception=TypeError)(text_scanner.decode_any)(head).map(lambda decoded: decoded[1]).default_value("") if head else ""


def _pure_roster(source: Source, deepscan: bool, /) -> Block[PureMagicWithConfidence]:
    # confidence-ranked roster; a Buffer spills to a bounded temp file so its ZIP/CFBF central directory
    # resolves the exact subtype, deepscan off drops to the no-I/O head+foot; PureError/PureValueError → empty.
    try:
        match source:
            case Source(tag="buffer", buffer=(payload, _)) if deepscan:
                # Exemption: puremagic deep-scan reads a real path for the ZIP/CFBF central directory; the
                # default `delete=True` reclaims the spill at context exit on every path, `delete_on_close`
                # only deferring the unlink past the read.
                with NamedTemporaryFile(delete_on_close=False) as spill:
                    spill.write(payload)
                    spill.flush()
                    return Block.of_seq(magic_file(spill.name))
            case Source(tag="buffer", buffer=(payload, _)):
                head, foot = string_details(payload)
                return Block.of_seq(identify_all(head, foot))
            case Source(tag="file", file=(path, _)) if deepscan:
                return Block.of_seq(magic_file(path))
            case Source(tag="file", file=(path, _)):
                head, foot = file_details(path)
                return Block.of_seq(identify_all(head, foot))
            case _ as unreachable:
                assert_never(unreachable)
    except PureError, PureValueError:
        return Block.empty()


def _pure_detect(source: Source, deepscan: bool, /) -> DetectIdentity:
    roster = _pure_roster(source, deepscan)
    strong = roster.filter(lambda match: match.confidence >= _CONFIDENCE_FLOOR and bool(match.mime_type))
    top = roster.try_head()
    mime = top.map(lambda match: match.mime_type).default_value("")
    media_class, container = MediaClass.of(mime), Container.of(mime)
    matches = tuple(dict.fromkeys(match.mime_type for match in strong))  # distinct strong MIMEs — a polyglot when > 1
    extensions = tuple(dict.fromkeys(ext for match in roster if (ext := match.extension.lstrip("."))))
    confidence = top.map(lambda match: match.confidence).default_value(0.0)
    claimed = source.claimed
    return DetectIdentity(
        mime=mime,
        description=top.map(lambda match: match.name).default_value(""),
        encoding=_charset(source, media_class),
        extensions=extensions,
        media_class=media_class,
        container=container,
        matches=matches,
        claimed=claimed,
        trust=_trust(mime, media_class, container, extensions, matches, claimed, confidence),
        confidence=confidence,
        engine=DetectEngine.PUREMAGIC,
        byte_length=source.length,
    )


@cache
def _cookie(
    magic_db: Path | None,
    facet_flag: str | None,
    flagged: frozendict[str, bool],
    params: frozendict[MagicParam, int],
    no_check: frozenset[CheckClass],
    /,
) -> "magic.Magic":
    cookie = magic.Magic(magic_file=str(magic_db) if magic_db is not None else None, **({facet_flag: True} if facet_flag else {}), **flagged)
    # Exemption: libmagic exposes cookie policy only through mutating setters and raw flag replacement. Every
    # mutation lives INSIDE this cached constructor and the cache is worker-process-local — `_gated_detect`
    # runs only inside the HOSTILE process worker, one job per worker — so a cooked cookie is never mutated after
    # construction nor shared across threads, and the per-key memo spares a magic-database reload per call.
    for param, value in params.items():
        cookie.setparam(getattr(magic, param.value), value)
    if no_check:  # MAGIC_NO_CHECK_* is raw-bit-only; classes OR into the flags via `magic_setflags`
        magic.magic_setflags(cookie.cookie, cookie.flags | reduce(or_, (getattr(magic, klass.value) for klass in no_check), 0))
    return cookie


def _cooked(source: Source, facet: MagicFacet, policy: DetectPolicy, flagged: frozendict[str, bool], /) -> str:
    cookie = _cookie(policy.magic_db, _FACET_FLAG.try_find(facet).default_value(None), flagged, policy.params, policy.no_check)
    match source:
        case Source(tag="buffer", buffer=(payload, _)):
            return cookie.from_buffer(payload)
        case Source(tag="file", file=(path, _)):
            return cookie.from_file(path)
        case _ as unreachable:
            assert_never(unreachable)


def _gated_detect(source: Source, profile: DetectProfile, policy: DetectPolicy, /) -> DetectIdentity:
    try:
        version = magic.version()
    except NotImplementedError:  # ancient libmagic lacks magic_version — detection proceeds without the extension hint
        version = 0
    flagged: frozendict[str, bool] = frozendict({flag.value: True for flag in policy.flags})
    facets = tuple(f for f in _PROFILE_FACETS[profile] if f is not MagicFacet.EXTENSION or version >= _EXTENSION_MIN)
    cooked: frozendict[MagicFacet, str] = frozendict({f: _cooked(source, f, policy, flagged) for f in facets})
    strongest = lambda raw: raw.split(_CONTINUE_SEP, 1)[0]
    mime = strongest(cooked.get(MagicFacet.MIME, ""))
    primary = cooked.get(MagicFacet.DESCRIPTION) or cooked.get(MagicFacet.MIME) or ""
    media_class, container = MediaClass.of(mime), Container.of(mime)
    matches = tuple(primary.split(_CONTINUE_SEP)) if DetectFlag.KEEP_GOING in policy.flags else ()
    extensions = tuple(e for e in strongest(cooked.get(MagicFacet.EXTENSION, "")).split("/") if e and e != "???")
    claimed = source.claimed
    return DetectIdentity(
        mime=mime,
        description=strongest(cooked.get(MagicFacet.DESCRIPTION, "")),
        encoding=strongest(cooked.get(MagicFacet.ENCODING, "")),
        extensions=extensions,
        media_class=media_class,
        container=container,
        matches=matches,
        claimed=claimed,
        # libmagic carries no confidence; it clears the floor and trusts its single match
        trust=_trust(mime, media_class, container, extensions, matches, claimed, 1.0),
        confidence=1.0,
        engine=DetectEngine.LIBMAGIC,
        byte_length=source.length,
        libmagic_version=version,
    )
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
