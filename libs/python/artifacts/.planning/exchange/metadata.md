# [PY_ARTIFACTS_METADATA]

The descriptive-metadata read/write owner at the exchange boundary. `Metadata` is ONE owner binding and recovering the EXIF / IPTC / XMP / ICC descriptive-metadata standards — title, creator, copyright, keyword, comment, rating, camera, exposure, GPS, rights, the full ICC profile header (manufacturer / model / copyright / rendering intent), the xmpMM asset identity/lineage, the raster container structure (dimensions / MIME / embedded thumbnail), and the media container structure (chapters / tracks / duration / bit rate) — on an already-emitted raster, PDF, or media artifact, discriminating the `Metadata` `read`/`write` verb over its `(MetaCarrier, payload)` shape — never a per-standard reader family, never a per-tag `get_author`/`set_title` accessor, never a per-format reader type, and never a carrier×direction case explosion: the carrier rides as the verb payload's first field, not a tag multiplier, so the op is two cases over three carriers, not six. The metadata CARRIER is the closed `MetaCarrier` axis (`RASTER` over the one categorical-best cross-format provider — `pyexiftool` standing, the `pyexiv2` unified in-process arm capability-selected where its cp-gate admits it — plus `pillow` ICC-header detail, `PDF` over `pikepdf`, `MEDIA` over `av`), and each carrier's `(reader, writer, lane)` triple is ONE `CarrierPolicy` row in one `_CARRIER` table — never a `_CARRIER_TABLE` acceptor pair beside a parallel `_LANE` offload map. The EXIF/IPTC/XMP/ICC standards are field-namespace facets every carrier read folds into the ONE `MetaFacts` in a single pass through one `MetaFacts.from_logical` materialization keyed by the one `_FIELD_KEYS` logical→standard correspondence — never a dispatch axis that fragments one raster into four separate provider reads and never four parallel per-standard key tables. Each carrier rides its admitted owner at the runtime placement its package dictates: `pikepdf` (`worker-native`, ungated) and `av` (`cp311-native`, core) resolve in-process folded onto the `_THREAD_GATE` `CapacityLimiter`-bounded `anyio.to_thread.run_sync` (the explicit thread band, never the per-loop 40-token default) so the native qpdf/FFmpeg call never blocks the loop, while the raster cluster crosses the `execution/lanes#LANE`-owned `WORKER_BAND` `CapacityLimiter`-bounded `anyio.to_process.run_sync` band onto the worker that folds EXIF + IPTC + XMP + ICC + GPS + maker-notes in ONE cross-format pass (`pyexiftool` over a temp-file `exiftool -stay_open` subprocess, or the thread-unsafe in-process `pyexiv2` `ImageData` parse where selected) — the `RASTER` lane the `partial(to_process.run_sync, limiter=WORKER_BAND)` value the `CarrierPolicy` row carries, never the unbounded per-loop default process limiter. Every operation returns one `(ContentKey, MetaFacts, bytes)` evidence triple — the recovered (read) or bound (write) `MetaFacts`, the source or re-encoded payload, and the runtime `ContentIdentity.of` content key — that the consumer projects onto the `core/receipt#RECEIPT` `ArtifactReceipt.Metadata(key, carrier, fields, byte_len)` flat-scalar fact exactly as `exchange/credential#PROVENANCE` and `exchange/conformance#CONFORMANCE` return their `(ContentKey, evidence)` pair: a metadata READ recovers the full descriptive field set and a WRITE the re-encoded artifact, both richer than the four-scalar receipt, so the owner returns the evidence and the consumer mints the flat case — never a write-only `@receipted` weave that would discard the read's recovered fields and the write's bytes, never a re-minted identity, and never a producer value object crossing into the receipt owner.

## [01]-[INDEX]

- [01]-[METADATA]: the one descriptive-metadata owner — a two-case `read`/`write` `expression.tagged_union` over the `(MetaCarrier, payload)` shape, the carrier riding as the payload's leading field so the carrier×direction product collapses to verb×payload; the `MetaCarrier` closed axis (`RASTER`/`PDF`/`MEDIA`) whose `_CARRIER` row selects the `(reader, writer, lane)` triple, the `RASTER` reader/writer capability-selected between the in-process `pyexiv2` unified arm and the standing `pyexiftool` cross-format subprocess by `find_spec` availability; the `MetaFacts` composite the readers fold and the writers consume through the one `_FIELD_KEYS` logical→`(xmp, exiftool, exiv2, media)` correspondence and the one `MetaFacts.from_logical` materialization; the `MetaBind` (`MERGE`/`REPLACE`/`STRIP`) write disposition; the `@stamina.retry` weave over the `exiftool -stay_open` spawn transient and the `@beartype(conf=FAULT_CONF)` ingress contract the sibling exchange owners carry; the `(ContentKey, MetaFacts, bytes)` evidence triple the consumer projects onto the `core/receipt#RECEIPT` `ArtifactReceipt.Metadata` case the ARCHITECTURE `[02]-[SEAMS]` `exchange/metadata → core/receipt` edge names. The `Iptc4xmpExt:DigitalSourceType` content-origin field is the UNSIGNED descriptive AI-provenance label, distinct from and beside the SIGNED `exchange/credential#CREDENTIAL` C2PA `DigitalSource` assertion. The abandoned `exif` package is replaced (`pyexiftool` the categorical-best cross-format superset it could never reach); `iptcinfo3`/`python-xmp-toolkit`/`pyvips` are superseded on this carrier by the one cross-format pass — flagged for the final `pyproject` reconciliation, never re-composed here.

## [02]-[METADATA]

- Owner: `Metadata` IS the one descriptive-metadata owner — an `expression.tagged_union` of exactly TWO cases discriminating the read/write verb directly (mirroring `exchange/credential#PROVENANCE`, never a one-field wrapper over a separate op union): `read: tuple[MetaCarrier, bytes]` and `write: tuple[MetaCarrier, bytes, WriteSpec]` (the write instruction `facts`+`bind` bundled in one named `WriteSpec` owner per the `exchange/credential#PROVENANCE`/`exchange/conformance#CONFORMANCE` `(bytes, Spec)` payload convention, never a naked positional `tuple[..., MetaFacts, MetaBind]` decoded by index) — the carrier riding as each payload's leading field and the direction the tag, so a new carrier adds zero cases (the carrier×direction product collapses to verb×payload); `MetaCarrier` the closed `StrEnum` axis (`RASTER`/`PDF`/`MEDIA`) whose `_CARRIER` row selects the acceptor-and-lane; `MetaBind` the closed write disposition (`MERGE`/`REPLACE`/`STRIP`); `MetaFacts` the ONE composite value owner every read materializes and every write consumes — eight nested frozen facets (`Descriptive` editorial + media collection tags + the JPEG COM `comment`, `Rights` creator/usage, `Capture` camera/exposure + the IPTC `Iptc4xmpExt:DigitalSourceType` content-origin/AI-provenance label, `Place` location, `History` xmpMM asset identity/lineage, `Color` the full ICC profile header, `RasterInfo` the raster container dimensions/MIME + embedded thumbnail, `MediaInfo` container chapters/tracks/duration/bit-rate) plus the `conformance` PDF/A·PDF/X status, recovering the full descriptive field set across all four standards plus the container-structure read in one shape and projected to the `core/receipt#RECEIPT` facts. The writable XMP/EXIF/IPTC facets fold through `_flat`; the read-only `Color` ICC header, `RasterInfo` raster container structure, and `MediaInfo` media container structure are evidence-only siblings of `conformance` the writers never re-emit. `CarrierPolicy` is the carrier collapse: one frozen row carries the `(reader, writer, lane)` triple a `MetaCarrier` keys, so the operation routes by one `_CARRIER` lookup over `self.carrier`, never a per-standard reader/writer class family, never a re-discriminating `match` inside an arm, and never a `gated: bool` knob the body re-pairs to a lane the value already selects. The `RASTER` reader/writer is the categorical-best cross-format provider selected ONCE at module scope by `find_spec("pyexiv2")`: where the `pyexiv2` cp-gate admits it, the in-process `ImageData` unified arm (one native parse of EXIF+IPTC+XMP+ICC, `modify_*`/`clear_*` + `modify_icc` + `get_bytes()`, MUST cross `to_process` because Exiv2 is thread-UNsafe); else the standing `pyexiftool` arm (one `ExifToolHelper.get_tags`/`set_tags` `-stay_open` subprocess pass over a temp file covering EXIF+IPTC+XMP+ICC+GPS+maker-notes cross-format) — ONE `CarrierPolicy` either way, both crossing the `WORKER_BAND` process band, never two parallel `RASTER` carriers.
- Cases: `Metadata` cases — `read(carrier, payload)` and `write(carrier, payload, WriteSpec(facts, bind))` — matched by one total `match`/`case` over `self` in `_run`, the read arm collapsing to one acceptor-lookup arm and the write arm to one (the `WriteSpec` unpacked once to `spec.facts`/`spec.bind`), never a parallel reader/writer per carrier or per standard. Each carrier's reader folds its native namespace into the one `MetaFacts`: RASTER reads EXIF + IPTC + XMP + ICC + GPS + maker-notes in ONE cross-format pass — the `pyexiftool` arm through `ExifToolHelper.get_tags(path, _EXIFTOOL_TAGS)` returning `-G`-grouped `"<group>:<tag>"` JSON (`EXIF:`, `IPTC:`, `XMP-*:`, `ICC_Profile:`, `Composite:GPS*`) folded through `_FIELD_KEYS[logical].exiftool`, the `pyexiv2` arm through one `ImageData(payload)` parse folding `read_exif() ∪ read_iptc() ∪ read_xmp()` through `_FIELD_KEYS[logical].exiv2` plus `read_icc()` bytes through `pillow` `ImageCms`; PDF reads the FULL Dublin-Core/XMP-Basic/XMP-MM/Adobe-PDF/Photoshop/xmpRights namespace (the `pdf:Producer` converting-application fact and the `xmpMM:DocumentID`/`InstanceID` identity included) through the `pikepdf` `PdfMetadata` mapping plus `pdfa_status`/`pdfx_status`; MEDIA reads the FFmpeg container tag set through `av` `Container.metadata` plus the container structure — `chapters()` navigation, per-stream `streams` labelling, and `duration` — folded into the `MediaInfo` facet. Every reader emits one `dict[str, object]` keyed by logical field name and folds it through the one `MetaFacts.from_logical`, so a carrier read is one logical projection plus one materialization, never a hand-built per-facet constructor per carrier.
- Auto: `_run` folds `self` through one total `match` whose two arms (read, write) lift the carrier's `CarrierPolicy` row once, so the per-carrier body is data, never a six-arm enumeration. The `pyexiftool` raster reader (`@stamina.retry(on=(RuntimeError, ExifToolVersionError))` over the `-stay_open` spawn transient) spills `payload` to a `NamedTemporaryFile(delete_on_close=False)`, opens one `ExifToolHelper(executable=_EXIFTOOL, common_args=["-G", "-n"])` context, reads the `_EXIFTOOL_TAGS` set once, projects every logical through `_FIELD_KEYS[logical].exiftool`, reads the signed `Composite:GPSLatitude`/`Composite:GPSLongitude` decimals directly (the `-n` numeric output — no DMS fold), splits the `IPTC:Keywords`/`XMP-dc:Subject` list through `_as_tuple`, and resolves the ICC header off the `ICC_Profile:` group tags directly (no `pillow` parse — exiftool decodes the header). The `pyexiv2` raster reader (import-selected, thread-UNsafe → `to_process` only) holds one `ImageData(payload, encoding="utf-8")` parse, merges `read_exif()`/`read_iptc()`/`read_xmp()` into one Exiv2-keyed dict projected through `_FIELD_KEYS[logical].exiv2`, evaluates EXIF `num/den` rationals through `_num` for the `_RATIONAL` float logicals, folds the `Exif.GPSInfo.GPSLatitude` DMS rational triple with its `GPSLatitudeRef` through `_dms` into the signed `Place.gps` pair, and resolves the full ICC header through `_icc` (one `ImageCms.ImageCmsProfile` parse over the `read_icc()` bytes railed through `expression.extra.result.catch`, each `getProfile*` accessor and the `getDefaultIntent` ordinal each railed through `catch` so one missing tag never sinks the rest). Both raster readers fold all four standards into the one `MetaFacts.from_logical`. The raster writer clears the DESCRIPTIVE namespace on `REPLACE`/`STRIP` (`-all=` group-wildcard scrub via `ExifToolHelper.execute` because `set_tags` rejects an empty dict, or `pyexiv2` `clear_exif`/`clear_iptc`/`clear_xmp`) while PRESERVING the ICC profile on `REPLACE` (`Color` is read-only evidence the writer can never re-author; only `STRIP` also scrubs the profile), sets each writable logical from `_flat(facts)` through `_FIELD_KEYS[logical].exiftool`/`.exiv2` (the keyword tuple bound as a `list` for the repeated `-IPTC:Keywords`/`Iptc.Application2.Keywords` directives), and returns the metadata-only re-encoded bytes (`Path(tmp).read_bytes()` after `set_tags`, or `ImageData.get_bytes()`) — a container-metadata mutation WITHOUT a pixel re-encode, higher-fidelity than a raster re-encode, so no `_rational` re-encode is needed (the provider owns the IFD-type round-trip). The PDF arms open one `PdfMetadata` context, the read folding every `_FIELD_KEYS` xmp qname present in the mapping plus `pdfa_status`/`pdfx_status`, the write deleting-then-assigning under the bind policy; the MEDIA arms open the `av` container, the read folding every `_FIELD_KEYS` media tag present in `Container.metadata` AND the `_media_info` container-structure read (the `Container.duration` seconds over `_AV_TIME_BASE`, the `chapters()` markers projected to `Chapter(start, title)` rows, and the per-`streams` `Track(kind, language, title)` labelling) folded into the `MediaInfo` facet while the container is still open, the write a `demux` → `add_stream_from_template` → `mux` bitstream-copy remux that rebinds `OutputContainer.metadata` without re-encoding. `_flat` is the one facts→logical projection every writer derives its bindings from (`structs.asdict` over the writable facets, dropping empties and tuples), and `from_logical` is its inverse (one `msgspec.convert(strict=False)` per facet over the logical dict, coercing the standard string values to each facet field's type), so a field reaches every provider from one declared `_FIELD_KEYS` correspondence rather than a per-standard getter/setter pair.
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

import stamina
from anyio import CapacityLimiter, to_process, to_thread
from beartype import beartype
from builtins import frozendict
from expression import case, tag, tagged_union
from expression.extra.result import catch
from msgspec import Struct, convert, structs
from msgspec import json as msgjson
from pydantic_settings import BaseSettings, SettingsConfigDict

from rasm.runtime.content_identity import ContentIdentity, ContentKey
from rasm.runtime.faults import FAULT_CONF, RuntimeRail, async_boundary
from rasm.runtime.lanes import WORKER_BAND

lazy from exiftool import ExifToolHelper  # exiftool -stay_open cross-format driver, to_process worker band
lazy from exiftool.exceptions import ExifToolVersionError  # typed spawn/version fault the retry weave re-execs on
lazy import pyexiv2  # OPTIONAL in-process Exiv2 unified arm; cp315-absent -> the exiftool arm stands
lazy from PIL import ImageCms  # pillow ICC-header reader over the pyexiv2 read_icc() bytes
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
# pyexiv2 arm and every other numeric field is a plain numeral `msgspec.convert(strict=False)` coerces directly.
_RATIONAL: Final[frozenset[str]] = frozenset({"f_number", "focal_length", "aperture", "exposure_bias", "altitude", "gps_direction"})

# ICC default-rendering-intent ordinal -> token (the liblcms2/`ImageCms.getDefaultIntent` 0-3 scale, the pyexiv2 arm's ICC intent).
_INTENT_NAME: Final[frozendict[int, str]] = frozendict({0: "perceptual", 1: "relative", 2: "saturation", 3: "absolute"})
_AV_TIME_BASE: Final[int] = 1_000_000  # `Container.duration` is AV_TIME_BASE microseconds; divide for seconds


# --- [MODELS] ---------------------------------------------------------------------------
class FieldKeys(Struct, frozen=True, gc=False):  # the per-logical provider correspondence row
    xmp: str = ""  # XMP qname for the pikepdf PDF carrier (`dc:title`, `xmpMM:DocumentID`, `pdf:Producer`)
    exiftool: str = ""  # exiftool `-G` grouped tag for the RASTER pyexiftool arm (`EXIF:Make`, `IPTC:Keywords`, `ICC_Profile:ProfileDescription`)
    exiv2: str = ""  # Exiv2 family key for the RASTER pyexiv2 arm (`Exif.Image.Make`, `Iptc.Application2.Byline`, `Xmp.dc.creator`)
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
    comment: str = ""  # JPEG COM segment — pyexiv2 `read_comment`/`modify_comment`, exiftool `File:Comment`; raster-only (no XMP/media twin)
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
        None  # signed decimal degrees — pyexiftool reads `Composite:GPS*` directly, pyexiv2 folds the DMS array via `_dms`
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
):  # raster container structure read-only (pyexiv2 `get_pixel_*`/`get_mime_type`/`read_thumbnail`) — the raster-carrier sibling of `Color`/`MediaInfo`
    width: int = 0  # pyexiv2 `get_pixel_width()` / exiftool `File:ImageWidth`
    height: int = 0  # pyexiv2 `get_pixel_height()` / exiftool `File:ImageHeight`
    mime: str = ""  # pyexiv2 `get_mime_type()` / exiftool `File:MIMEType` — the container's sniffed MIME
    thumbnail: bytes = b""  # pyexiv2 `read_thumbnail()` / exiftool `-b -ThumbnailImage` — the embedded EXIF preview, a DAM/forensic band mirroring credential's `resources`


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

    async def of(self) -> RuntimeRail[tuple[ContentKey, MetaFacts, bytes]]:
        return await async_boundary(f"metadata.{self.carrier.value}.{self.tag}", self._emit)

    # evidence triple, never a `@receipted` weave: a READ recovers the full `MetaFacts`, a WRITE the
    # re-encoded bytes — the consumer mints the flat `ArtifactReceipt.Metadata` from the triple.
    async def _emit(self) -> tuple[ContentKey, MetaFacts, bytes]:
        payload, facts = await self._run()
        return ContentIdentity.of(f"metadata.{self.carrier.value}.{self.tag}", payload), facts, payload

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
    # the pyexiv2 EXIF GPS fold: a degree/minute/second rational triple plus an N/S/E/W hemisphere ref
    # to signed decimal degrees. The pyexiftool arm reads `Composite:GPS*` signed decimals directly.
    parts = [_num(token) for token in value.replace(",", " ").split() if token]
    if not parts:
        return None
    degrees = sum(part / 60**index for index, part in enumerate(parts[:3]))
    return -degrees if ref.strip().upper().startswith(("S", "W")) else degrees  # N/S/E/W or North/South/...


def _icc(blob: bytes) -> dict[str, object]:
    # the pyexiv2 arm's ICC header read over the `read_icc()` bytes: description + manufacturer/model/copyright
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
        "render_intent": _INTENT_NAME.get(intent, ""),
    }


# --- [RASTER_CARRIER] -------------------------------------------------------------------
# The RASTER cluster crosses the `WORKER_BAND` `to_process` band. The categorical-best cross-format provider
# folds EXIF + IPTC + XMP + ICC + GPS + maker-notes in ONE pass, replacing the abandoned `exif` (JPEG-EXIF-IFD
# only) and superseding the former four-provider split (`iptcinfo3` IIM + libxmp XMP + pillow ICC + libvips).
@stamina.retry(on=(RuntimeError, ExifToolVersionError), attempts=3, timeout=30.0)
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


@stamina.retry(on=(RuntimeError, ExifToolVersionError), attempts=3, timeout=30.0)
def _exiftool_write(payload: bytes, facts: MetaFacts, bind: MetaBind) -> bytes:
    with NamedTemporaryFile(suffix=".img", delete_on_close=False) as tmp:  # Exemption: exiftool mutates a real path in place
        tmp.write(payload)
        tmp.close()
        with ExifToolHelper(executable=_EXIFTOOL, common_args=["-G", "-n"]) as et:
            if bind in (MetaBind.REPLACE, MetaBind.STRIP):
                et.execute("-all=", "-overwrite_original", tmp.name)  # group-wildcard scrub; `set_tags` rejects an empty dict
            if bind is not MetaBind.STRIP:
                tags: dict[str, str | list[str]] = {
                    keys.exiftool: value for logical, value in _flat(facts).items() if (keys := _FIELD_KEYS.get(logical)) and keys.exiftool
                }
                if facts.descriptive.keywords:  # a `list` value emits the repeated `-IPTC:Keywords=`/`-XMP-dc:Subject=` directives
                    tags["IPTC:Keywords"] = tags["XMP-dc:Subject"] = list(facts.descriptive.keywords)
                if tags:
                    et.set_tags(tmp.name, tags, params=["-overwrite_original"])
        return Path(tmp.name).read_bytes()


def _exiv2_read(payload: bytes) -> MetaFacts:
    with pyexiv2.ImageData(payload, encoding="utf-8") as img:  # Exemption: the native Exiv2 handle is context-scoped, must close
        merged: dict[str, object] = {**img.read_exif(), **img.read_iptc(), **img.read_xmp()}
        icc = img.read_icc()
        raster = RasterInfo(  # container facts + embedded preview read while the native handle is open
            width=img.get_pixel_width(),
            height=img.get_pixel_height(),
            mime=img.get_mime_type(),
            thumbnail=catch(exception=Exception)(img.read_thumbnail)().default_value(b""),  # absent/odd-format thumbnail skips by omission
        )
        comment = catch(exception=Exception)(img.read_comment)().default_value(
            ""
        )  # JPEG COM segment — a separate Exiv2 method, not a family dict tag
    flat: dict[str, object] = {logical: _scalar(merged[keys.exiv2]) for logical, keys in _FIELD_KEYS.items() if keys.exiv2 and keys.exiv2 in merged}
    flat.update({logical: _num(str(flat[logical])) for logical in _RATIONAL if logical in flat})
    flat["keywords"] = _as_tuple(merged.get("Iptc.Application2.Keywords") or merged.get("Xmp.dc.subject"))
    flat["comment"] = comment
    lat = _dms(_scalar(merged.get("Exif.GPSInfo.GPSLatitude", "")), _scalar(merged.get("Exif.GPSInfo.GPSLatitudeRef", "")))
    lon = _dms(_scalar(merged.get("Exif.GPSInfo.GPSLongitude", "")), _scalar(merged.get("Exif.GPSInfo.GPSLongitudeRef", "")))
    flat["gps"] = (lat, lon) if lat is not None and lon is not None else None
    flat["icc_present"] = bool(icc)
    flat.update(_icc(icc))  # the full ICC header off the read_icc() bytes, railed through `catch`
    return MetaFacts.from_logical(flat, raster=raster)


def _exiv2_write(payload: bytes, facts: MetaFacts, bind: MetaBind) -> bytes:
    with pyexiv2.ImageData(payload, encoding="utf-8") as img:  # Exemption: native Exiv2 mutation kernel over one context-scoped handle
        if bind in (MetaBind.REPLACE, MetaBind.STRIP):
            img.clear_exif()
            img.clear_iptc()
            img.clear_xmp()
            img.clear_comment()  # the JPEG COM segment is a separate Exiv2 namespace, scrubbed with the descriptive families
            if bind is MetaBind.STRIP:  # REPLACE preserves the ICC profile (`Color` is read-only evidence); only STRIP scrubs it
                img.clear_icc()
        if bind is not MetaBind.STRIP:
            keyed: dict[str, str | list[str]] = {
                keys.exiv2: value for logical, value in _flat(facts).items() if (keys := _FIELD_KEYS.get(logical)) and keys.exiv2
            }
            if facts.descriptive.keywords:
                keyed["Iptc.Application2.Keywords"] = keyed["Xmp.dc.subject"] = list(facts.descriptive.keywords)
            img.modify_exif({key: value for key, value in keyed.items() if key.startswith("Exif.")})
            img.modify_iptc({key: value for key, value in keyed.items() if key.startswith("Iptc.")})
            img.modify_xmp({key: value for key, value in keyed.items() if key.startswith("Xmp.")})
            if facts.descriptive.comment:  # the JPEG COM rides its own Exiv2 `modify_comment`, not the family dicts (no `exiv2` `_FIELD_KEYS` column)
                img.modify_comment(facts.descriptive.comment)
        return img.get_bytes()  # metadata-only re-encode of the original container, no pixel re-encode


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
                    if (keys := _FIELD_KEYS.get(logical)) and keys.xmp:
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
            kept.update({keys.media: value for logical, value in _flat(facts).items() if (keys := _FIELD_KEYS.get(logical)) and keys.media})
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
# reads the signed `Composite:GPS*` decimals directly, pyexiv2 folds the `Exif.GPSInfo.*` DMS array via `_dms`.
# The ICC-header logicals carry an `exiftool` column (the `ICC_Profile:` group) but no `exiv2` column — the pyexiv2
# arm resolves them through `pillow` off the `read_icc()` bytes.
_FIELD_KEYS: Final[frozendict[str, FieldKeys]] = frozendict({
    "title": FieldKeys(xmp="dc:title", exiftool="XMP-dc:Title", exiv2="Xmp.dc.title", media="title"),
    "headline": FieldKeys(xmp="photoshop:Headline", exiftool="XMP-photoshop:Headline", exiv2="Xmp.photoshop.Headline"),
    "caption": FieldKeys(xmp="dc:description", exiftool="XMP-dc:Description", exiv2="Xmp.dc.description", media="comment"),
    "keywords": FieldKeys(xmp="dc:subject", exiftool="XMP-dc:Subject", exiv2="Xmp.dc.subject", media="keywords"),
    "rating": FieldKeys(xmp="xmp:Rating", exiftool="XMP-xmp:Rating", exiv2="Xmp.xmp.Rating"),
    "label": FieldKeys(xmp="xmp:Label", exiftool="XMP-xmp:Label", exiv2="Xmp.xmp.Label"),
    "category": FieldKeys(xmp="photoshop:Category", exiftool="XMP-photoshop:Category", exiv2="Xmp.photoshop.Category"),
    "genre": FieldKeys(
        xmp="Iptc4xmpCore:IntellectualGenre", exiftool="XMP-iptcCore:IntellectualGenre", exiv2="Xmp.iptc.IntellectualGenre", media="genre"
    ),
    "language": FieldKeys(xmp="dc:language", exiftool="XMP-dc:Language", exiv2="Xmp.dc.language", media="language"),
    "instructions": FieldKeys(xmp="photoshop:Instructions", exiftool="XMP-photoshop:Instructions", exiv2="Xmp.photoshop.Instructions"),
    "description_writer": FieldKeys(xmp="photoshop:CaptionWriter", exiftool="XMP-photoshop:CaptionWriter", exiv2="Xmp.photoshop.CaptionWriter"),
    "comment": FieldKeys(
        exiftool="File:Comment"
    ),  # JPEG COM — exiftool rows it; the pyexiv2 arm reads/writes it via `read_comment`/`modify_comment` (no dict column)
    "album": FieldKeys(media="album"),
    "album_artist": FieldKeys(media="album_artist"),
    "composer": FieldKeys(media="composer"),
    "creator": FieldKeys(xmp="dc:creator", exiftool="XMP-dc:Creator", exiv2="Xmp.dc.creator", media="artist"),
    "credit": FieldKeys(xmp="photoshop:Credit", exiftool="IPTC:Credit", exiv2="Iptc.Application2.Credit"),
    "source": FieldKeys(xmp="photoshop:Source", exiftool="IPTC:Source", exiv2="Iptc.Application2.Source"),
    "copyright": FieldKeys(xmp="dc:rights", exiftool="XMP-dc:Rights", exiv2="Xmp.dc.rights", media="copyright"),
    "usage_terms": FieldKeys(xmp="xmpRights:UsageTerms", exiftool="XMP-xmpRights:UsageTerms", exiv2="Xmp.xmpRights.UsageTerms"),
    "web_statement": FieldKeys(xmp="xmpRights:WebStatement", exiftool="XMP-xmpRights:WebStatement", exiv2="Xmp.xmpRights.WebStatement"),
    "publisher": FieldKeys(xmp="dc:publisher", exiftool="XMP-dc:Publisher", exiv2="Xmp.dc.publisher", media="publisher"),
    "marked": FieldKeys(xmp="xmpRights:Marked", exiftool="XMP-xmpRights:Marked", exiv2="Xmp.xmpRights.Marked"),
    "make": FieldKeys(exiftool="EXIF:Make", exiv2="Exif.Image.Make"),
    "model": FieldKeys(exiftool="EXIF:Model", exiv2="Exif.Image.Model"),
    "lens": FieldKeys(exiftool="EXIF:LensModel", exiv2="Exif.Photo.LensModel"),
    "lens_make": FieldKeys(exiftool="EXIF:LensMake", exiv2="Exif.Photo.LensMake"),
    "software": FieldKeys(xmp="xmp:CreatorTool", exiftool="EXIF:Software", exiv2="Exif.Image.Software", media="encoder"),
    "producer": FieldKeys(xmp="pdf:Producer", exiftool="XMP-pdf:Producer", exiv2="Xmp.pdf.Producer"),
    "digital_source_type": FieldKeys(
        xmp="Iptc4xmpExt:DigitalSourceType", exiftool="XMP-iptcExt:DigitalSourceType", exiv2="Xmp.iptcExt.DigitalSourceType"
    ),
    "orientation": FieldKeys(exiftool="EXIF:Orientation", exiv2="Exif.Image.Orientation"),
    "exposure_time": FieldKeys(exiftool="EXIF:ExposureTime", exiv2="Exif.Photo.ExposureTime"),
    "f_number": FieldKeys(exiftool="EXIF:FNumber", exiv2="Exif.Photo.FNumber"),
    "aperture": FieldKeys(exiftool="EXIF:ApertureValue", exiv2="Exif.Photo.ApertureValue"),
    "iso": FieldKeys(exiftool="EXIF:ISO", exiv2="Exif.Photo.ISOSpeedRatings"),
    "focal_length": FieldKeys(exiftool="EXIF:FocalLength", exiv2="Exif.Photo.FocalLength"),
    "exposure_bias": FieldKeys(exiftool="EXIF:ExposureCompensation", exiv2="Exif.Photo.ExposureBiasValue"),
    "exposure_program": FieldKeys(exiftool="EXIF:ExposureProgram", exiv2="Exif.Photo.ExposureProgram"),
    "metering_mode": FieldKeys(exiftool="EXIF:MeteringMode", exiv2="Exif.Photo.MeteringMode"),
    "flash": FieldKeys(exiftool="EXIF:Flash", exiv2="Exif.Photo.Flash"),
    "white_balance": FieldKeys(exiftool="EXIF:WhiteBalance", exiv2="Exif.Photo.WhiteBalance"),
    "serial_number": FieldKeys(exiftool="EXIF:SerialNumber", exiv2="Exif.Photo.BodySerialNumber"),
    "created": FieldKeys(xmp="xmp:CreateDate", exiftool="EXIF:DateTimeOriginal", exiv2="Exif.Photo.DateTimeOriginal", media="creation_time"),
    "modified": FieldKeys(xmp="xmp:ModifyDate", exiftool="EXIF:ModifyDate", exiv2="Exif.Image.DateTime"),
    "gps_lat": FieldKeys(exiv2="Exif.GPSInfo.GPSLatitude"),
    "gps_lat_ref": FieldKeys(exiv2="Exif.GPSInfo.GPSLatitudeRef"),
    "gps_lon": FieldKeys(exiv2="Exif.GPSInfo.GPSLongitude"),
    "gps_lon_ref": FieldKeys(exiv2="Exif.GPSInfo.GPSLongitudeRef"),
    "altitude": FieldKeys(exiftool="EXIF:GPSAltitude", exiv2="Exif.GPSInfo.GPSAltitude"),
    "gps_direction": FieldKeys(exiftool="EXIF:GPSImgDirection", exiv2="Exif.GPSInfo.GPSImgDirection"),
    "gps_timestamp": FieldKeys(exiftool="EXIF:GPSDateStamp", exiv2="Exif.GPSInfo.GPSDateStamp"),
    "map_datum": FieldKeys(exiftool="EXIF:GPSMapDatum", exiv2="Exif.GPSInfo.GPSMapDatum"),
    "city": FieldKeys(xmp="photoshop:City", exiftool="XMP-photoshop:City", exiv2="Xmp.photoshop.City"),
    "state": FieldKeys(xmp="photoshop:State", exiftool="XMP-photoshop:State", exiv2="Xmp.photoshop.State"),
    "country": FieldKeys(xmp="photoshop:Country", exiftool="XMP-photoshop:Country", exiv2="Xmp.photoshop.Country"),
    "country_code": FieldKeys(xmp="Iptc4xmpCore:CountryCode", exiftool="XMP-iptcCore:CountryCode", exiv2="Xmp.iptc.CountryCode"),
    "sublocation": FieldKeys(xmp="Iptc4xmpCore:Location", exiftool="XMP-iptcCore:Location", exiv2="Xmp.iptc.Location"),
    "document_id": FieldKeys(xmp="xmpMM:DocumentID", exiftool="XMP-xmpMM:DocumentID", exiv2="Xmp.xmpMM.DocumentID"),
    "instance_id": FieldKeys(xmp="xmpMM:InstanceID", exiftool="XMP-xmpMM:InstanceID", exiv2="Xmp.xmpMM.InstanceID"),
    "original_id": FieldKeys(xmp="xmpMM:OriginalDocumentID", exiftool="XMP-xmpMM:OriginalDocumentID", exiv2="Xmp.xmpMM.OriginalDocumentID"),
    "space": FieldKeys(exiftool="EXIF:ColorSpace", exiv2="Exif.Photo.ColorSpace"),
    "icc_name": FieldKeys(exiftool="ICC_Profile:ProfileDescription"),
    "render_intent": FieldKeys(exiftool="ICC_Profile:RenderingIntent"),
    "icc_maker": FieldKeys(exiftool="ICC_Profile:DeviceManufacturer"),
    "icc_model": FieldKeys(exiftool="ICC_Profile:DeviceModel"),
    "icc_copyright": FieldKeys(exiftool="ICC_Profile:ProfileCopyright"),
})

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

# the in-process thread bound for the GIL-releasing pikepdf/av arms — explicit, never the per-loop
# 40-token default; the RASTER lane rides the shared `execution/lanes#LANE` `WORKER_BAND` instead.
_THREAD_GATE: Final[CapacityLimiter] = CapacityLimiter(8)

# the RASTER reader/writer is the categorical-best cross-format provider selected ONCE by import availability:
# the in-process `pyexiv2` unified arm where its cp-gate admits it (thread-UNsafe -> `to_process` mandatory),
# else the standing `pyexiftool` subprocess arm — ONE `CarrierPolicy` either way, both on `WORKER_BAND`.
_RASTER_POLICY: Final[CarrierPolicy] = (
    CarrierPolicy(reader=_exiv2_read, writer=_exiv2_write, lane=partial(to_process.run_sync, limiter=WORKER_BAND))
    if find_spec("pyexiv2") is not None
    else CarrierPolicy(reader=_exiftool_read, writer=_exiftool_write, lane=partial(to_process.run_sync, limiter=WORKER_BAND))
)
# each `CarrierPolicy` pre-folds its limiter so the dispatch never re-pairs it: RASTER crosses the shared
# `WORKER_BAND`-bounded `to_process` worker, PDF/MEDIA the `_THREAD_GATE`-bounded in-process `to_thread`.
_CARRIER: Final[frozendict[MetaCarrier, CarrierPolicy]] = frozendict({
    MetaCarrier.RASTER: _RASTER_POLICY,
    MetaCarrier.PDF: CarrierPolicy(reader=_read_pdf, writer=_write_pdf, lane=partial(to_thread.run_sync, limiter=_THREAD_GATE)),
    MetaCarrier.MEDIA: CarrierPolicy(reader=_read_media, writer=_write_media, lane=partial(to_thread.run_sync, limiter=_THREAD_GATE)),
})
```

## [03]-[RESEARCH]

- [RASTER_METADATA_PROVIDERS] [RESOLVED]: the RASTER carrier is rebound off the abandoned `exif` package (verified NOT installed on cp315 — `assay api resolve exif` -> `unsupported`; the former `_exif_fields`/`_write_exif_fields` over `exif.Image` were illusory code) onto the brief-mandated categorical-best cross-format provider. `pyexiftool` (verified INSTALLED `0.5.6`, pure-Python, cp315-ungated) is the standing arm: one `ExifToolHelper(executable=_EXIFTOOL, common_args=["-G", "-n"])` `-stay_open` subprocess over a `NamedTemporaryFile`, `get_tags(path, _EXIFTOOL_TAGS)` returning `-G`-grouped `"<group>:<tag>"` JSON folded through `_FIELD_KEYS[logical].exiftool` into `MetaFacts.from_logical`, `set_tags(path, tags, params=["-overwrite_original"])` for the write, `execute("-all=", "-overwrite_original", path)` for the STRIP/REPLACE group-wildcard scrub, the `Composite:GPS*` signed decimals read directly (the `-n` numeric output), and the ICC header off the `ICC_Profile:` group — the EXIF + IPTC + XMP + ICC + GPS + maker-note superset the JPEG-EXIF-IFD-only `exif` could never reach, replacing the former four-provider `exif`/`iptcinfo3`/`libxmp`/`pillow-ICC` split with ONE cross-format pass. The `executable=` boundary resolves discovery-env (`RASM_META_EXIFTOOL` via `MetaSettings`) -> configured path -> `shutil.which("exiftool")`, the brief [03] system-tool subprocess boundary, never a hardcoded literal.
- [PYEXIV2_UNIFIED_ARM] [RESOLVED]: the optional in-process `pyexiv2` arm (Exiv2 C++ core, verified cp-gated `; python_version<'3.15'` — `assay api resolve pyexiv2` -> `unsupported` on cp315, ABSENT) supersedes the subprocess split WHERE its cp-gate admits it: ONE `ImageData(payload, encoding="utf-8")` parse yielding `read_exif() ∪ read_iptc() ∪ read_xmp()` folded through `_FIELD_KEYS[logical].exiv2`, `read_icc()` bytes through the retained `pillow` `ImageCms` `_icc` header parse, `modify_exif`/`modify_iptc`/`modify_xmp` + `clear_*` + `modify_icc` under the `MetaBind` disposition, and `get_bytes()` a metadata-only re-encode WITHOUT a pixel re-encode (higher fidelity than a raster re-encode, no `NamedTemporaryFile` round-trip). The `RASTER` `CarrierPolicy` reader/writer is selected ONCE at module scope by `find_spec("pyexiv2")` — the in-process arm where present, the `pyexiftool` arm as the cp315 standing fallback, ONE carrier policy either way. pyexiv2 is thread-UNsafe (process-global Exiv2 state), so it MUST cross the `WORKER_BAND` `to_process` band, NEVER the `_THREAD_GATE` `to_thread` arm the GIL-releasing `pikepdf`/`av` arms use.
- [CARRIER_DISCRIMINANT] [RESOLVED]: the owner discriminates by the `read`/`write` VERB over the `(MetaCarrier, payload)` shape, not by standard and not by a carrier×direction case product — a single raster carries EXIF + IPTC + XMP + ICC simultaneously, so a per-standard `ReadExif`/`ReadIptc`/`ReadXmp` op family fragments one carrier into four reads where ONE `get_tags`/`ImageData` pass recovers all four into the one `MetaFacts`, and a six-case `read_raster`/`write_raster`/`read_pdf`/… union is the carrier×direction explosion the two-case `Metadata` `tagged_union` collapses by riding the carrier as the verb payload's first field. The standards are field-namespace facets the carrier readers fold (`Capture`/`Place` from EXIF/GPS, `Descriptive`/`Rights` from XMP/IPTC, `Color` from ICC, `conformance` from PDF/A·PDF/X) through the one `_FIELD_KEYS` correspondence and the one `MetaFacts.from_logical` materialization, and the `MetaBind` (`MERGE`/`REPLACE`/`STRIP`) write policy carries the disposition as a behaviour-bearing value rather than a `clear: bool` knob, the `STRIP` arm the privacy-scrub (`exiftool -all=`, `pyexiv2 clear_*`, `pikepdf del meta[key]`, `av` empty `OutputContainer.metadata`).
- [FIELD_CORRESPONDENCE] [RESOLVED]: the former per-standard tables collapse into one primary `_FIELD_KEYS: frozendict[str, FieldKeys]` keyed by logical field name, each `FieldKeys(xmp, exiftool, exiv2, media)` row carrying the four provider spellings of one field per the DERIVED_LOGIC one-primary-correspondence law — `_read_pdf` reads its `xmp` column, `_exiftool_read`/`_exiv2_read` read their `exiftool`/`exiv2` columns, `_read_media` its `media` column, and `_flat`/the writers write their own column, and the `_CARRIER` row collapses the former acceptor pair and offload map into one `CarrierPolicy(reader, writer, lane)` per the algorithms.md ROUTE_UNION "one policy row carries the whole axis bundle" law. `MetaFacts.from_logical` (one `msgspec.convert(strict=False)` per facet) is the one materializer all four readers fold into, and `_flat` (`structs.asdict` over the writable facets) is its inverse — the logical name is the facet field name, so a new field needs no per-provider table edit. The IPTC Extension `Iptc4xmpExt:DigitalSourceType` content-origin field (the plain-XMP AI-provenance label over the `digitalCapture`/`algorithmicMedia`/`trainedAlgorithmicMedia`/`composite` IPTC NewsCodes IRIs) rides the existing `_FIELD_KEYS` row across all four providers, distinct from the SIGNED `exchange/credential#CREDENTIAL` C2PA `DigitalSource` assertion (this is the unsigned descriptive field a DAM consumer reads beside, not instead of, the credential manifest).
- [RESILIENCE_AND_CONTRACT] [RESOLVED]: the exiftool `-stay_open` spawn death (a `RuntimeError` from `run()` or an `ExifToolVersionError` on a binary below `12.15`) is the ONE retriable boundary fault — `@stamina.retry(on=(RuntimeError, ExifToolVersionError), attempts=3, timeout=30.0)` over the module-level `_exiftool_read`/`_exiftool_write` re-execs the binary on a transient spawn failure, distinct from a deterministic tag/format fault that never succeeds on retry, mirroring the `exchange/credential#PROVENANCE` `@stamina.retry` weave. The `(MetaCarrier, bytes, MetaFacts, MetaBind)` ingress carries the shared `@beartype(conf=FAULT_CONF)` contract on the `Metadata.Read`/`Write` factories (the `exchange/conformance#CONFORMANCE`/`exchange/credential#PROVENANCE` sibling contract), and the best-effort ICC/profile captures rail through `expression.extra.result.catch` (the conformance `_resilient` shape) rather than bare `try`/`except`, so a malformed profile or one missing accessor skips by omission.
- [PROVIDER_SUPERSESSION] [RESOLVED]: `exif` is removed (dead, already absent from `pyproject`; the code fence's `lazy import exif` was the sole stale point, now gone). The former RASTER split providers are superseded on this carrier by the one cross-format pass: `iptcinfo3` (IPTC-IIM, subsumed by `pyexiftool`/`pyexiv2` cross-format IPTC), `python-xmp-toolkit`/`libxmp` (XMP packet, subsumed AND verified `unsupported` on cp315 — `assay api resolve libxmp` -> absent, so composing it as a live path is fragile), and `pyvips` (the former decode/re-encode + ICC-byte carrier, unnecessary for a metadata-only op that the provider mutates in place at higher fidelity). `pillow` `ImageCms` is RETAINED for the pyexiv2 arm's ICC-header parse. The brief [03] keeps `iptcinfo3`/`python-xmp-toolkit` admitted; metadata.md is now their sole consumer, so their supersession is flagged for the final `pyproject` reconciliation per the brief's own supersede-flag mechanism, never removed unilaterally here.
- [CONTAINER_AND_THUMBNAIL] [RESOLVED]: the descriptive read now recovers the container-structure and preview evidence both raster arms expose but the prior facet set dropped, each growing the owner in place per the page's own Growth law (one facet/field + one reader leg + one `populated` term, never a parallel type). The new read-only `RasterInfo` facet (the raster-carrier twin of `Color`/`MediaInfo`) carries `width`/`height`/`mime`/`thumbnail`: the `pyexiv2` arm reads `get_pixel_width()`/`get_pixel_height()`/`get_mime_type()`/`read_thumbnail()` off the open native handle (the thumbnail railed through `expression.extra.result.catch` so an absent/odd-format preview skips by omission), the `pyexiftool` arm reads `File:ImageWidth`/`File:ImageHeight`/`File:MIMEType` off the `-G` JSON (appended to `_EXIFTOOL_TAGS` beside the `Composite:GPS*` pair, no `_FIELD_KEYS` row because the facet materializes separately like `MediaInfo`) plus `execute("-b", "-ThumbnailImage", path, raw_bytes=True)` for the undecoded preview bytes — the DAM/forensic band mirroring `exchange/credential#CREDENTIAL`'s `resources` thumbnail extraction, read-only (never re-emitted on write; a `modify_thumbnail` rebind is a future `_flat` growth). The `Descriptive.comment` field rows the JPEG COM segment across both arms: the `pyexiftool` arm through the `_FIELD_KEYS` `comment` row (`File:Comment`, folded and re-written by `_flat`/the `-all=` scrub automatically), the `pyexiv2` arm through its separate `read_comment`/`modify_comment`/`clear_comment` methods (no `exiv2` dict column, the COM being a distinct Exiv2 namespace not in `read_exif() ∪ read_iptc() ∪ read_xmp()`). The `MediaInfo.bit_rate` field folds `Container.bit_rate` while the container is open (beside `duration`/`chapters`/`tracks`), the descriptive container-structure fact the prior read omitted. The exiftool read decode additionally swaps `set_json_loads(msgjson.decode)` (msgspec over the stdlib parser on the `get_tags` JSON path, the catalog-cited faster parse — the shared wire-codec rail layered onto the folder provider). No receipt shape change: the new evidence rides the `populated` tally and the returned `MetaFacts` triple, the `ArtifactReceipt.Metadata` four-scalar case unchanged.
- [META_RECEIPT_CASE] [RESOLVED]: the descriptive-metadata operation returns the rich `(ContentKey, MetaFacts, bytes)` evidence triple, and the consumer projects the ARCHITECTURE `[02]-[SEAMS]` `exchange/metadata → python:artifacts/core/receipt` edge — the carrier, the populated descriptive-field count, and the payload byte length onto the shared `core/receipt#RECEIPT` `ArtifactReceipt.Metadata` case `metadata: tuple[ContentKey, str, int, int]` mirroring the flat-scalar `Credential`/`Media` cases, so the receipt owner imports no `MetaFacts` value object. `_emit` carries NO `@receipted` weave — exactly as `exchange/credential#PROVENANCE` `_emit` carries `@stamina.retry` alone and `exchange/conformance#CONFORMANCE` `close` carries none, both returning the rich `(ContentKey, evidence)` pair the consumer mints the receipt from — because a metadata READ recovers the full `MetaFacts` (the descriptive field set the reading caller needs) and a WRITE the re-encoded artifact bytes, both exceeding the four-scalar summary. `_emit` keys the (source or re-encoded) payload through `ContentIdentity.of` and returns `(key, facts, payload)`; the write arms key the re-encoded bytes so a metadata-bound artifact carries a fresh content key the `csharp:Rasm.Persistence` store re-derives, the read arms key the source bytes, and the metadata owner re-mints no canonical content key (the runtime `content_identity` owns it).
