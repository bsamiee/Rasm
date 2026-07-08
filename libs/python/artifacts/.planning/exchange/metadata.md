# [PY_ARTIFACTS_METADATA]

The descriptive-metadata read/write owner at the exchange boundary. `Metadata` is ONE owner binding and recovering the EXIF / IPTC / XMP / ICC descriptive-metadata standards — title, creator, copyright, keyword, comment, rating, camera, exposure, GPS, rights, the full ICC profile header (manufacturer / model / copyright / rendering intent), the xmpMM asset identity/lineage, the raster container structure (dimensions / MIME / embedded thumbnail), and the media container structure (chapters / tracks / duration / bit rate) — on an already-emitted raster, PDF, or media artifact, discriminating the `Metadata` `read`/`write` verb over its `(MetaCarrier, payload)` shape — never a per-standard reader family, never a per-tag `get_author`/`set_title` accessor, never a per-format reader type, and never a carrier×direction case explosion: the carrier rides as the verb payload's first field, not a tag multiplier, so the op is two cases over three carriers, not six. The EXIF/IPTC/XMP/ICC standards are field-namespace facets every carrier read folds into the ONE `MetaFacts` in a single pass through one `MetaFacts.from_logical` materialization keyed by the one `_FIELD_KEYS` logical→standard correspondence — never a dispatch axis that fragments one raster into four separate provider reads and never four parallel per-standard key tables. Each carrier rides its admitted owner at the runtime placement its package dictates: `pikepdf` (`worker-native`, ungated) and `av` (`cp311-native`, core) resolve in-process folded onto the `_THREAD_GATE` `CapacityLimiter`-bounded the runtime thread lane (the explicit thread band, never the per-loop 40-token default) so the native qpdf/FFmpeg call never blocks the loop, while the raster cluster crosses the `execution/lanes#LANE`-owned `WORKER_BAND` `CapacityLimiter`-bounded `anyio.to_process.run_sync, limiter=WORKER_BAND)` value the `CarrierPolicy` row carries, never the unbounded per-loop default process limiter. Every operation returns one `(ContentKey, MetaFacts, bytes)` evidence triple — the recovered (read) or bound (write) `MetaFacts`, the source or re-encoded payload, and the runtime `ContentIdentity.of` content key — that the consumer projects onto the `core/receipt#RECEIPT` `ArtifactReceipt.Metadata(key, carrier, fields, byte_len)` flat-scalar fact exactly as `exchange/credential#PROVENANCE` and `exchange/conformance#CONFORMANCE` return their `(ContentKey, evidence)` pair: a metadata READ recovers the full descriptive field set and a WRITE the re-encoded artifact, both richer than the four-scalar receipt, so the owner returns the evidence and the consumer mints the flat case — never a write-only `@receipted` weave that discards the read's recovered fields and the write's bytes, never a re-minted identity, and never a producer value object crossing into the receipt owner.

## [01]-[INDEX]

- [01]-[METADATA]: the one descriptive-metadata owner — a two-case `read`/`write` `expression.from_logical` materialization; the `MetaBind` (`MERGE`/`REPLACE`/`STRIP`) write disposition; the `@runtime retry retry` weave over the `exiftool -stay_open` spawn transient; and the `@beartype(conf=FAULT_CONF)` ingress contract sibling exchange owners carry.
- [02]-[METADATA_EVIDENCE]: the `(ContentKey, MetaFacts, bytes)` evidence triple projects onto the `core/receipt#RECEIPT` `ArtifactReceipt.Metadata` case the ARCHITECTURE `[02]-[SEAMS]` `exchange/metadata → core/receipt` edge names.
- [03]-[METADATA_PROVENANCE]: `Iptc4xmpExt:DigitalSourceType` is the UNSIGNED descriptive AI-provenance label, distinct from the SIGNED `exchange/credential#CREDENTIAL` C2PA `DigitalSource` assertion. The abandoned `exif` package is replaced by `pyexiftool`.

## [02]-[METADATA]

- Owner: `Metadata` IS the one descriptive-metadata owner — an `expression.tagged_union` of exactly TWO cases discriminating the read/write verb directly (mirroring `exchange/credential#PROVENANCE`, never a one-field wrapper over a separate op union): `read: tuple[MetaCarrier, bytes]` and `write: tuple[MetaCarrier, bytes, WriteSpec]` (the write instruction `facts`+`bind` bundled in one named `WriteSpec` owner per the `exchange/credential#PROVENANCE`/`exchange/conformance#CONFORMANCE` `(bytes, Spec)` payload convention, never a naked positional `tuple[..., MetaFacts, MetaBind]` decoded by index) — the carrier riding as each payload's leading field and the direction the tag, so a new carrier adds zero cases (the carrier×direction product collapses to verb×payload); `MetaCarrier` the closed `StrEnum` axis (`RASTER`/`PDF`/`MEDIA`) whose `_CARRIER` row selects the acceptor-and-lane; `MetaBind` the closed write disposition (`MERGE`/`REPLACE`/`STRIP`); `MetaFacts` the ONE composite value owner every read materializes and every write consumes — eight nested frozen facets (`Descriptive` editorial + media collection tags + the JPEG COM `comment`, `Rights` creator/usage, `Capture` camera/exposure + the IPTC `Iptc4xmpExt:DigitalSourceType` content-origin/AI-provenance label, `Place` location, `History` xmpMM asset identity/lineage, `Color` the full ICC profile header, `RasterInfo` the raster container dimensions/MIME + embedded thumbnail, `MediaInfo` container chapters/tracks/duration/bit-rate) plus the `conformance` PDF/A·PDF/X status, recovering the full descriptive field set across all four standards plus the container-structure read in one shape and projected to the `core/receipt#RECEIPT` facts. The writable XMP/EXIF/IPTC facets fold through `_flat`; the read-only `Color` ICC header, `RasterInfo` raster container structure, and `MediaInfo` media container structure are evidence-only siblings of `conformance` the writers never re-emit. `CarrierPolicy` is the carrier collapse: one frozen row carries the `(reader, writer, lane)` triple a `MetaCarrier` keys, so the operation routes by one `_CARRIER` lookup over `self.carrier`, never a per-standard reader/writer class family, never a re-discriminating `match` inside an arm, and never a `gated: bool` knob the body re-pairs to a lane the value already selects.get_tags`/`set_tags` `-stay_open` subprocess pass over a temp file covering EXIF+IPTC+XMP+ICC+GPS+maker-notes cross-format) — ONE `CarrierPolicy` either way, both crossing the `WORKER_BAND` process band, never two parallel `RASTER` carriers.
- Cases: `Metadata` cases — `read(carrier, payload)` and `write(carrier, payload, WriteSpec(facts, bind))` — matched by one total `match`/`case` over `self` in `_run`, the read arm collapsing to one acceptor-lookup arm and the write arm to one (the `WriteSpec` unpacked once to `spec.facts`/`spec.bind`), never a parallel reader/writer per carrier or per standard. Each carrier's reader folds its native namespace into the one `MetaFacts`: RASTER reads EXIF + IPTC + XMP + ICC + GPS + maker-notes in ONE cross-format pass — the `pyexiftool` arm through `ExifToolHelper.get_tags(path, _EXIFTOOL_TAGS)` returning `-G`-grouped `"<group>:<tag>"` JSON (`EXIF:`, `IPTC:`, `XMP-*:`, `ICC_Profile:`, `Composite:GPS*`) folded through `_FIELD_KEYS[logical].exiv2` plus `read_icc()` bytes through `pillow` `ImageCms`; PDF reads the FULL Dublin-Core/XMP-Basic/XMP-MM/Adobe-PDF/Photoshop/xmpRights namespace (the `pdf:Producer` converting-application fact and the `xmpMM:DocumentID`/`InstanceID` identity included) through the `pikepdf` `PdfMetadata` mapping plus `pdfa_status`/`pdfx_status`; MEDIA reads the FFmpeg container tag set through `av` `Container.metadata` plus the container structure — `chapters()` navigation, per-stream `streams` labelling, and `duration` — folded into the `MediaInfo` facet. Every reader emits one `dict[str, object]` keyed by logical field name and folds it through the one `MetaFacts.from_logical`, so a carrier read is one logical projection plus one materialization, never a hand-built per-facet constructor per carrier.
- Auto: `_run` folds `self` through one total `match` whose two arms (read, write) lift the carrier's `CarrierPolicy` row once, so the per-carrier body is data, never a six-arm enumeration. The `pyexiftool` raster reader (`@the runtime `RetryClass.OCCT` retry)` over the `-stay_open` spawn transient) spills `payload` to a `NamedTemporaryFile(delete_on_close=False)`, opens one `ExifToolHelper(executable=_EXIFTOOL, common_args=["-G", "-n"])` context, reads the `_EXIFTOOL_TAGS` set once, projects every logical through `_FIELD_KEYS[logical].exiftool`, reads the signed `Composite:GPSLatitude`/`Composite:GPSLongitude` decimals directly (the `-n` numeric output — no DMS fold), splits the `IPTC:Keywords`/`XMP-dc:Subject` list through `_as_tuple`, and resolves the ICC header off the `ICC_Profile:` group tags directly (no `pillow` parse — exiftool decodes the header).exiv2`, evaluates EXIF `num/den` rationals through `_num` for the `_RATIONAL` float logicals, folds the `Exif.GPSInfo.GPSLatitude` DMS rational triple with its `GPSLatitudeRef` through `_dms` into the signed `Place.gps` pair, and resolves the full ICC header through `_icc` (one `ImageCms.ImageCmsProfile` parse over the `read_icc()` bytes railed through `expression.extra.result.catch`, each `getProfile*` accessor and the `getDefaultIntent` ordinal each railed through `catch` so one missing tag never sinks the rest). Both raster readers fold all four standards into the one `MetaFacts.from_logical`. The raster writer clears the DESCRIPTIVE namespace on `REPLACE`/`STRIP` (`-all=` group-wildcard scrub via `ExifToolHelper.exiftool`/`.exiv2` (the keyword tuple bound as a `list` for the repeated `-IPTC:Keywords`/`Iptc.Application2.Keywords` directives), and returns the metadata-only re-encoded bytes (`Path(tmp).read_bytes()` after `set_tags`, or `ImageData.get_bytes()`) — a container-metadata mutation WITHOUT a pixel re-encode, higher-fidelity than a raster re-encode, so no `_rational` re-encode is needed (the provider owns the IFD-type round-trip). The PDF arms open one `PdfMetadata` context, the read folding every `_FIELD_KEYS` xmp qname present in the mapping plus `pdfa_status`/`pdfx_status`, the write deleting-then-assigning under the bind policy; the MEDIA arms open the `av` container, the read folding every `_FIELD_KEYS` media tag present in `Container.metadata` AND the `_media_info` container-structure read (the `Container.duration` seconds over `_AV_TIME_BASE`, the `chapters()` markers projected to `Chapter(start, title)` rows, and the per-`streams` `Track(kind, language, title)` labelling) folded into the `MediaInfo` facet while the container is still open, the write a `demux` → `add_stream_from_template` → `mux` bitstream-copy remux that rebinds `OutputContainer.metadata` without re-encoding. `_flat` is the one facts→logical projection every writer derives its bindings from (`structs.asdict` over the writable facets, dropping empties and tuples), and `from_logical` is its inverse (one `msgspec.convert(strict=False)` per facet over the logical dict, coercing the standard string values to each facet field's type), so a field reaches every provider from one declared `_FIELD_KEYS` correspondence rather than a per-standard getter/setter pair.
- Receipt: each operation folds into `MetaFacts` and returns the `(ContentKey, MetaFacts, bytes)` triple, and the consumer projects the settled `core/receipt#RECEIPT` `ArtifactReceipt.Metadata(key, carrier, fields, byte_len)` case the ARCHITECTURE `[02]-[SEAMS]` `exchange/metadata → python:artifacts/core/receipt` edge names — the carrier the `MetaCarrier` value off the op the consumer admitted, the field count the `MetaFacts.populated` tally (the recovered/bound flat scalars plus the keyword set, GPS pair, ICC-header colour evidence, the raster dimensions/MIME/thumbnail evidence, and the media chapter/track/bit-rate counts), the byte length the returned payload — exactly as the coordinator mints `ArtifactReceipt.Credential` from the `exchange/credential#PROVENANCE` pair and `ArtifactReceipt.Verdict` from the `exchange/conformance#CONFORMANCE` pair. The write arms key the re-encoded bytes through `ContentIdentity.of` so a metadata-bound artifact carries a fresh content key the `csharp:Rasm.Persistence` store re-derives, and the read arms key the source bytes unchanged. The `core/receipt#RECEIPT` union carries the flat-scalar `Metadata` case (`metadata: tuple[ContentKey, str, int, int]`, its second slot the `carrier` discriminant) mirroring the flat-scalar `Credential`/`Media` cases, so the consumer spreads the carrier/field-count/byte-length slice onto it and the receipt owner imports no `MetaFacts` value object — the rich `MetaFacts` rides the returned triple to the reading caller, the flat case lives on the receipt page, neither minted here.
- Growth: a new metadata carrier is one `MetaCarrier` member plus one `_CARRIER` `CarrierPolicy(reader, writer, lane)` row plus its two carrier functions — zero new op cases, because the carrier is the payload field, not a tag; a new descriptive field is one field on the owning `MetaFacts` facet plus one `_FIELD_KEYS` row, the `_flat`/`from_logical` derivation reaching every provider with no per-provider table edit; a new provider spelling a carrier supports is one more column on the `FieldKeys` row plus one fold inside that carrier's reader/writer; a new write disposition is one `MetaBind` member plus one arm in the writers; a new writable descriptive facet (the `History` xmpMM identity row is exactly this) is one nested frozen `Struct` on `MetaFacts` plus one `from_logical` convert leg plus its rows in `_flat`'s facet tuple; a new read-only structured facet (the `MediaInfo` chapters/tracks and the `RasterInfo` dimensions/MIME/thumbnail reads are exactly this) is one nested frozen `Struct` plus its reader population plus one `from_logical` keyword and one `populated` term, never a `_flat` write leg; a new ICC-header fact is one `Color` field plus one `getProfile*`/`ICC_Profile:` tag read; a new binary-path or worker knob is one `MetaSettings` field; zero new surface.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
import shutil
from collections.abc import Awaitable, Callable, Mapping
from dataclasses import dataclass
from enum import StrEnum
from functools import partial
from importlib.util import find_spec
from io import BytesIO
from pathlib import Path
from tempfile import NamedTemporaryFile
from typing import Final, Literal, Self, assert_never

from beartype import beartype
from expression import case, tag, tagged_union
from expression.collections import Map
from expression.extra.result import catch
from msgspec import Struct, convert, structs
from msgspec import json as msgjson
from pydantic_settings import BaseSettings, SettingsConfigDict

from rasm.runtime.identity import CANONICAL_POLICY, ContentIdentity, ContentKey
from rasm.runtime.lanes import LanePolicy, Modality
from rasm.runtime.resilience import RetryClass
from rasm.runtime.faults import FAULT_CONF, RuntimeRail, async_boundary
from rasm.runtime.lanes import WORKER_BAND

lazy from exiftool import ExifToolHelper  # exiftool -stay_open cross-format driver, to_process worker band
lazy from exiftool.exceptions import ExifToolVersionError  # typed spawn/version fault the retry weave re-execs on
lazy from PIL import ImageCms  # pillow ICC-header reader over recovered ICC bytes
lazy import pikepdf  # qpdf PDF metadata, to_thread in-process arm
lazy import av  # FFmpeg container read/remux, to_thread in-process arm

# --- [TYPES] ----------------------------------------------------------------------------
type MetaReader = Callable[[bytes], "MetaFacts"]
type MetaWriter = Callable[[bytes, "MetaFacts", "MetaBind"], bytes]
type Offload = Callable[..., Awaitable[object]]


class MetaCarrier(StrEnum):
    RASTER = "raster"
    PDF = "pdf"
    MEDIA = "media"


class MetaBind(StrEnum):
    MERGE = "merge"  # set provided fields, keep the rest
    REPLACE = "replace"  # clear the descriptive namespace (preserving the ICC profile), then set provided
    STRIP = "strip"  # clear the descriptive namespace AND the ICC profile, set nothing


# --- [CONSTANTS] ------------------------------------------------------------------------
# float logicals whose Exiv2 EXIF value is a `num/den` rational `_num` normalizes before materialization;
# the pyexiftool `-n` numeric output already emits these as plain decimals, so `_RATIONAL` applies only to the
# every other numeric field is a plain numeral `msgspec.convert(strict=False)` coerces directly.
_RATIONAL: Final[frozenset[str]] = frozenset({"f_number", "focal_length", "aperture", "exposure_bias", "altitude", "gps_direction"})

# ICC default-rendering-intent ordinal -> token (the liblcms2/`ImageCms.
_INTENT_NAME: Final[Map[int, str]] = Map.of_seq([(0, "perceptual"), (1, "relative"), (2, "saturation"), (3, "absolute")])
_AV_TIME_BASE: Final[int] = 1_000_000  # `Container.duration` is AV_TIME_BASE microseconds; divide for seconds


# --- [MODELS] ---------------------------------------------------------------------------
class FieldKeys(Struct, frozen=True, gc=False):  # the per-logical provider correspondence row
    xmp: str = ""  # XMP qname for the pikepdf PDF carrier (`dc:title`, `xmpMM:DocumentID`, `pdf:Producer`)
    exiftool: str = ""  # exiftool `-G` grouped tag for the RASTER pyexiftool arm (`EXIF:Make`, `IPTC:Keywords`, `ICC_Profile:ProfileDescription`)
    media: str = ""  # FFmpeg container tag for the av MEDIA carrier


class Descriptive(Struct, frozen=True):  # editorial: dc:* / xmp:* / photoshop:* / Iptc4xmpCore:* + FFmpeg container tags
    title: str = ""
    headline: str = ""
    caption: str = ""
    keywords: tuple[str, ...] = ()
    rating: int = 0
    label: str = ""
    category: str = ""
    genre: str = ""
    language: str = ""
    instructions: str = ""
    description_writer: str = ""
    comment: str = ""  # JPEG COM segment — exiftool `File:Comment`; raster-only (no XMP/media twin)
    album: str = ""  # FFmpeg `album` container tag — the descriptive collection an audio/video asset belongs to
    album_artist: str = ""  # FFmpeg `album_artist` container tag
    composer: str = ""  # FFmpeg `composer` container tag


class Rights(Struct, frozen=True):  # creator + rights: IPTC by-line/credit / xmpRights / dc:rights+publisher
    creator: str = ""
    credit: str = ""
    source: str = ""
    copyright: str = ""
    usage_terms: str = ""
    web_statement: str = ""
    publisher: str = ""
    marked: bool | None = None


class Capture(Struct, frozen=True):  # camera/exposure: EXIF IFD0/Photo + xmp:Create/ModifyDate
    make: str = ""
    model: str = ""
    lens: str = ""
    lens_make: str = ""
    software: str = ""  # xmp:CreatorTool — the application that authored the content
    producer: str = ""  # pdf:Producer — the application that converted/emitted the PDF (distinct from CreatorTool)
    digital_source_type: str = ""  # Iptc4xmpExt:DigitalSourceType — the IPTC content-origin IRI (digitalCapture/algorithmicMedia/trainedAlgorithmicMedia/composite); the plain-XMP AI-provenance label, distinct from the signed exchange/credential C2PA assertion
    orientation: int = 0
    exposure_time: str = ""
    f_number: float = 0.0
    aperture: float = 0.0
    iso: int = 0
    focal_length: float = 0.0
    exposure_bias: float = 0.0
    exposure_program: str = ""
    metering_mode: str = ""
    flash: str = ""
    white_balance: str = ""
    serial_number: str = ""
    created: str = ""
    modified: str = ""


class Place(Struct, frozen=True):  # location: EXIF GPS + photoshop/Iptc4xmpCore
    gps: tuple[float, float] | None = (
        None  # signed decimal degrees — pyexiftool reads `Composite:GPS*` directly; `_dms` folds a DMS array where a carrier hands one back
    )
    altitude: float | None = None
    gps_direction: float | None = None
    gps_timestamp: str = ""
    map_datum: str = ""
    city: str = ""
    state: str = ""
    country: str = ""
    country_code: str = ""
    sublocation: str = ""


class History(Struct, frozen=True):  # XMP Media Management (xmpMM) asset identity + lineage — the provenance co-identity the credential rail reads
    document_id: str = ""  # xmpMM:DocumentID — the stable cross-rendition asset id
    instance_id: str = ""  # xmpMM:InstanceID — this specific rendition's id
    original_id: str = ""  # xmpMM:OriginalDocumentID — the ancestor this asset derives from


class Color(Struct, frozen=True):  # colour evidence: colour space + the ICC profile header (read-only)
    space: str = ""
    icc_name: str = ""
    icc_present: bool = False
    render_intent: str = ""  # ICC default rendering intent (perceptual/relative/saturation/absolute)
    icc_maker: str = ""  # ICC profile manufacturer tag
    icc_model: str = ""  # ICC profile model tag (distinct from camera `Capture.model`)
    icc_copyright: str = ""  # ICC profile copyright tag (distinct from asset `Rights.copyright`)


class RasterInfo(
    Struct, frozen=True
):  # raster container structure read-only (exiftool `File:*` structure rows) — the raster-carrier sibling of `Color`/`MediaInfo`
    width: int = 0  # exiftool `File:ImageWidth`
    height: int = 0  # exiftool `File:ImageHeight`
    mime: str = ""  # exiftool `File:MIMEType` — the container's sniffed MIME
    thumbnail: bytes = b""  # exiftool `-b -ThumbnailImage` — the embedded EXIF preview, a DAM/forensic band mirroring credential's `resources`


class Chapter(Struct, frozen=True, gc=False):  # one media navigation chapter — the descriptive timeline marker `av` `Container.chapters()` yields
    start: float = 0.0  # chapter start in seconds (the `start * time_base` fold over the `Chapter` TypedDict)
    title: str = ""


class Track(
    Struct, frozen=True, gc=False
):  # one container stream's descriptive labelling (NOT its codec/encode params — those are media/container's)
    kind: str = ""  # `Stream.type` — video/audio/subtitle/data/attachment
    language: str = ""  # `Stream.language`
    title: str = ""  # `Stream.metadata["title"]`


class MediaInfo(
    Struct, frozen=True
):  # media container structure read-only (av `streams`/`chapters()`/`duration`/`bit_rate`) — the media-carrier sibling of `Color`/`conformance`
    duration: float = 0.0
    bit_rate: int = 0  # `InputContainer.bit_rate` — overall container bit rate (bits/sec), a descriptive container-structure fact folded while the container is open
    chapters: tuple[Chapter, ...] = ()
    tracks: tuple[Track, ...] = ()


class MetaFacts(Struct, frozen=True):
    descriptive: Descriptive = Descriptive()
    rights: Rights = Rights()
    capture: Capture = Capture()
    place: Place = Place()
    history: History = History()  # xmpMM identity/lineage — a writable XMP facet, folded by `_flat`
    color: Color = Color()  # ICC header evidence — read-only, excluded from `_flat`
    raster: RasterInfo = RasterInfo()  # raster container structure — read-only, populated only by the RASTER readers
    media: MediaInfo = MediaInfo()  # media container structure — read-only, populated only by `_read_media`
    conformance: str = ""  # pikepdf pdfa_status / pdfx_status

    @classmethod
    def from_logical(cls, flat: Mapping[str, object], /, *, media: MediaInfo = MediaInfo(), raster: RasterInfo = RasterInfo()) -> Self:
        # one logical dict -> the flat facets via per-facet `convert(strict=False)`; unknown keys fall
        # away per facet, the standard string values coerce to each field's type, `marked` lifts to bool.
        # `media`/`raster` ride in as already-materialized structured facets (their fields carry binary/derived
        # values — thumbnail bytes, chapter rows — that are not logical string keys the per-facet convert reaches).
        data = dict(flat)
        if isinstance(mark := data.get("marked"), str):
            data["marked"] = mark.strip().lower() in ("true", "1", "yes")
        return cls(
            descriptive=convert(data, Descriptive, strict=False),
            rights=convert(data, Rights, strict=False),
            capture=convert(data, Capture, strict=False),
            place=convert(data, Place, strict=False),
            history=convert(data, History, strict=False),
            color=convert(data, Color, strict=False),
            raster=raster,
            media=media,
            conformance=str(data.get("conformance", "")),
        )

    @property
    def populated(self) -> int:
        # flat scalar tally + the non-scalar/read-only evidence each facet carries off the `_flat` path
        color = sum(1 for value in structs.asdict(self.color).values() if value)
        raster = bool(self.raster.width) + bool(self.raster.mime) + bool(self.raster.thumbnail)
        return (
            len(_flat(self))
            + bool(self.conformance)
            + bool(self.descriptive.keywords)
            + bool(self.place.gps)
            + color
            + raster
            + len(self.media.chapters)
            + len(self.media.tracks)
            + bool(self.media.duration)
            + bool(self.media.bit_rate)
        )


@dataclass(frozen=True, slots=True, kw_only=True)
class CarrierPolicy:  # the `(reader, writer, lane)` triple one `MetaCarrier` keys, the carrier collapse
    reader: MetaReader
    writer: MetaWriter
    lane: Offload


# the typed write instruction the `write` case carries beside its carrier+artifact, mirroring the
# `exchange/credential#PROVENANCE`/`exchange/conformance#CONFORMANCE` `(bytes, Spec)` payload convention:
# `facts`+`bind` ride one named owner (the growth site a new write knob lands on) rather than a naked
# positional `tuple[..., MetaFacts, MetaBind]` decoded by index.
class WriteSpec(Struct, frozen=True):
    facts: MetaFacts
    bind: MetaBind = MetaBind.MERGE


# --- [SERVICES] -------------------------------------------------------------------------
class MetaSettings(BaseSettings):
    # the brief [03] system-tool subprocess boundary: discovery-env (`RASM_META_EXIFTOOL`) -> configured path ->
    # `shutil.which` fallback, resolved ONCE at module scope, never a hardcoded literal and never a per-call re-read.
    model_config = SettingsConfigDict(env_prefix="RASM_META_", frozen=True, extra="forbid")
    exiftool: str | None = None


# `Metadata` IS the closed union: the two read/write cases, the carrier-polymorphic mints, the carrier
# property, the async entry, and the dispatch all fold onto one `tagged_union` (mirroring
# `exchange/credential#PROVENANCE`), never a one-field `Metadata` wrapper over a separate `MetaOp`.
@tagged_union(frozen=True)
class Metadata:
    tag: Literal["read", "write"] = tag()
    read: tuple[MetaCarrier, bytes] = case()
    write: tuple[MetaCarrier, bytes, WriteSpec] = case()

    @classmethod
    @beartype(conf=FAULT_CONF)
    def Read(cls, carrier: MetaCarrier, payload: bytes, /) -> Self:
        return cls(read=(carrier, payload))

    @classmethod
    @beartype(conf=FAULT_CONF)
    def Write(cls, carrier: MetaCarrier, payload: bytes, facts: MetaFacts, bind: MetaBind = MetaBind.MERGE, /) -> Self:
        return cls(write=(carrier, payload, WriteSpec(facts=facts, bind=bind)))

    @property
    def carrier(self) -> MetaCarrier:
        match self:
            case Metadata(tag="read", read=(carrier, *_)) | Metadata(tag="write", write=(carrier, *_)):
                return carrier
            case _ as unreachable:
                assert_never(unreachable)

    def emit(self, /) -> ArtifactWork:
        return ArtifactWork(key=self._key, work=self._emit, parents=(), admission=Admission(keyed=None), cost=1.0)

    @property
    def _key(self) -> ContentKey:
        # key-over-INPUT: canonical (carrier ⊕ op ⊕ source) minted PRE-RUN — the WRITE-side re-encoded
        # carrier bytes are content-addressed on the receipt FACTS, never flattened or carried whole.
        return ContentIdentity.of(f"metadata-{self.carrier.value}-{self.tag}", self, policy=CANONICAL_POLICY)

    async def _emit(self) -> RuntimeRail[ArtifactReceipt]:
        # the triple collapses per the one contract: facts to the Metadata band, the write-side bytes an
        # addressed evidence fact; callers read facts off the receipt (receipt.slot == node.key).
        return (await async_boundary(f"metadata.{self.carrier.value}.{self.tag}", self._folded)).map(
            lambda pf: ArtifactReceipt.Metadata(self._key, self.carrier.value, pf[1].populated, len(pf[0]))
        )

    async def _folded(self) -> tuple[bytes, MetaFacts]:
        return await self._run()

    async def _run(self) -> tuple[bytes, MetaFacts]:
        policy = _CARRIER[self.carrier]
        match self:
            case Metadata(tag="read", read=(_carrier, payload)):
                return payload, await policy.lane(policy.reader, payload)
            case Metadata(tag="write", write=(_carrier, payload, spec)):
                return await policy.lane(policy.writer, payload, spec.facts, spec.bind), spec.facts
            case _ as unreachable:
                assert_never(unreachable)


# --- [OPERATIONS] -----------------------------------------------------------------------
def _flat(facts: MetaFacts) -> dict[str, str]:
    # the one facts->logical projection over the WRITABLE facets; `structs.asdict` dropping empties
    # and tuples (keywords ride their own writer arm) so each writer derives bindings from one map.
    # read-only evidence (`color` ICC header, `media` structure, `conformance` PDF/A status) never folds here.
    fields: dict[str, object] = {}
    for facet in (facts.descriptive, facts.rights, facts.capture, facts.place, facts.history):
        fields.update(structs.asdict(facet))
    out: dict[str, str] = {}
    for logical, value in fields.items():
        match value:
            case str() as text if text:
                out[logical] = text
            case bool() as flag:  # `bool` before `int` — `marked` carries an explicit false
                out[logical] = str(flag)
            case int() | float() as num if num:
                out[logical] = str(num)
    return out


def _scalar(value: object, /) -> str:
    # an Exiv2/exiftool multi-value tag arrives as a list (Bag/Seq/repeatable) or a LangAlt dict; reduce it
    # to the joined/default-language scalar a scalar `MetaFacts` field carries, never a raw provider container.
    match value:
        case list() as items:
            return "; ".join(str(item).strip() for item in items if str(item).strip())
        case dict() as langalt:
            return str(next(iter(langalt.values()), ""))
        case _:
            return str(value)


def _as_tuple(value: object, /) -> tuple[str, ...]:
    # the keyword projection: a provider list rides straight through, a delimited string splits on `;`/`,`.
    match value:
        case list() as items:
            return tuple(str(item).strip() for item in items if str(item).strip())
        case str() as text:
            return tuple(part.strip() for part in text.replace(",", ";").split(";") if part.strip())
        case None:
            return ()
        case _:
            return (str(value),)


def _num(value: str, /) -> float:
    # an Exiv2 EXIF rational is `num/den`; evaluate it (numerator over denominator), never the numerator
    # alone (`28/10` is f/2.8, not 28). A bare decimal carries no slash and returns directly.
    num, slash, den = value.partition("/")
    keep = lambda text: "".join(ch for ch in text if ch.isdigit() or ch in ".-")
    top, bottom = keep(num), keep(den)
    if slash and top.strip(".-") and bottom.strip(".-") and (divisor := float(bottom)):
        return float(top) / divisor  # a `0/0` GPS-unknown rational falls through to 0.0, never raising ZeroDivisionError
    return float(top) if top.strip(".-") else 0.0


def _dms(value: str, ref: str, /) -> float | None:
    # the EXIF GPS fold: a degree/minute/second rational triple plus an N/S/E/W hemisphere ref
    # to signed decimal degrees. The pyexiftool arm reads `Composite:GPS*` signed decimals directly.
    parts = [_num(token) for token in value.replace(",", " ").split() if token]
    if not parts:
        return None
    degrees = sum(part / 60**index for index, part in enumerate(parts[:3]))
    return -degrees if ref.strip().upper().startswith(("S", "W")) else degrees  # N/S/E/W or North/South/...


def _icc(blob: bytes) -> dict[str, object]:
    # the ICC header read over recovered ICC bytes: description + manufacturer/model/copyright
    # provenance + default rendering intent, railed through `catch` so a malformed profile or missing tag skips
    # by omission rather than faulting the read. The pyexiftool arm reads these off the `ICC_Profile:` group instead.
    if not blob:
        return {}
    # best-effort ICC parse: a malformed profile skips by omission (the `boundaries.md` plugin-load `catch(exception=Exception)`
    # disposition), never sinking the rest of the metadata read; the precise `PyCMSError` narrows each header accessor below.
    return catch(exception=Exception)(ImageCms.ImageCmsProfile)(BytesIO(blob)).map(_icc_header).default_value({})


def _icc_header(profile: "ImageCms.ImageCmsProfile", /) -> dict[str, object]:
    read = lambda reader: catch(exception=ImageCms.PyCMSError)(reader)(profile).map(lambda value: str(value).strip()).default_value("")
    intent = catch(exception=ImageCms.PyCMSError)(ImageCms.getDefaultIntent)(profile).default_value(-1)
    return {
        "icc_name": read(ImageCms.getProfileDescription),
        "icc_maker": read(ImageCms.getProfileManufacturer),
        "icc_model": read(ImageCms.getProfileModel),
        "icc_copyright": read(ImageCms.getProfileCopyright),
        "render_intent": _INTENT_NAME.try_find(intent).default_value(""),
    }


# --- [RASTER_CARRIER] -------------------------------------------------------------------
# The RASTER cluster crosses the `WORKER_BAND` `to_process` band. The categorical-best cross-format provider
# folds EXIF + IPTC + XMP + ICC + GPS + maker-notes in ONE pass, replacing the abandoned `exif` JPEG-EXIF-only arm
# and superseding the former per-standard provider split.
def _exiftool_read(payload: bytes) -> MetaFacts:
    with NamedTemporaryFile(suffix=".img", delete_on_close=False) as tmp:  # Exemption: exiftool reads a real path, not a stream
        tmp.write(payload)
        tmp.close()
        with ExifToolHelper(executable=_EXIFTOOL, common_args=["-G", "-n"]) as et:
            et.set_json_loads(msgjson.decode)  # msgspec over the stdlib json parser on the get_tags decode path (catalog-cited faster parse)
            rows = et.get_tags(tmp.name, list(_EXIFTOOL_TAGS))
            thumb = et.execute("-b", "-ThumbnailImage", tmp.name, raw_bytes=True)  # undecoded embedded EXIF preview bytes
        grouped = rows[0] if rows else {}
    flat: dict[str, object] = {logical: grouped[keys.exiftool] for logical, keys in _FIELD_KEYS.items() if keys.exiftool and keys.exiftool in grouped}
    flat["keywords"] = _as_tuple(grouped.get("IPTC:Keywords") or grouped.get("XMP-dc:Subject"))
    lat, lon = grouped.get("Composite:GPSLatitude"), grouped.get("Composite:GPSLongitude")  # `-n` -> signed decimals, no DMS fold
    flat["gps"] = (float(lat), float(lon)) if lat is not None and lon is not None else None
    flat["icc_present"] = any(key.startswith("ICC_Profile:") for key in grouped)  # the header fields ride `_FIELD_KEYS` off the same JSON
    raster = RasterInfo(
        width=int(grouped.get("File:ImageWidth") or 0),
        height=int(grouped.get("File:ImageHeight") or 0),
        mime=str(grouped.get("File:MIMEType", "")),
        thumbnail=thumb if isinstance(thumb, bytes) else b"",
    )
    return MetaFacts.from_logical(flat, raster=raster)


def _exiftool_write(payload: bytes, facts: MetaFacts, bind: MetaBind) -> bytes:
    with NamedTemporaryFile(suffix=".img", delete_on_close=False) as tmp:  # Exemption: exiftool mutates a real path in place
        tmp.write(payload)
        tmp.close()
        with ExifToolHelper(executable=_EXIFTOOL, common_args=["-G", "-n"]) as et:
            if bind in (MetaBind.REPLACE, MetaBind.STRIP):
                et.execute("-all=", "-overwrite_original", tmp.name)  # group-wildcard scrub; `set_tags` rejects an empty dict
            if bind is not MetaBind.STRIP:
                tags: dict[str, str | list[str]] = {
                    keys.exiftool: value for logical, value in _flat(facts).items() if (keys := _FIELD_KEYS.try_find(logical).default_value(None)) and keys.exiftool
                }
                if facts.descriptive.keywords:  # a `list` value emits the repeated `-IPTC:Keywords=`/`-XMP-dc:Subject=` directives
                    tags["IPTC:Keywords"] = tags["XMP-dc:Subject"] = list(facts.descriptive.keywords)
                if tags:
                    et.set_tags(tmp.name, tags, params=["-overwrite_original"])
        return Path(tmp.name).read_bytes()


# --- [PDF_CARRIER] ----------------------------------------------------------------------
# pikepdf reads/writes the docinfo/XMP/conformance namespace in-process on the `to_thread` arm.
def _read_pdf(payload: bytes) -> MetaFacts:
    with pikepdf.open(BytesIO(payload)) as pdf, pdf.open_metadata(set_pikepdf_as_editor=False, update_docinfo=True) as meta:
        join = lambda value: "; ".join(value) if isinstance(value, list) else str(value)
        flat: dict[str, object] = {logical: join(meta[keys.xmp]) for logical, keys in _FIELD_KEYS.items() if keys.xmp and keys.xmp in meta}
        flat["keywords"] = tuple(meta.get("dc:subject", ()))
        flat["conformance"] = meta.pdfa_status or meta.pdfx_status or ""  # both-absent coalesces to "" rather than the literal str "None"
        return MetaFacts.from_logical(flat)


def _write_pdf(payload: bytes, facts: MetaFacts, bind: MetaBind) -> bytes:
    sink = BytesIO()
    with pikepdf.open(BytesIO(payload)) as pdf:
        with pdf.open_metadata(set_pikepdf_as_editor=True, update_docinfo=True) as meta:
            if bind in (MetaBind.REPLACE, MetaBind.STRIP):
                for key in list(meta):
                    del meta[key]
            if bind is not MetaBind.STRIP:
                for logical, value in _flat(facts).items():
                    if (keys := _FIELD_KEYS.try_find(logical).default_value(None)) and keys.xmp:
                        meta[keys.xmp] = value
                if facts.descriptive.keywords:
                    meta["dc:subject"] = list(facts.descriptive.keywords)
        pdf.save(sink, deterministic_id=True)  # stable /ID so the persistence store re-derives one content key over the re-saved bytes
    return sink.getvalue()


# --- [MEDIA_CARRIER] --------------------------------------------------------------------
# av reads container tags + structure in-process on `to_thread`; write is a no-re-encode remux.
def _media_info(container: "av.container.InputContainer") -> MediaInfo:
    # the container-structure read every media descriptive consumer needs beyond the flat tag set:
    # navigation chapters (title + start) and per-stream labelling (kind/language/title) `av` exposes.
    duration = container.duration / _AV_TIME_BASE if container.duration else 0.0
    chapters = tuple(
        Chapter(start=float(ch["start"] * ch["time_base"]) if ch["time_base"] else float(ch["start"]), title=ch["metadata"].get("title", ""))
        for ch in container.chapters()
    )
    tracks = tuple(Track(kind=stream.type, language=stream.language or "", title=stream.metadata.get("title", "")) for stream in container.streams)
    return MediaInfo(duration=duration, bit_rate=container.bit_rate or 0, chapters=chapters, tracks=tracks)


def _read_media(payload: bytes) -> MetaFacts:
    with av.open(BytesIO(payload), mode="r") as container:
        tags = {key.lower(): value for key, value in container.metadata.items()}
        media = _media_info(container)  # read while the container is open — streams/chapters die with it
    flat: dict[str, object] = {logical: tags[keys.media] for logical, keys in _FIELD_KEYS.items() if keys.media and keys.media in tags}
    flat["keywords"] = tuple(k.strip() for k in tags.get("keywords", "").split(",") if k.strip())
    return MetaFacts.from_logical(flat, media=media)


def _write_media(payload: bytes, facts: MetaFacts, bind: MetaBind) -> bytes:
    sink = BytesIO()
    with av.open(BytesIO(payload), mode="r") as source, av.open(sink, mode="w", format=source.format.name) as out:
        kept = {} if bind in (MetaBind.REPLACE, MetaBind.STRIP) else dict(source.metadata)
        if bind is not MetaBind.STRIP:
            kept.update({keys.media: value for logical, value in _flat(facts).items() if (keys := _FIELD_KEYS.try_find(logical).default_value(None)) and keys.media})
            if facts.descriptive.keywords:  # `_flat` drops the tuple, so the comma-joined keyword tag the read splits is bound here
                kept["keywords"] = ",".join(facts.descriptive.keywords)
        out.metadata.update(kept)
        streams = {stream.index: out.add_stream_from_template(stream) for stream in source.streams}
        for packet in source.demux():
            if packet.dts is None:
                continue
            packet.stream = streams[packet.stream.index]
            out.mux(packet)
    return sink.getvalue()


# --- [TABLES] ---------------------------------------------------------------------------
# the one logical -> (xmp, exiftool, exiv2, media) correspondence; `from_logical`/`_flat` derive every read and
# write from it, so a new field is one row and a new provider a carrier supports is one column. GPS: pyexiftool
# reads `Composite:GPS*` decimal degrees directly and folds a returned `Exif.GPSInfo.*` DMS array via `_dms`.
# The ICC-header logicals carry an `exiftool` column (the `ICC_Profile:` group); the `exiv2` family-key
# column is retained mapping data for the recovered carrier keys, never a provider selector.
_FIELD_KEYS: Final[Map[str, FieldKeys]] = Map.of_seq([
    ("title", FieldKeys(xmp="dc:title", exiftool="XMP-dc:Title", exiv2="Xmp.dc.title", media="title")),
    ("headline", FieldKeys(xmp="photoshop:Headline", exiftool="XMP-photoshop:Headline", exiv2="Xmp.photoshop.Headline")),
    ("caption", FieldKeys(xmp="dc:description", exiftool="XMP-dc:Description", exiv2="Xmp.dc.description", media="comment")),
    ("keywords", FieldKeys(xmp="dc:subject", exiftool="XMP-dc:Subject", exiv2="Xmp.dc.subject", media="keywords")),
    ("rating", FieldKeys(xmp="xmp:Rating", exiftool="XMP-xmp:Rating", exiv2="Xmp.xmp.Rating")),
    ("label", FieldKeys(xmp="xmp:Label", exiftool="XMP-xmp:Label", exiv2="Xmp.xmp.Label")),
    ("category", FieldKeys(xmp="photoshop:Category", exiftool="XMP-photoshop:Category", exiv2="Xmp.photoshop.Category")),
    ("genre", FieldKeys(
        xmp="Iptc4xmpCore:IntellectualGenre", exiftool="XMP-iptcCore:IntellectualGenre", exiv2="Xmp.iptc.IntellectualGenre", media="genre"
    )),
    ("language", FieldKeys(xmp="dc:language", exiftool="XMP-dc:Language", exiv2="Xmp.dc.language", media="language")),
    ("instructions", FieldKeys(xmp="photoshop:Instructions", exiftool="XMP-photoshop:Instructions", exiv2="Xmp.photoshop.Instructions")),
    ("description_writer", FieldKeys(xmp="photoshop:CaptionWriter", exiftool="XMP-photoshop:CaptionWriter", exiv2="Xmp.photoshop.CaptionWriter")),
    ("comment", FieldKeys(
        exiftool="File:Comment"
    )),  # JPEG COM — exiftool rows it (no dict column)
    ("album", FieldKeys(media="album")),
    ("album_artist", FieldKeys(media="album_artist")),
    ("composer", FieldKeys(media="composer")),
    ("creator", FieldKeys(xmp="dc:creator", exiftool="XMP-dc:Creator", exiv2="Xmp.dc.creator", media="artist")),
    ("credit", FieldKeys(xmp="photoshop:Credit", exiftool="IPTC:Credit", exiv2="Iptc.Application2.Credit")),
    ("source", FieldKeys(xmp="photoshop:Source", exiftool="IPTC:Source", exiv2="Iptc.Application2.Source")),
    ("copyright", FieldKeys(xmp="dc:rights", exiftool="XMP-dc:Rights", exiv2="Xmp.dc.rights", media="copyright")),
    ("usage_terms", FieldKeys(xmp="xmpRights:UsageTerms", exiftool="XMP-xmpRights:UsageTerms", exiv2="Xmp.xmpRights.UsageTerms")),
    ("web_statement", FieldKeys(xmp="xmpRights:WebStatement", exiftool="XMP-xmpRights:WebStatement", exiv2="Xmp.xmpRights.WebStatement")),
    ("publisher", FieldKeys(xmp="dc:publisher", exiftool="XMP-dc:Publisher", exiv2="Xmp.dc.publisher", media="publisher")),
    ("marked", FieldKeys(xmp="xmpRights:Marked", exiftool="XMP-xmpRights:Marked", exiv2="Xmp.xmpRights.Marked")),
    ("make", FieldKeys(exiftool="EXIF:Make", exiv2="Exif.Image.Make")),
    ("model", FieldKeys(exiftool="EXIF:Model", exiv2="Exif.Image.Model")),
    ("lens", FieldKeys(exiftool="EXIF:LensModel", exiv2="Exif.Photo.LensModel")),
    ("lens_make", FieldKeys(exiftool="EXIF:LensMake", exiv2="Exif.Photo.LensMake")),
    ("software", FieldKeys(xmp="xmp:CreatorTool", exiftool="EXIF:Software", exiv2="Exif.Image.Software", media="encoder")),
    ("producer", FieldKeys(xmp="pdf:Producer", exiftool="XMP-pdf:Producer", exiv2="Xmp.pdf.Producer")),
    ("digital_source_type", FieldKeys(
        xmp="Iptc4xmpExt:DigitalSourceType", exiftool="XMP-iptcExt:DigitalSourceType", exiv2="Xmp.iptcExt.DigitalSourceType"
    )),
    ("orientation", FieldKeys(exiftool="EXIF:Orientation", exiv2="Exif.Image.Orientation")),
    ("exposure_time", FieldKeys(exiftool="EXIF:ExposureTime", exiv2="Exif.Photo.ExposureTime")),
    ("f_number", FieldKeys(exiftool="EXIF:FNumber", exiv2="Exif.Photo.FNumber")),
    ("aperture", FieldKeys(exiftool="EXIF:ApertureValue", exiv2="Exif.Photo.ApertureValue")),
    ("iso", FieldKeys(exiftool="EXIF:ISO", exiv2="Exif.Photo.ISOSpeedRatings")),
    ("focal_length", FieldKeys(exiftool="EXIF:FocalLength", exiv2="Exif.Photo.FocalLength")),
    ("exposure_bias", FieldKeys(exiftool="EXIF:ExposureCompensation", exiv2="Exif.Photo.ExposureBiasValue")),
    ("exposure_program", FieldKeys(exiftool="EXIF:ExposureProgram", exiv2="Exif.Photo.ExposureProgram")),
    ("metering_mode", FieldKeys(exiftool="EXIF:MeteringMode", exiv2="Exif.Photo.MeteringMode")),
    ("flash", FieldKeys(exiftool="EXIF:Flash", exiv2="Exif.Photo.Flash")),
    ("white_balance", FieldKeys(exiftool="EXIF:WhiteBalance", exiv2="Exif.Photo.WhiteBalance")),
    ("serial_number", FieldKeys(exiftool="EXIF:SerialNumber", exiv2="Exif.Photo.BodySerialNumber")),
    ("created", FieldKeys(xmp="xmp:CreateDate", exiftool="EXIF:DateTimeOriginal", exiv2="Exif.Photo.DateTimeOriginal", media="creation_time")),
    ("modified", FieldKeys(xmp="xmp:ModifyDate", exiftool="EXIF:ModifyDate", exiv2="Exif.Image.DateTime")),
    ("gps_lat", FieldKeys(exiv2="Exif.GPSInfo.GPSLatitude")),
    ("gps_lat_ref", FieldKeys(exiv2="Exif.GPSInfo.GPSLatitudeRef")),
    ("gps_lon", FieldKeys(exiv2="Exif.GPSInfo.GPSLongitude")),
    ("gps_lon_ref", FieldKeys(exiv2="Exif.GPSInfo.GPSLongitudeRef")),
    ("altitude", FieldKeys(exiftool="EXIF:GPSAltitude", exiv2="Exif.GPSInfo.GPSAltitude")),
    ("gps_direction", FieldKeys(exiftool="EXIF:GPSImgDirection", exiv2="Exif.GPSInfo.GPSImgDirection")),
    ("gps_timestamp", FieldKeys(exiftool="EXIF:GPSDateStamp", exiv2="Exif.GPSInfo.GPSDateStamp")),
    ("map_datum", FieldKeys(exiftool="EXIF:GPSMapDatum", exiv2="Exif.GPSInfo.GPSMapDatum")),
    ("city", FieldKeys(xmp="photoshop:City", exiftool="XMP-photoshop:City", exiv2="Xmp.photoshop.City")),
    ("state", FieldKeys(xmp="photoshop:State", exiftool="XMP-photoshop:State", exiv2="Xmp.photoshop.State")),
    ("country", FieldKeys(xmp="photoshop:Country", exiftool="XMP-photoshop:Country", exiv2="Xmp.photoshop.Country")),
    ("country_code", FieldKeys(xmp="Iptc4xmpCore:CountryCode", exiftool="XMP-iptcCore:CountryCode", exiv2="Xmp.iptc.CountryCode")),
    ("sublocation", FieldKeys(xmp="Iptc4xmpCore:Location", exiftool="XMP-iptcCore:Location", exiv2="Xmp.iptc.Location")),
    ("document_id", FieldKeys(xmp="xmpMM:DocumentID", exiftool="XMP-xmpMM:DocumentID", exiv2="Xmp.xmpMM.DocumentID")),
    ("instance_id", FieldKeys(xmp="xmpMM:InstanceID", exiftool="XMP-xmpMM:InstanceID", exiv2="Xmp.xmpMM.InstanceID")),
    ("original_id", FieldKeys(xmp="xmpMM:OriginalDocumentID", exiftool="XMP-xmpMM:OriginalDocumentID", exiv2="Xmp.xmpMM.OriginalDocumentID")),
    ("space", FieldKeys(exiftool="EXIF:ColorSpace", exiv2="Exif.Photo.ColorSpace")),
    ("icc_name", FieldKeys(exiftool="ICC_Profile:ProfileDescription")),
    ("render_intent", FieldKeys(exiftool="ICC_Profile:RenderingIntent")),
    ("icc_maker", FieldKeys(exiftool="ICC_Profile:DeviceManufacturer")),
    ("icc_model", FieldKeys(exiftool="ICC_Profile:DeviceModel")),
    ("icc_copyright", FieldKeys(exiftool="ICC_Profile:ProfileCopyright")),
])

# the exiftool binary path (discovery-env -> configured -> `shutil.which`) resolved once at module scope, and
# the scoped `-G`-grouped tag set the read requests (every `exiftool` column plus the signed `Composite:GPS*` pair).
_SETTINGS: Final[MetaSettings] = MetaSettings()
_EXIFTOOL: Final[str] = _SETTINGS.exiftool or shutil.which("exiftool") or "exiftool"
_EXIFTOOL_TAGS: Final[tuple[str, ...]] = (
    *(keys.exiftool for keys in _FIELD_KEYS.values() if keys.exiftool),
    "Composite:GPSLatitude",
    "Composite:GPSLongitude",  # signed decimal GPS pair (`-n`)
    "File:ImageWidth",
    "File:ImageHeight",
    "File:MIMEType",  # `RasterInfo` container facts (no `_FIELD_KEYS` row — the facet materializes separately)
)

# the RASTER reader/writer is the standing pyexiftool one-pass arm (the removed in-process Exiv2 arm is
# superseded — the plane re-bases on the out-of-process provider); ONE `CarrierPolicy` per carrier, each
# pre-folding its runtime lane so the dispatch never re-pairs it — RASTER the PROCESS lane, PDF/MEDIA the THREAD lane.
_CARRIER: Final[Map[MetaCarrier, CarrierPolicy]] = Map.of_seq([
    (MetaCarrier.RASTER, CarrierPolicy(reader=_exiftool_read, writer=_exiftool_write, lane=partial(LanePolicy.offload, modality=Modality.PROCESS, retry=RetryClass.OCCT))),
    (MetaCarrier.PDF, CarrierPolicy(reader=_read_pdf, writer=_write_pdf, lane=partial(LanePolicy.offload, modality=Modality.THREAD, retry=RetryClass.OCCT))),
    (MetaCarrier.MEDIA, CarrierPolicy(reader=_read_media, writer=_write_media, lane=partial(LanePolicy.offload, modality=Modality.THREAD, retry=RetryClass.OCCT))),
])
```
