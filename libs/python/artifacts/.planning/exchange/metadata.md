# [PY_ARTIFACTS_METADATA]

The descriptive-metadata read/write owner at the exchange boundary. `Metadata` is ONE owner binding and recovering the EXIF / IPTC / XMP / ICC descriptive-metadata standards — title, creator, copyright, keyword, rating, camera, exposure, GPS, rights, the full ICC profile header (manufacturer / model / copyright / rendering intent), the xmpMM asset identity/lineage, and the media container structure (chapters / tracks / duration) — on an already-emitted raster, PDF, or media artifact, discriminating the `Metadata` `read`/`write` verb over its `(MetaCarrier, payload)` shape — never a per-standard reader family, never a per-tag `get_author`/`set_title` accessor, never a per-format reader type, and never a carrier×direction case explosion: the carrier rides as the verb payload's first field, not a tag multiplier, so the op is two cases over three carriers, not six. The metadata CARRIER is the closed `MetaCarrier` axis (`RASTER` over `pyvips` + `pillow` + `lxml`, `PDF` over `pikepdf`, `MEDIA` over `av`), and each carrier's `(reader, writer, lane)` triple is ONE `CarrierPolicy` row in one `_CARRIER` table — never a `_CARRIER_TABLE` acceptor pair beside a parallel `_LANE` offload map. The EXIF/IPTC/XMP/ICC standards are field-namespace facets every carrier read folds into the ONE `MetaFacts` in a single pass through one `MetaFacts.from_logical` materialization keyed by the one `_FIELD_KEYS` logical→standard correspondence — never a dispatch axis that fragments one raster into three separate reads and never three parallel per-standard key tables. Each carrier rides its admitted owner at the runtime placement its package dictates: `pikepdf` (`worker-native`, ungated) and `av` (`cp311-native`, core) resolve in-process folded onto the `_THREAD_GATE` `CapacityLimiter`-bounded `anyio.to_thread.run_sync` (the explicit thread band, never the per-loop 40-token default) so the native qpdf/FFmpeg call never blocks the loop, while the raster cluster (`pyvips` over Forge-provisioned libvips, `pillow`/`lxml` pending their packages) crosses the `execution/lanes#LANE`-owned `WORKER_BAND` `CapacityLimiter`-bounded `anyio.to_process.run_sync` band onto the worker that imports all three at boundary scope and folds EXIF + IPTC + XMP + ICC in one crossing — the `RASTER` lane the `partial(to_process.run_sync, limiter=WORKER_BAND)` value the `CarrierPolicy` row carries, never the unbounded per-loop default process limiter. Every operation returns one `(ContentKey, MetaFacts, bytes)` evidence triple — the recovered (read) or bound (write) `MetaFacts`, the source or re-encoded payload, and the runtime `ContentIdentity.of` content key — that the consumer projects onto the `core/receipt#RECEIPT` `ArtifactReceipt.Metadata(key, carrier, fields, byte_len)` flat-scalar fact exactly as `exchange/credential#PROVENANCE` and `exchange/conformance#CONFORMANCE` return their `(ContentKey, evidence)` pair: a metadata READ recovers the full descriptive field set and a WRITE the re-encoded artifact, both richer than the four-scalar receipt, so the owner returns the evidence and the consumer mints the flat case — never a write-only `@receipted` weave that would discard the read's recovered fields and the write's bytes, never a re-minted identity, and never a producer value object crossing into the receipt owner.

## [01]-[INDEX]


## [02]-[METADATA]

- Owner: `Metadata` IS the one descriptive-metadata owner — an `expression.tagged_union` of exactly TWO cases discriminating the read/write verb directly (mirroring `exchange/credential#PROVENANCE`, never a one-field wrapper over a separate op union): `read: tuple[MetaCarrier, bytes]` and `write: tuple[MetaCarrier, bytes, WriteSpec]` (the write instruction `facts`+`bind` bundled in one named `WriteSpec` owner per the `exchange/credential#PROVENANCE`/`exchange/conformance#CONFORMANCE` `(bytes, Spec)` payload convention, never a naked positional `tuple[..., MetaFacts, MetaBind]` decoded by index) — the carrier riding as each payload's leading field and the direction the tag, so a new carrier adds zero cases (the carrier×direction product collapses to verb×payload); `MetaCarrier` the closed `StrEnum` axis (`RASTER`/`PDF`/`MEDIA`) whose `_CARRIER` row selects the acceptor-and-lane; `MetaBind` the closed write disposition (`MERGE`/`REPLACE`/`STRIP`); `MetaFacts` the ONE composite value owner every read materializes and every write consumes — seven nested frozen facets (`Descriptive` editorial + media collection tags, `Rights` creator/usage, `Capture` camera/exposure + the IPTC `Iptc4xmpExt:DigitalSourceType` content-origin/AI-provenance label, `Place` location, `History` xmpMM asset identity/lineage, `Color` the full ICC profile header, `MediaInfo` container chapters/tracks/duration) plus the `conformance` PDF/A·PDF/X status, recovering the full descriptive field set across all four standards plus the container-structure read in one shape and projected to the `core/receipt#RECEIPT` facts. The writable XMP/EXIF/media facets fold through `_flat`; the read-only `Color` ICC header and `MediaInfo` container structure are evidence-only siblings of `conformance` the writers never re-emit. `CarrierPolicy` is the carrier collapse: one frozen row carries the `(reader, writer, lane)` triple a `MetaCarrier` keys, so the operation routes by one `_CARRIER` lookup over `self.carrier`, never a per-standard reader/writer class family, never a re-discriminating `match` inside an arm, and never a `gated: bool` knob the body re-pairs to a lane the value already selects.
- Cases: `Metadata` cases — `read(carrier, payload)` and `write(carrier, payload, WriteSpec(facts, bind))` — matched by one total `match`/`case` over `self` in `_run`, the read arm collapsing to one acceptor-lookup arm and the write arm to one (the `WriteSpec` unpacked once to `spec.facts`/`spec.bind`), never a parallel reader/writer per carrier or per standard. Each carrier's reader folds its native namespace into the one `MetaFacts`: RASTER reads the libvips metadata namespace (`exif-ifd0-*`/`exif-ifd2-*`/`exif-ifd3-*` GPS, `iptc-data`, `xmp-data`, `icc-profile-data`, `orientation`, `interpretation`) through `pyvips` `Image.get`/`get_fields`, parses the `xmp-data` RDF blob through `lxml`, and resolves the full ICC profile header (description, manufacturer, model, copyright, default rendering intent) through `pillow` `ImageCms`; PDF reads the FULL Dublin-Core/XMP-Basic/XMP-MM/Adobe-PDF/Photoshop/xmpRights namespace (the `pdf:Producer` converting-application fact and the `xmpMM:DocumentID`/`InstanceID` identity included) through the `pikepdf` `PdfMetadata` mapping plus `pdfa_status`/`pdfx_status`; MEDIA reads the FFmpeg container tag set through `av` `Container.metadata` plus the container structure — `chapters()` navigation, per-stream `streams` labelling, and `duration` — folded into the `MediaInfo` facet. Every reader emits one `dict[str, object]` keyed by logical field name and folds it through the one `MetaFacts.from_logical`, so a carrier read is one logical projection plus one materialization, never a hand-built per-facet constructor per carrier.
- Auto: `_run` folds `self` through one total `match` whose two arms (read, write) lift the carrier's `CarrierPolicy` row once, so the per-carrier body is data, never a six-arm enumeration. The raster reader holds one `pyvips.Image.new_from_buffer(payload, "", access=Access.SEQUENTIAL)`, reads `Image.get_fields()` once for the present-field set, projects every EXIF logical through `_clean` (stripping the libvips `" (type, …)"` value suffix), parses the `xmp-data` blob through `_xmp` (one `lxml.etree.fromstring` over a `resolve_entities=False`/`no_network=True` parser, one namespaced `xpath` fold per `_FIELD_KEYS` xmp qname), evaluates EXIF `num/den` rationals (`28/10` → `2.8`) through `_num` for the `_RATIONAL` float logicals, folds the popped `gps_lat`/`gps_lon` degree/minute/second triples with their `gps_lat_ref`/`gps_lon_ref` hemisphere refs through `_dms` into the signed `Place.gps` decimal-degree pair, resolves the full ICC profile header through `_icc` (one `pillow` `ImageCms.ImageCmsProfile` parse, then `getProfileDescription`/`getProfileManufacturer`/`getProfileModel`/`getProfileCopyright` plus the `getDefaultIntent` ordinal mapped through `_INTENT_NAME`, each accessor's own `PyCMSError` folding to `""` so one missing tag never sinks the rest), and folds all four standards into the one `MetaFacts.from_logical`. The raster writer copies the image, clears the DESCRIPTIVE namespace (`exif-`/`iptc-`/`xmp-`/`orientation`) on `REPLACE` while PRESERVING the read-only `icc-profile-data` colour profile the `Color` facet can never re-author (only `STRIP` scrubs the profile, riding the `write_to_buffer(strip=…)` encoder flag the `MetaBind` selects, never a second remove pass), sets each EXIF logical from `_flat(facts)` through `_FIELD_KEYS[logical].exif` — re-encoding the `_RATIONAL` floats to the `num/den` form through `_rational` so the EXIF IFD type round-trips with `_num` rather than storing a plain decimal — sets the rebuilt XMP packet (`_xmp_packet` authoring the `rdf:Description` tree through `lxml.etree.SubElement`) through `Image.set("xmp-data", …)`, then `write_to_buffer` re-encodes by the `_FORMAT` loader suffix. The PDF arms open one `PdfMetadata` context, the read folding every `_FIELD_KEYS` xmp qname present in the mapping plus `pdfa_status`/`pdfx_status`, the write deleting-then-assigning under the bind policy; the MEDIA arms open the `av` container, the read folding every `_FIELD_KEYS` media tag present in `Container.metadata` AND the `_media_info` container-structure read (the `Container.duration` seconds over `_AV_TIME_BASE`, the `chapters()` `Chapter`-TypedDict navigation markers projected to the `Chapter(start, title)` rows, and the per-`streams` `Track(kind, language, title)` labelling) folded into the `MediaInfo` facet while the container is still open, the write a `demux` → `add_stream_from_template` → `mux` bitstream-copy remux that rebinds `OutputContainer.metadata` (the keyword tuple comma-joined onto the `keywords` tag the read splits) without re-encoding — the stream-copy remux carrying the source chapters and per-stream structure forward untouched. `_flat` is the one facts→logical projection every writer derives its bindings from (`structs.asdict` over the facets, dropping empties and tuples), and `from_logical` is its inverse (one `msgspec.convert(strict=False)` per facet over the logical dict, coercing the standard string values to each facet field's type), so a field reaches every standard from one declared `_FIELD_KEYS` correspondence rather than a per-standard getter/setter pair.
- Receipt: each operation folds into `MetaFacts` and returns the `(ContentKey, MetaFacts, bytes)` triple, and the consumer projects the settled `core/receipt#RECEIPT` `ArtifactReceipt.Metadata(key, carrier, fields, byte_len)` case the ARCHITECTURE `[02]-[SEAMS]` `exchange/metadata → python:artifacts/core/receipt` edge names — the carrier the `MetaCarrier` value off the op the consumer admitted, the field count the `MetaFacts.populated` tally (the recovered/bound flat scalars plus the keyword set, GPS pair, ICC-header colour evidence, and the media chapter/track counts), the byte length the returned payload — exactly as the coordinator mints `ArtifactReceipt.Credential` from the `exchange/credential#PROVENANCE` pair and `ArtifactReceipt.Verdict` from the `exchange/conformance#CONFORMANCE` pair. The write arms key the re-encoded bytes through `ContentIdentity.of` so a metadata-bound artifact carries a fresh content key the `csharp:Rasm.Persistence` store re-derives, and the read arms key the source bytes unchanged. The `core/receipt#RECEIPT` union carries the flat-scalar `Metadata` case (`metadata: tuple[ContentKey, str, int, int]`, its second slot the `carrier` discriminant) mirroring the flat-scalar `Credential`/`Media` cases, so the consumer spreads the carrier/field-count/byte-length slice onto it and the receipt owner imports no `MetaFacts` value object — the rich `MetaFacts` rides the returned triple to the reading caller, the flat case lives on the receipt page, neither minted here.
- Growth: a new metadata carrier is one `MetaCarrier` member plus one `_CARRIER` `CarrierPolicy(reader, writer, lane)` row plus its two carrier functions — zero new op cases, because the carrier is the payload field, not a tag; a new descriptive field is one field on the owning `MetaFacts` facet plus one `_FIELD_KEYS` row, the `_flat`/`from_logical` derivation reaching every standard with no per-standard table edit; a new standard a carrier supports is one more column on the `FieldKeys` row plus one fold inside that carrier's reader/writer; a new write disposition is one `MetaBind` member plus one arm in the writers; a new writable descriptive facet (the `History` xmpMM identity row is exactly this) is one nested frozen `Struct` on `MetaFacts` plus one `from_logical` convert leg plus its rows in `_flat`'s facet tuple; a new read-only structured facet (the `MediaInfo` chapters/tracks read is exactly this) is one nested frozen `Struct` plus its reader population plus one `from_logical` keyword and one `populated` term, never a `_flat` write leg; a new ICC-header fact is one `Color` field plus one `getProfile*` read inside `_icc`; zero new surface.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from collections.abc import Awaitable, Callable, Mapping
from dataclasses import dataclass
from enum import StrEnum
from functools import partial
from io import BytesIO
from typing import Final, Literal, Self, assert_never

from anyio import CapacityLimiter, to_process, to_thread
from builtins import frozendict
from expression import case, tag, tagged_union
from msgspec import Struct, convert, structs

from rasm.runtime.content_identity import ContentIdentity, ContentKey
from rasm.runtime.faults import RuntimeRail, async_boundary
from rasm.runtime.lanes import WORKER_BAND

lazy import pyvips             # libvips raster engine, to_process worker band
lazy from PIL import ImageCms  # pillow ICC header reader, to_process worker band
lazy from lxml import etree    # libxml2 XMP read/author, to_process worker band
lazy import pikepdf            # qpdf PDF metadata, to_thread in-process arm
lazy import av                 # FFmpeg container read/remux, to_thread in-process arm

# --- [TYPES] ----------------------------------------------------------------------------
type MetaReader = Callable[[bytes], "MetaFacts"]
type MetaWriter = Callable[[bytes, "MetaFacts", "MetaBind"], bytes]
type Offload = Callable[..., Awaitable[object]]


class MetaCarrier(StrEnum):
    RASTER = "raster"
    PDF = "pdf"
    MEDIA = "media"


class MetaBind(StrEnum):
    MERGE = "merge"      # set provided fields, keep the rest
    REPLACE = "replace"  # clear the metadata namespace, then set provided
    STRIP = "strip"      # clear the metadata namespace, set nothing


# --- [CONSTANTS] ------------------------------------------------------------------------
_RDF_NS: Final[str] = "http://www.w3.org/1999/02/22-rdf-syntax-ns#"
_XMP_NS: Final[frozendict[str, str]] = frozendict({
    "dc": "http://purl.org/dc/elements/1.1/",
    "xmp": "http://ns.adobe.com/xap/1.0/",
    "xmpMM": "http://ns.adobe.com/xap/1.0/mm/",
    "photoshop": "http://ns.adobe.com/photoshop/1.0/",
    "xmpRights": "http://ns.adobe.com/xap/1.0/rights/",
    "Iptc4xmpCore": "http://iptc.org/std/Iptc4xmpCore/1.0/xmlns/",
    "Iptc4xmpExt": "http://iptc.org/std/Iptc4xmpExt/2008-02-29/",
    "pdf": "http://ns.adobe.com/pdf/1.3/",
})

# float logicals whose EXIF value is a `num/den` rational `_num` normalizes before materialization;
# every other numeric field is a plain numeral `msgspec.convert(strict=False)` coerces directly.
_RATIONAL: Final[frozenset[str]] = frozenset({"f_number", "focal_length", "aperture", "exposure_bias", "altitude", "gps_direction"})

# ICC default-rendering-intent ordinal -> token (the liblcms2/`ImageCms.getDefaultIntent` 0-3 scale).
_INTENT_NAME: Final[frozendict[int, str]] = frozendict({0: "perceptual", 1: "relative", 2: "saturation", 3: "absolute"})
_AV_TIME_BASE: Final[int] = 1_000_000  # `Container.duration` is AV_TIME_BASE microseconds; divide for seconds

_FORMAT: Final[frozendict[str, str]] = frozendict({
    "jpegload": ".jpg", "pngload": ".png", "tiffload": ".tif", "webpload": ".webp", "heifload": ".heif",
    "gifload": ".gif", "jp2kload": ".jp2", "jxlload": ".jxl",
})


# --- [MODELS] ---------------------------------------------------------------------------
class FieldKeys(Struct, frozen=True, gc=False):  # the per-logical standard correspondence row
    xmp: str = ""
    exif: str = ""
    media: str = ""


class Descriptive(Struct, frozen=True):  # editorial: dc:* / xmp:* / photoshop:* / Iptc4xmpCore:* / EXIF ImageDescription / FFmpeg container tags
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
    album: str = ""         # FFmpeg `album` container tag — the descriptive collection an audio/video asset belongs to
    album_artist: str = ""  # FFmpeg `album_artist` container tag
    composer: str = ""      # FFmpeg `composer` container tag


class Rights(Struct, frozen=True):  # creator + rights: IPTC by-line/credit / xmpRights / dc:rights+publisher / EXIF Artist
    creator: str = ""
    credit: str = ""
    source: str = ""
    copyright: str = ""
    usage_terms: str = ""
    web_statement: str = ""
    publisher: str = ""
    marked: bool | None = None


class Capture(Struct, frozen=True):  # camera/exposure: EXIF ifd0/ifd2 + xmp:Create/ModifyDate
    make: str = ""
    model: str = ""
    lens: str = ""
    lens_make: str = ""
    software: str = ""      # xmp:CreatorTool — the application that authored the content
    producer: str = ""      # pdf:Producer — the application that converted/emitted the PDF (distinct from CreatorTool)
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


class Place(Struct, frozen=True):  # location: EXIF GPS ifd3 + photoshop/Iptc4xmpCore
    gps: tuple[float, float] | None = None  # read-derived from the EXIF DMS arrays via `_dms`; location WRITE rides the textual city/state/country/sublocation and the scalar altitude/direction
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
    document_id: str = ""    # xmpMM:DocumentID — the stable cross-rendition asset id
    instance_id: str = ""    # xmpMM:InstanceID — this specific rendition's id
    original_id: str = ""    # xmpMM:OriginalDocumentID — the ancestor this asset derives from


class Color(Struct, frozen=True):  # colour evidence: libvips interpretation + the ICC profile header (read-only)
    space: str = ""
    icc_name: str = ""
    icc_present: bool = False
    render_intent: str = ""   # ICC default rendering intent (perceptual/relative/saturation/absolute) via `getDefaultIntent`
    icc_maker: str = ""       # ICC profile manufacturer tag via `getProfileManufacturer`
    icc_model: str = ""       # ICC profile model tag via `getProfileModel` (distinct from camera `Capture.model`)
    icc_copyright: str = ""   # ICC profile copyright tag via `getProfileCopyright` (distinct from asset `Rights.copyright`)


class Chapter(Struct, frozen=True, gc=False):  # one media navigation chapter — the descriptive timeline marker `av` `Container.chapters()` yields
    start: float = 0.0  # chapter start in seconds (the `start * time_base` fold over the `Chapter` TypedDict)
    title: str = ""


class Track(Struct, frozen=True, gc=False):  # one container stream's descriptive labelling (NOT its codec/encode params — those are media/video's)
    kind: str = ""      # `Stream.type` — video/audio/subtitle/data/attachment
    language: str = ""  # `Stream.language`
    title: str = ""     # `Stream.metadata["title"]`


class MediaInfo(Struct, frozen=True):  # media container structure read-only (av `streams`/`chapters()`/`duration`) — the media-carrier sibling of `Color`/`conformance`
    duration: float = 0.0
    chapters: tuple[Chapter, ...] = ()
    tracks: tuple[Track, ...] = ()


class MetaFacts(Struct, frozen=True):
    descriptive: Descriptive = Descriptive()
    rights: Rights = Rights()
    capture: Capture = Capture()
    place: Place = Place()
    history: History = History()       # xmpMM identity/lineage — a writable XMP facet, folded by `_flat`
    color: Color = Color()             # ICC header evidence — read-only, excluded from `_flat`
    media: MediaInfo = MediaInfo()     # media container structure — read-only, populated only by `_read_media`
    conformance: str = ""              # pikepdf pdfa_status / pdfx_status

    @classmethod
    def from_logical(cls, flat: Mapping[str, object], /, *, media: MediaInfo = MediaInfo()) -> Self:
        # one logical dict -> the flat facets via per-facet `convert(strict=False)`; unknown keys fall
        # away per facet, the standard string values coerce to each field's type, `marked` lifts to bool.
        # `media` rides in as the already-materialized structured facet (chapters/tracks are not logical keys).
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
            media=media,
            conformance=str(data.get("conformance", "")),
        )

    @property
    def populated(self) -> int:
        # flat scalar tally + the non-scalar/read-only evidence each facet carries off the `_flat` path
        color = sum(1 for value in structs.asdict(self.color).values() if value)
        return len(_flat(self)) + bool(self.conformance) + bool(self.descriptive.keywords) + bool(self.place.gps) + color + len(self.media.chapters) + len(self.media.tracks) + bool(self.media.duration)


@dataclass(frozen=True, slots=True, kw_only=True)
class CarrierPolicy:  # the `(reader, writer, lane)` triple one `MetaCarrier` keys, the carrier collapse
    reader: MetaReader
    writer: MetaWriter
    lane: Offload


# the typed write instruction the `write` case carries beside its carrier+artifact, mirroring the
# `exchange/credential#PROVENANCE`/`exchange/conformance#CONFORMANCE` `(bytes, Spec)` payload convention:
# `facts`+`bind` ride one named owner (the growth site a new write knob lands on) rather than a naked
# positional `tuple[..., MetaFacts, MetaBind]` decoded by index. The carrier stays the leading payload
# field both verbs share so `self.carrier` reads it uniformly for the one `_CARRIER` dispatch.
class WriteSpec(Struct, frozen=True):
    facts: MetaFacts
    bind: MetaBind = MetaBind.MERGE


# `Metadata` IS the closed union: the two read/write cases, the carrier-polymorphic mints, the carrier
# property, the async entry, and the dispatch all fold onto one `tagged_union` (mirroring
# `exchange/credential#PROVENANCE`), never a one-field `Metadata` wrapper over a separate `MetaOp`.
@tagged_union(frozen=True)
class Metadata:
    tag: Literal["read", "write"] = tag()
    read: tuple[MetaCarrier, bytes] = case()
    write: tuple[MetaCarrier, bytes, WriteSpec] = case()

    @classmethod
    def Read(cls, carrier: MetaCarrier, payload: bytes, /) -> Self:
        return cls(read=(carrier, payload))

    @classmethod
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
    # and tuples (keywords/gps ride their own writer arms) so each writer derives bindings from one map.
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


def _clean(value: str) -> str:
    return value.split(" (")[0].strip()  # libvips appends a " (type, N bytes)" suffix to EXIF string values


def _num(value: str) -> float:
    # an EXIF rational is `num/den`; evaluate it (numerator over denominator), never the numerator
    # alone (`28/10` is f/2.8, not 28). A bare decimal carries no slash and returns directly.
    num, slash, den = value.partition("/")
    keep = lambda text: "".join(ch for ch in text if ch.isdigit() or ch in ".-")
    top, bottom = keep(num), keep(den)
    if slash and top.strip(".-") and bottom.strip(".-") and (divisor := float(bottom)):
        return float(top) / divisor  # a `0/0` GPS-unknown rational falls through to 0.0, never raising ZeroDivisionError
    return float(top) if top.strip(".-") else 0.0


def _rational(value: float) -> str:
    # the EXIF write inverse of `_num`: a float binds back to the `num/den` rational the libvips EXIF
    # writer types, where a plain decimal stores under the wrong IFD type and breaks the `_num` re-read.
    return f"{round(value * 1000)}/1000"


def _dms(value: str, ref: str) -> float | None:
    # EXIF GPS is a degree/minute/second rational triple plus an N/S/E/W hemisphere ref; fold the
    # components to signed decimal degrees, never the bare first-component degrees the old read lost.
    parts = [_num(token) for token in value.replace(",", " ").split() if token]
    if not parts:
        return None
    degrees = sum(part / 60**index for index, part in enumerate(parts[:3]))
    return -degrees if ref.strip().upper().startswith(("S", "W")) else degrees  # N/S/E/W or North/South/...


def _xmp(blob: bytes) -> dict[str, str]:
    if not blob:
        return {}
    root = etree.fromstring(blob, parser=etree.XMLParser(resolve_entities=False, no_network=True, recover=True))
    ns = dict(_XMP_NS)
    # XMP simple props ride as `rdf:Description` attributes; Alt/Bag values ride as nested `rdf:li` text.
    text = lambda qname: " ; ".join(t.strip() for t in root.xpath(f"//{qname}//text()[normalize-space()] | //@{qname}", namespaces=ns) if t.strip())
    return {logical: value for logical, keys in _FIELD_KEYS.items() if keys.xmp and (value := text(keys.xmp))}


def _xmp_packet(facts: MetaFacts) -> bytes:
    fields = {keys.xmp: value for logical, value in _flat(facts).items() if (keys := _FIELD_KEYS.get(logical)) and keys.xmp}
    if not fields and not facts.descriptive.keywords:
        return b""
    rdf = etree.Element(f"{{{_RDF_NS}}}RDF", nsmap={"rdf": _RDF_NS, **dict(_XMP_NS)})
    description = etree.SubElement(rdf, f"{{{_RDF_NS}}}Description")
    for qname, value in fields.items():
        prefix, local = qname.split(":")
        etree.SubElement(description, f"{{{_XMP_NS[prefix]}}}{local}").text = value
    if facts.descriptive.keywords:
        bag = etree.SubElement(etree.SubElement(description, f"{{{_XMP_NS['dc']}}}subject"), f"{{{_RDF_NS}}}Bag")
        for keyword in facts.descriptive.keywords:
            etree.SubElement(bag, f"{{{_RDF_NS}}}li").text = keyword
    return etree.tostring(rdf, xml_declaration=True, encoding="utf-8")


def _icc(blob: bytes) -> dict[str, object]:
    # the ICC profile header read: description + manufacturer/model/copyright provenance + default
    # rendering intent, the full header concept `Color` carries beyond a bare name.
    try:  # Exemption: ImageCms raises on a malformed ICC blob; the header is best-effort descriptive evidence.
        profile = ImageCms.ImageCmsProfile(BytesIO(blob))
    except (OSError, ValueError, ImageCms.PyCMSError):
        return {}

    def field(reader: Callable[[ImageCms.ImageCmsProfile], str], /) -> str:
        try:  # a profile missing one tag raises PyCMSError on that accessor alone; the rest still read
            return str(reader(profile)).strip()
        except ImageCms.PyCMSError:
            return ""

    try:
        intent = ImageCms.getDefaultIntent(profile)
    except ImageCms.PyCMSError:
        intent = -1
    return {
        "icc_name": field(ImageCms.getProfileDescription),
        "icc_maker": field(ImageCms.getProfileManufacturer),
        "icc_model": field(ImageCms.getProfileModel),
        "icc_copyright": field(ImageCms.getProfileCopyright),
        "render_intent": _INTENT_NAME.get(intent, ""),
    }


# --- [RASTER_CARRIER] -------------------------------------------------------------------
# pyvips + pillow + lxml fold EXIF/IPTC/XMP/ICC in one gated-companion `to_process` crossing.
def _read_raster(payload: bytes) -> MetaFacts:
    image = pyvips.Image.new_from_buffer(payload, "", access=pyvips.Access.SEQUENTIAL)
    present = frozenset(image.get_fields())
    read = lambda field: _clean(str(image.get(field))) if field in present else ""
    exif = {logical: value for logical, keys in _FIELD_KEYS.items() if keys.exif and (value := read(keys.exif))}
    flat: dict[str, object] = {**exif, **_xmp(bytes(image.get("xmp-data")) if "xmp-data" in present else b"")}
    flat.update({logical: _num(str(flat[logical])) for logical in _RATIONAL if logical in flat})
    lat = _dms(str(flat.pop("gps_lat", "")), str(flat.pop("gps_lat_ref", "")))
    lon = _dms(str(flat.pop("gps_lon", "")), str(flat.pop("gps_lon_ref", "")))
    flat["gps"] = (lat, lon) if lat is not None and lon is not None else None
    flat["keywords"] = tuple(k.strip() for k in str(flat.get("keywords", "")).split(";") if k.strip())
    flat["space"] = str(image.get("interpretation"))
    flat["icc_present"] = "icc-profile-data" in present
    if "icc-profile-data" in present:
        flat.update(_icc(bytes(image.get_value("icc-profile-data"))))  # the full ICC header, not a bare name
    return MetaFacts.from_logical(flat)


def _write_raster(payload: bytes, facts: MetaFacts, bind: MetaBind) -> bytes:
    image = pyvips.Image.new_from_buffer(payload, "", access=pyvips.Access.SEQUENTIAL).copy()
    if bind is MetaBind.REPLACE:  # STRIP scrubs everything through the `strip=` write flag below; REPLACE clears the DESCRIPTIVE namespace then re-sets
        for name in image.get_fields():
            # the `icc-profile-data` colour profile is preserved — `Color` is read-only evidence the writer can never restore,
            # so a descriptive REPLACE that stripped it would irrecoverably damage colour; only STRIP scrubs the profile.
            if name.startswith(("exif-", "iptc-", "xmp-")) or name == "orientation":
                image.remove(name)
    if bind is not MetaBind.STRIP:
        for logical, value in _flat(facts).items():
            if (keys := _FIELD_KEYS.get(logical)) and keys.exif:
                image.set(keys.exif, _rational(float(value)) if logical in _RATIONAL else value)
        if packet := _xmp_packet(facts):
            image.set("xmp-data", packet)
    return image.write_to_buffer(_FORMAT.get(str(image.get("vips-loader")), ".tif"), strip=bind is MetaBind.STRIP)


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
    tracks = tuple(
        Track(kind=stream.type, language=stream.language or "", title=stream.metadata.get("title", ""))
        for stream in container.streams
    )
    return MediaInfo(duration=duration, chapters=chapters, tracks=tracks)


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
# the one logical -> (xmp, exif, media) correspondence; `from_logical`/`_flat` derive every read and
# write from it, so a new field is one row and a new standard a carrier supports is one column.
# `gps_lat`/`gps_lon` + their `gps_lat_ref`/`gps_lon_ref` hemisphere refs are EXIF read sources the
# raster reader folds through `_dms` into the signed `Place.gps` decimal-degree pair.
_FIELD_KEYS: Final[frozendict[str, FieldKeys]] = frozendict({
    "title": FieldKeys(xmp="dc:title", exif="exif-ifd0-ImageDescription", media="title"),
    "headline": FieldKeys(xmp="photoshop:Headline"),
    "caption": FieldKeys(xmp="dc:description", media="comment"),
    "keywords": FieldKeys(xmp="dc:subject", media="keywords"),
    "rating": FieldKeys(xmp="xmp:Rating"),
    "label": FieldKeys(xmp="xmp:Label"),
    "category": FieldKeys(xmp="photoshop:Category"),
    "genre": FieldKeys(xmp="Iptc4xmpCore:IntellectualGenre", media="genre"),
    "language": FieldKeys(xmp="dc:language", media="language"),
    "instructions": FieldKeys(xmp="photoshop:Instructions"),
    "description_writer": FieldKeys(xmp="photoshop:CaptionWriter"),
    "album": FieldKeys(media="album"),
    "album_artist": FieldKeys(media="album_artist"),
    "composer": FieldKeys(media="composer"),
    "creator": FieldKeys(xmp="dc:creator", exif="exif-ifd0-Artist", media="artist"),
    "credit": FieldKeys(xmp="photoshop:Credit"),
    "source": FieldKeys(xmp="photoshop:Source"),
    "copyright": FieldKeys(xmp="dc:rights", exif="exif-ifd0-Copyright", media="copyright"),
    "usage_terms": FieldKeys(xmp="xmpRights:UsageTerms"),
    "web_statement": FieldKeys(xmp="xmpRights:WebStatement"),
    "publisher": FieldKeys(xmp="dc:publisher", media="publisher"),
    "marked": FieldKeys(xmp="xmpRights:Marked"),
    "make": FieldKeys(exif="exif-ifd0-Make"),
    "model": FieldKeys(exif="exif-ifd0-Model"),
    "lens": FieldKeys(exif="exif-ifd2-LensModel"),
    "lens_make": FieldKeys(exif="exif-ifd2-LensMake"),
    "software": FieldKeys(xmp="xmp:CreatorTool", exif="exif-ifd0-Software", media="encoder"),
    "producer": FieldKeys(xmp="pdf:Producer"),
    "digital_source_type": FieldKeys(xmp="Iptc4xmpExt:DigitalSourceType"),
    "orientation": FieldKeys(exif="orientation"),
    "exposure_time": FieldKeys(exif="exif-ifd2-ExposureTime"),
    "f_number": FieldKeys(exif="exif-ifd2-FNumber"),
    "aperture": FieldKeys(exif="exif-ifd2-ApertureValue"),
    "iso": FieldKeys(exif="exif-ifd2-ISOSpeedRatings"),
    "focal_length": FieldKeys(exif="exif-ifd2-FocalLength"),
    "exposure_bias": FieldKeys(exif="exif-ifd2-ExposureBiasValue"),
    "exposure_program": FieldKeys(exif="exif-ifd2-ExposureProgram"),
    "metering_mode": FieldKeys(exif="exif-ifd2-MeteringMode"),
    "flash": FieldKeys(exif="exif-ifd2-Flash"),
    "white_balance": FieldKeys(exif="exif-ifd2-WhiteBalance"),
    "serial_number": FieldKeys(exif="exif-ifd2-BodySerialNumber"),
    "created": FieldKeys(xmp="xmp:CreateDate", exif="exif-ifd2-DateTimeOriginal", media="creation_time"),
    "modified": FieldKeys(xmp="xmp:ModifyDate", exif="exif-ifd0-DateTime"),
    "gps_lat": FieldKeys(exif="exif-ifd3-GPSLatitude"),
    "gps_lat_ref": FieldKeys(exif="exif-ifd3-GPSLatitudeRef"),
    "gps_lon": FieldKeys(exif="exif-ifd3-GPSLongitude"),
    "gps_lon_ref": FieldKeys(exif="exif-ifd3-GPSLongitudeRef"),
    "altitude": FieldKeys(exif="exif-ifd3-GPSAltitude"),
    "gps_direction": FieldKeys(exif="exif-ifd3-GPSImgDirection"),
    "gps_timestamp": FieldKeys(exif="exif-ifd3-GPSDateStamp"),
    "map_datum": FieldKeys(exif="exif-ifd3-GPSMapDatum"),
    "city": FieldKeys(xmp="photoshop:City"),
    "state": FieldKeys(xmp="photoshop:State"),
    "country": FieldKeys(xmp="photoshop:Country"),
    "country_code": FieldKeys(xmp="Iptc4xmpCore:CountryCode"),
    "sublocation": FieldKeys(xmp="Iptc4xmpCore:Location"),
    "document_id": FieldKeys(xmp="xmpMM:DocumentID"),
    "instance_id": FieldKeys(xmp="xmpMM:InstanceID"),
    "original_id": FieldKeys(xmp="xmpMM:OriginalDocumentID"),
})
# the in-process thread bound for the GIL-releasing pikepdf/av arms — explicit, never the per-loop
# 40-token default; the gated RASTER lane rides the shared `execution/lanes#LANE` `WORKER_BAND` instead.
_THREAD_GATE: Final[CapacityLimiter] = CapacityLimiter(8)
# each `CarrierPolicy` pre-folds its limiter so the dispatch never re-pairs it: RASTER crosses the shared
# `WORKER_BAND`-bounded `to_process` worker, PDF/MEDIA the `_THREAD_GATE`-bounded in-process `to_thread`.
_CARRIER: Final[frozendict[MetaCarrier, CarrierPolicy]] = frozendict({
    MetaCarrier.RASTER: CarrierPolicy(reader=_read_raster, writer=_write_raster, lane=partial(to_process.run_sync, limiter=WORKER_BAND)),
    MetaCarrier.PDF: CarrierPolicy(reader=_read_pdf, writer=_write_pdf, lane=partial(to_thread.run_sync, limiter=_THREAD_GATE)),
    MetaCarrier.MEDIA: CarrierPolicy(reader=_read_media, writer=_write_media, lane=partial(to_thread.run_sync, limiter=_THREAD_GATE)),
})
```

## [03]-[RESEARCH]

- [CARRIER_DISCRIMINANT] [RESOLVED]: the owner discriminates by the `read`/`write` VERB over the `(MetaCarrier, payload)` shape, not by standard and not by a carrier×direction case product — a single raster carries EXIF + IPTC + XMP + ICC simultaneously, so a per-standard `ReadExif`/`ReadIptc`/`ReadXmp` op family fragments one carrier into three reads where one pass over `pyvips.Image.get_fields()` + the `lxml` XMP parse recovers all four into the one `MetaFacts`, and a six-case `read_raster`/`write_raster`/`read_pdf`/… union is the carrier×direction explosion the two-case `Metadata` tagged_union collapses by riding the carrier as the verb payload's first field. The standards are field-namespace facets the carrier readers fold (`Capture`/`Place` from EXIF, `Descriptive`/`Rights` from XMP/IPTC, `Color` from ICC, `conformance` from PDF/A·PDF/X) through the one `_FIELD_KEYS` correspondence and the one `MetaFacts.from_logical` materialization, and the `MetaBind` (`MERGE`/`REPLACE`/`STRIP`) write policy carries the disposition as a behaviour-bearing value rather than a `clear: bool` knob, the `STRIP` arm the privacy-scrub (`pyvips.Image.remove` over the metadata namespace, `pikepdf` `del meta[key]`, `av` empty `OutputContainer.metadata`).
- [FIELD_CORRESPONDENCE] [RESOLVED]: the three former per-standard tables (`_XMP_KEYS`/`_EXIF_FIELDS`/`_MEDIA_TAGS`) collapse into one primary `_FIELD_KEYS: frozendict[str, FieldKeys]` keyed by logical field name, each `FieldKeys(xmp, exif, media)` row carrying the three standard spellings of one field, per the DERIVED_LOGIC one-primary-correspondence law — `_xmp`/`_read_pdf`/`_read_media` read their own column, `_flat`/`_xmp_packet`/the writers write their own column, and the `_CARRIER` row collapses the former `_CARRIER_TABLE` acceptor pair and `_LANE` offload map into one `CarrierPolicy(reader, writer, lane)` per the algorithms.md ROUTE_UNION "one policy row carries the whole axis bundle" law. `MetaFacts.from_logical` (one `msgspec.convert(strict=False)` per facet, verified to coerce the standard string values to each facet field's type and drop foreign keys) is the one materializer all three readers fold into, and `_flat` (`structs.asdict` over the facets) is its inverse the three writers derive from — the logical name is the facet field name, so a new field needs no per-standard table edit. The IPTC Extension `Iptc4xmpExt:DigitalSourceType` content-origin field (the plain-XMP AI-provenance label over the `digitalCapture`/`algorithmicMedia`/`trainedAlgorithmicMedia`/`composite` IPTC NewsCodes IRIs) is exactly this one-row growth on `Capture` over the added `Iptc4xmpExt` `_XMP_NS` namespace, riding the existing `_xmp` lxml parse and `pikepdf` `PdfMetadata` read with NO new package member — closing the IPTC-Extension half of the page's claimed IPTC scope, and distinct from the SIGNED `exchange/credential#CREDENTIAL` C2PA `DigitalSource` assertion (this is the unsigned descriptive field a DAM consumer reads beside, not instead of, the credential manifest).
- [META_RECEIPT_CASE] [RESOLVED]: the descriptive-metadata operation returns the rich `(ContentKey, MetaFacts, bytes)` evidence triple, and the consumer projects the ARCHITECTURE `[02]-[SEAMS]` `exchange/metadata → python:artifacts/core/receipt` edge — the carrier, the populated descriptive-field count, and the payload byte length onto the shared `core/receipt#RECEIPT` `ArtifactReceipt.Metadata` case `metadata: tuple[ContentKey, str, int, int]` mirroring the flat-scalar `Credential`/`Media` cases, so the receipt owner imports no `MetaFacts` value object. `_emit` carries NO `@receipted` weave — exactly as `exchange/credential#PROVENANCE` `_emit` carries `@stamina.retry` alone and `exchange/conformance#CONFORMANCE` `close` carries none, both returning the rich `(ContentKey, evidence)` pair the consumer mints the receipt from — because a metadata READ recovers the full `MetaFacts` (the descriptive field set the reading caller needs) and a WRITE the re-encoded artifact bytes, both exceeding the four-scalar summary. The prior `@receipted(Redaction.STRUCTURAL)` weave was the deleted form on two counts: it discarded the read's recovered fields and the write's re-encoded bytes (a write-only producer's pattern wrongly applied to a read/write owner), and it cited the phantom `Redaction.STRUCTURAL` the runtime `observability/receipts#RECEIPT` `Redaction` owner — a `Struct(classified, salt)` with no named policy instances — does not define. `_emit` keys the (source or re-encoded) payload through `ContentIdentity.of` and returns `(key, facts, payload)`; the consumer spreads the carrier value (off the `Metadata` it admitted), `facts.populated`, and `len(payload)` onto `ArtifactReceipt.Metadata` exactly as it mints `ArtifactReceipt.Credential` from the credential pair and `ArtifactReceipt.Verdict` from the conformance pair; the write arms key the re-encoded bytes so a metadata-bound artifact carries a fresh content key the `csharp:Rasm.Persistence` store re-derives, the read arms key the source bytes, and the metadata owner re-mints no canonical content key (the runtime `content_identity` owns it).
