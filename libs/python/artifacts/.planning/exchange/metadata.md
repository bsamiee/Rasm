# [PY_ARTIFACTS_METADATA]

The descriptive-metadata read/write owner at the exchange boundary. `Metadata` is ONE owner binding and recovering the EXIF / IPTC / XMP / ICC descriptive-metadata standards — title, creator, copyright, keyword, rating, camera, exposure, GPS, rights, and colour-profile fields — on an already-emitted raster, PDF, or media artifact, discriminating the `Metadata` `read`/`write` verb over its `(MetaCarrier, payload)` shape — never a per-standard reader family, never a per-tag `get_author`/`set_title` accessor, never a per-format reader type, and never a carrier×direction case explosion: the carrier rides as the verb payload's first field, not a tag multiplier, so the op is two cases over three carriers, not six. The metadata CARRIER is the closed `MetaCarrier` axis (`RASTER` over `pyvips` + `pillow` + `lxml`, `PDF` over `pikepdf`, `MEDIA` over `av`), and each carrier's `(reader, writer, lane)` triple is ONE `CarrierPolicy` row in one `_CARRIER` table — never a `_CARRIER_TABLE` acceptor pair beside a parallel `_LANE` offload map. The EXIF/IPTC/XMP/ICC standards are field-namespace facets every carrier read folds into the ONE `MetaFacts` in a single pass through one `MetaFacts.from_logical` materialization keyed by the one `_FIELD_KEYS` logical→standard correspondence — never a dispatch axis that fragments one raster into three separate reads and never three parallel per-standard key tables. Each carrier rides its admitted owner at the runtime placement its wheel dictates: `pikepdf` (`cp314-abi3`, ungated) and `av` (`cp311-abi3`, cp315-clean) resolve in-process folded onto `anyio.to_thread.run_sync` so the native qpdf/FFmpeg call never blocks the loop, while the raster cluster (`pyvips` over Forge-provisioned libvips, `pillow`/`lxml` pending their cp315 wheels) crosses the `execution/lanes#LANE`-owned `GATED_BAND` `CapacityLimiter`-bounded `anyio.to_process.run_sync` band onto the companion worker that imports all three at boundary scope and folds EXIF + IPTC + XMP + ICC in one crossing — the `RASTER` lane the `partial(to_process.run_sync, limiter=GATED_BAND)` value the `CarrierPolicy` row carries, never the unbounded per-loop default process limiter. Every operation returns one `(ContentKey, MetaFacts, bytes)` evidence triple — the recovered (read) or bound (write) `MetaFacts`, the source or re-encoded payload, and the runtime `ContentIdentity.of` content key — that the consumer projects onto the `core/receipt#RECEIPT` `ArtifactReceipt.Metadata(key, carrier, fields, byte_len)` flat-scalar fact exactly as `exchange/credential#PROVENANCE` and `exchange/conformance#CONFORMANCE` return their `(ContentKey, evidence)` pair: a metadata READ recovers the full descriptive field set and a WRITE the re-encoded artifact, both richer than the four-scalar receipt, so the owner returns the evidence and the consumer mints the flat case — never a write-only `@receipted` weave that would discard the read's recovered fields and the write's bytes, never a re-minted identity, and never a producer value object crossing into the receipt owner.

## [01]-[INDEX]

- [01]-[METADATA]: descriptive-metadata read/write owner over RASTER (`pyvips` `Image.get`/`set`/`get_fields`/`remove` + `pillow` `ImageCms` + `lxml` XMP-RDF parse), PDF (`pikepdf` `Pdf.open_metadata()` `PdfMetadata` mapping + `pdfa_status`/`pdfx_status`), and MEDIA (`av` `Container.metadata` + `add_stream_from_template` remux) — the two-case `Metadata` `read`/`write` tagged_union over the `MetaCarrier` payload field, folding into one composite `MetaFacts` (`Descriptive`/`Rights`/`Capture`/`Place`/`Color`) through one `MetaFacts.from_logical` materialization keyed by the one `_FIELD_KEYS` logical→`(xmp, exif, media)` correspondence, the one `_CARRIER` `CarrierPolicy(reader, writer, lane)` dispatch row, and the `(ContentKey, MetaFacts, bytes)` evidence triple the consumer projects onto the `ArtifactReceipt.Metadata` facts; the in-process PDF/MEDIA arms ride `to_thread`, the gated raster cluster crosses the `execution/lanes#LANE` `GATED_BAND`-bounded `to_process` companion-worker band.

## [02]-[METADATA]

- Owner: `Metadata` IS the one descriptive-metadata owner — an `expression.tagged_union` of exactly TWO cases discriminating the read/write verb directly (mirroring `exchange/credential#PROVENANCE`, never a one-field wrapper over a separate op union): `read: tuple[MetaCarrier, bytes]` and `write: tuple[MetaCarrier, bytes, MetaFacts, MetaBind]` — the carrier riding as each payload's first field and the direction the tag, so a new carrier adds zero cases (the carrier×direction product collapses to verb×payload); `MetaCarrier` the closed `StrEnum` axis (`RASTER`/`PDF`/`MEDIA`) whose `_CARRIER` row selects the acceptor-and-lane; `MetaBind` the closed write disposition (`MERGE`/`REPLACE`/`STRIP`); `MetaFacts` the ONE composite value owner every read materializes and every write consumes — five nested frozen facets (`Descriptive` editorial, `Rights` creator/usage, `Capture` camera/exposure, `Place` location, `Color` ICC/colour-space) plus the `conformance` PDF/A·PDF/X status, recovering the full descriptive field set across all four standards in one shape and projected to the `core/receipt#RECEIPT` facts. `CarrierPolicy` is the carrier collapse: one frozen row carries the `(reader, writer, lane)` triple a `MetaCarrier` keys, so the operation routes by one `_CARRIER` lookup over `self.carrier`, never a per-standard reader/writer class family, never a re-discriminating `match` inside an arm, and never a `gated: bool` knob the body re-pairs to a lane the value already selects.
- Cases: `Metadata` cases — `read(carrier, payload)` and `write(carrier, payload, facts, bind)` — matched by one total `match`/`case` over `self` in `_run`, the read arm collapsing to one acceptor-lookup arm and the write arm to one, never a parallel reader/writer per carrier or per standard. Each carrier's reader folds its native namespace into the one `MetaFacts`: RASTER reads the libvips metadata namespace (`exif-ifd0-*`/`exif-ifd2-*`/`exif-ifd3-*` GPS, `iptc-data`, `xmp-data`, `icc-profile-data`, `orientation`, `interpretation`) through `pyvips` `Image.get`/`get_fields`, parses the `xmp-data` RDF blob through `lxml`, and resolves the ICC profile name through `pillow` `ImageCms`; PDF reads the FULL Dublin-Core/XMP-Basic/Photoshop/xmpRights namespace through the `pikepdf` `PdfMetadata` mapping plus `pdfa_status`/`pdfx_status`; MEDIA reads the FFmpeg container tag set through `av` `Container.metadata`. Every reader emits one `dict[str, object]` keyed by logical field name and folds it through the one `MetaFacts.from_logical`, so a carrier read is one logical projection plus one materialization, never a hand-built five-facet constructor per carrier.
- Entry: `Metadata.of` is `async` over the runtime `async_boundary` and returns one `RuntimeRail[tuple[ContentKey, MetaFacts, bytes]]`; `_emit` is the thin core that dispatches `_run`, keys the (source or re-encoded) bytes through `ContentIdentity.of`, and returns the `(key, facts, payload)` evidence triple — carrying NO `@receipted` weave, exactly as `exchange/credential#PROVENANCE` returns its `(ContentKey, CredentialEvidence)` pair and `exchange/conformance#CONFORMANCE` `close` its `(ContentKey, ConformanceVerdict)` pair, because a metadata READ recovers the full `MetaFacts` (the descriptive field set the reading caller needs) and a WRITE the re-encoded bytes — both richer than the four-scalar receipt, so the owner returns the evidence and the consumer mints the flat `ArtifactReceipt.Metadata`, never the write-only producer's `@receipted`-harvested `ArtifactReceipt` return that discards the recovered fields and the re-encoded artifact. `Metadata.Read`/`Metadata.Write` are the carrier-polymorphic mints (each constructing the one `Metadata` case with the carrier as its payload field), so the modality is the carrier value the mint carries, never a name-suffix family; `_run` reads one `policy = _CARRIER[self.carrier]` row and dispatches `await policy.lane(policy.reader, payload)` for the read arm and `await policy.lane(policy.writer, payload, facts, bind)` for the write arm — the PDF/MEDIA carriers' `lane` is `to_thread.run_sync` (in-process `pikepdf`/`av`), the RASTER carrier's `lane` is `partial(to_process.run_sync, limiter=GATED_BAND)` (the gated companion worker importing `pyvips`/`pillow`/`lxml` at boundary scope, bounded by the shared `execution/lanes#LANE` `GATED_BAND` `CapacityLimiter` the `detect`/`graphic/raster/io` companion band shares, never the per-loop default process limiter), the lane a `CarrierPolicy` field the value selects with its resource bound pre-folded, never a signature knob and never an unbounded `to_process.run_sync`.
- Auto: `_run` folds `self` through one total `match` whose two arms (read, write) lift the carrier's `CarrierPolicy` row once, so the per-carrier body is data, never a six-arm enumeration. The raster reader holds one `pyvips.Image.new_from_buffer(payload, "", access=Access.SEQUENTIAL)`, reads `Image.get_fields()` once for the present-field set, projects every EXIF logical through `_clean` (stripping the libvips `" (type, …)"` value suffix), parses the `xmp-data` blob through `_xmp` (one `lxml.etree.fromstring` over a `resolve_entities=False`/`no_network=True` parser, one namespaced `xpath` fold per `_FIELD_KEYS` xmp qname), evaluates EXIF `num/den` rationals (`28/10` → `2.8`) through `_num` for the `_RATIONAL` float logicals, folds the popped `gps_lat`/`gps_lon` degree/minute/second triples with their `gps_lat_ref`/`gps_lon_ref` hemisphere refs through `_dms` into the signed `Place.gps` decimal-degree pair, resolves the ICC name through `pillow` `ImageCms.getProfileDescription`, and folds all four standards into the one `MetaFacts.from_logical`. The raster writer copies the image, removes the metadata namespace on `REPLACE`/`STRIP`, sets each EXIF logical from `_flat(facts)` through `_FIELD_KEYS[logical].exif`, sets the rebuilt XMP packet (`_xmp_packet` authoring the `rdf:Description` tree through `lxml.etree.SubElement`) through `Image.set("xmp-data", …)`, then `write_to_buffer` re-encodes by the `_FORMAT` loader suffix. The PDF arms open one `PdfMetadata` context, the read folding every `_FIELD_KEYS` xmp qname present in the mapping plus `pdfa_status`/`pdfx_status`, the write deleting-then-assigning under the bind policy; the MEDIA arms open the `av` container, the read folding every `_FIELD_KEYS` media tag present in `Container.metadata`, the write a `demux` → `add_stream_from_template` → `mux` bitstream-copy remux that rebinds `OutputContainer.metadata` without re-encoding. `_flat` is the one facts→logical projection every writer derives its bindings from (`structs.asdict` over the facets, dropping empties and tuples), and `from_logical` is its inverse (one `msgspec.convert(strict=False)` per facet over the logical dict, coercing the standard string values to each facet field's type), so a field reaches every standard from one declared `_FIELD_KEYS` correspondence rather than a per-standard getter/setter pair.
- Receipt: each operation folds into `MetaFacts` and returns the `(ContentKey, MetaFacts, bytes)` triple, and the consumer projects the settled `core/receipt#RECEIPT` `ArtifactReceipt.Metadata(key, carrier, fields, byte_len)` case the ARCHITECTURE `[02]-[SEAMS]` `exchange/metadata → python:artifacts/core/receipt` edge names — the carrier the `MetaCarrier` value off the op the consumer admitted, the field count the `MetaFacts.populated` tally (the recovered/bound scalars plus the keyword set, GPS pair, and colour evidence), the byte length the returned payload — exactly as the coordinator mints `ArtifactReceipt.Credential` from the `exchange/credential#PROVENANCE` pair and `ArtifactReceipt.Verdict` from the `exchange/conformance#CONFORMANCE` pair. The write arms key the re-encoded bytes through `ContentIdentity.of` so a metadata-bound artifact carries a fresh content key the `csharp:Rasm.Persistence` store re-derives, and the read arms key the source bytes unchanged. The `core/receipt#RECEIPT` union carries the flat-scalar `Metadata` case (`metadata: tuple[ContentKey, str, int, int]`, its second slot the `carrier` discriminant) mirroring the flat-scalar `Credential`/`Media` cases, so the consumer spreads the carrier/field-count/byte-length slice onto it and the receipt owner imports no `MetaFacts` value object — the rich `MetaFacts` rides the returned triple to the reading caller, the flat case lives on the receipt page, neither minted here.
- Packages: `pyvips` (admitted, `3.1.1`, libvips Forge-provisioned: `Image.new_from_buffer(access=Access.SEQUENTIAL)`, `Image.get`/`get_fields`/`get_value`/`set`/`remove`/`copy`, `Image.write_to_buffer`, `Access`/`Error`) the streaming raster carrier reading EXIF/IPTC/XMP/ICC over the libvips field namespace without a full pixel decode; `pillow` (admitted, `12.2.0`, cp315-gated: `ImageCms.ImageCmsProfile`/`getProfileDescription` the ICC profile-name read over the embedded profile blob) the in-process ICC-name resolver the companion worker pairs with `pyvips`; `lxml` (admitted, `6.1.1`, cp315-gated companion: `etree.fromstring`/`XMLParser(resolve_entities=False, no_network=True, recover=True)`/`_Element.xpath`/`Element`/`SubElement`/`tostring`/`QName`) the XMP-RDF parse and packet-author owner, the admitted XML owner, never a hand-rolled RDF/XML codec; `pikepdf` (admitted, `10.9.1`, `cp314-abi3` ungated in-process: `Pdf.open`/`open_metadata(set_pikepdf_as_editor=, update_docinfo=) -> PdfMetadata`, the `PdfMetadata` mapping with `pdfa_status`/`pdfx_status`, `Pdf.save`) the PDF docinfo/XMP/conformance carrier; `av` (admitted, `17.1.0`, `cp311-abi3` cp315-clean in-process: `av.open`, `Container.metadata`, `InputContainer.demux`/`streams`/`format.name`, `OutputContainer.add_stream_from_template`/`mux`/`metadata`, `Packet.stream`/`dts`) the media container/stream metadata carrier; `expression` (`tagged_union`/`tag`/`case`); `msgspec` (`Struct(frozen=True)` for `FieldKeys`/`MetaFacts` and its five facet owners, `convert(strict=False)` the `from_logical` materializer, `structs.asdict` the `_flat` projection); `frozendict` (the `_FIELD_KEYS`/`_CARRIER`/`_XMP_NS`/`_FORMAT` tables); `anyio` (`to_thread.run_sync` the in-process PDF/MEDIA offload, `partial(to_process.run_sync, limiter=GATED_BAND)` the gated raster companion band bounded by the shared limiter); `functools` (`partial` pre-folding the limiter onto the RASTER lane value); runtime (`content_identity.ContentIdentity`/`ContentKey`, `faults.RuntimeRail`/`async_boundary`, `lanes.GATED_BAND` the shared `to_process` companion-band `CapacityLimiter` every gated raster crossing bounds against); `core/receipt#RECEIPT` (`ArtifactReceipt` the shared receipt family, the `Metadata` case the consumer projects the returned triple onto).
- Growth: a new metadata carrier is one `MetaCarrier` member plus one `_CARRIER` `CarrierPolicy(reader, writer, lane)` row plus its two carrier functions — zero new op cases, because the carrier is the payload field, not a tag; a new descriptive field is one field on the owning `MetaFacts` facet plus one `_FIELD_KEYS` row, the `_flat`/`from_logical` derivation reaching every standard with no per-standard table edit; a new standard a carrier supports is one more column on the `FieldKeys` row plus one fold inside that carrier's reader/writer; a new write disposition is one `MetaBind` member plus one arm in the writers; a new descriptive facet is one nested frozen `Struct` on `MetaFacts` plus one `from_logical` convert leg; zero new surface.
- Boundary: a per-format reader (`JpegExifReader`/`PngExifReader`), a per-tag getter/setter accessor family (`get_author`/`set_author`/`get_title`/`set_title`), a per-standard `ExifMeta`/`XmpMeta`/`IptcMeta` class family, a carrier×direction six-case op union where the carrier is the verb payload's field, a `_LANE` offload table beside a `_CARRIER_TABLE` acceptor map where one `CarrierPolicy` row carries `(reader, writer, lane)`, parallel `_XMP_KEYS`/`_EXIF_FIELDS`/`_MEDIA_TAGS` correspondences where one `_FIELD_KEYS` primary owns the logical→standard map, a write-only `@receipted`-harvested `ArtifactReceipt` return discarding the rich `MetaFacts` a READ recovers and the re-encoded bytes a WRITE produces where the `(ContentKey, MetaFacts, bytes)` triple carries both to the consumer (mirroring the `exchange/credential#PROVENANCE`/`exchange/conformance#CONFORMANCE` evidence pair), a phantom `Redaction.STRUCTURAL` the runtime `observability/receipts#RECEIPT` `Redaction` owner never defines, a degrees-only GPS read or a numerator-only rational where `_dms` folds the degree/minute/second triple with its hemisphere ref and `_num` divides `num/den`, a hand-rolled APP1/XMP-packet/IIM-marker codec where `pyvips`/`pikepdf`/`av` own the standard and `lxml` owns the XMP RDF, a flat erased `dict[str, Any]` metadata bag the consumer re-validates, a `MappingProxyType` table where `frozendict` is the py3.15 owner, a phantom `exif`/`iptcinfo3`/`python-xmp-toolkit` arm signature-locked behind an un-admitted package where the admitted `pyvips`/`pillow`/`lxml` cluster already owns the raster EXIF/IPTC/XMP concern, a `to_process` subprocess band for the in-process `pikepdf`/`av` carriers where `to_thread` offloads the GIL-releasing native call, an unbounded bare `to_process.run_sync` trusting the per-loop default process limiter (and the false `reliability/faults#FAULT`-owns-the-band attribution — the faults owner mints no `CapacityLimiter`) where the shared `execution/lanes#LANE` `GATED_BAND` bounds the RASTER companion band, a one-field `Metadata` wrapper over a separate `MetaOp` op union where the one `tagged_union` owns the read/write cases, the carrier-polymorphic `Read`/`Write` mints, the `carrier` property, and the async entry directly (the `exchange/credential#PROVENANCE` law), and an identity re-mint the runtime `content_identity` owns are the deleted forms; this owner reads and writes descriptive metadata on already-emitted bytes and produces no artifact. The untrusted field admission for the write arms folds an external field dict into `MetaFacts` through the `boundaries.md` `msgspec.convert`/ingress seam at the caller, composed there, never re-taught here — the interior receives the canonical `MetaFacts`. The descriptive-metadata facts are orthogonal to the `exchange/detect#DETECT` format-identification gate (detect sniffs the container type and routes the carrier, metadata reads the descriptive fields inside it), to the `exchange/credential#CREDENTIAL` C2PA provenance bind (credential is the signed tamper-evident manifest, metadata is the plain descriptive field set), and to the `media/video#MEDIA`/`media/audio#MEDIA` encode owners (those own the codec/transcode pipeline and the encode-evidence `Media` receipt, this owner the descriptive container tags through a no-re-encode remux), each contributing its own fact slice to the shared `ArtifactReceipt`. The ICC blob travels to `graphic/raster#RASTER` for a managed colour transform; this owner reads only its presence, profile name, and colour-space interpretation as descriptive evidence.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from collections.abc import Awaitable, Callable, Mapping
from dataclasses import dataclass
from enum import StrEnum
from functools import partial
from io import BytesIO
from typing import Final, Literal, Self, assert_never

from anyio import to_process, to_thread
from builtins import frozendict
from expression import case, tag, tagged_union
from msgspec import Struct, convert, structs

from rasm.runtime.content_identity import ContentIdentity, ContentKey
from rasm.runtime.faults import RuntimeRail, async_boundary
from rasm.runtime.lanes import GATED_BAND

# --- [TYPES] ----------------------------------------------------------------------------
type MetaReader = Callable[[bytes], "MetaFacts"]
type MetaWriter = Callable[[bytes, "MetaFacts", "MetaBind"], bytes]
type Offload = Callable[..., Awaitable[object]]
type MetaDir = Literal["read", "write"]


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
    "photoshop": "http://ns.adobe.com/photoshop/1.0/",
    "xmpRights": "http://ns.adobe.com/xap/1.0/rights/",
    "Iptc4xmpCore": "http://iptc.org/std/Iptc4xmpCore/1.0/xmlns/",
})

# float logicals whose EXIF value is a `num/den` rational `_num` normalizes before materialization;
# every other numeric field is a plain numeral `msgspec.convert(strict=False)` coerces directly.
_RATIONAL: Final[frozenset[str]] = frozenset({"f_number", "focal_length", "aperture", "exposure_bias", "altitude", "gps_direction"})

_FORMAT: Final[frozendict[str, str]] = frozendict({
    "jpegload": ".jpg", "pngload": ".png", "tiffload": ".tif", "webpload": ".webp", "heifload": ".heif",
    "gifload": ".gif", "jp2kload": ".jp2", "jxlload": ".jxl",
})


# --- [MODELS] ---------------------------------------------------------------------------
class FieldKeys(Struct, frozen=True, gc=False):  # the per-logical standard correspondence row
    xmp: str = ""
    exif: str = ""
    media: str = ""


class Descriptive(Struct, frozen=True):  # editorial: dc:* / xmp:* / photoshop:* / Iptc4xmpCore:* / EXIF ImageDescription
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
    software: str = ""
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
    gps: tuple[float, float] | None = None
    altitude: float | None = None
    gps_direction: float | None = None
    gps_timestamp: str = ""
    map_datum: str = ""
    city: str = ""
    state: str = ""
    country: str = ""
    country_code: str = ""
    sublocation: str = ""


class Color(Struct, frozen=True):  # colour evidence: libvips interpretation + ICC profile (read-only)
    space: str = ""
    icc_name: str = ""
    icc_present: bool = False


class MetaFacts(Struct, frozen=True):
    descriptive: Descriptive = Descriptive()
    rights: Rights = Rights()
    capture: Capture = Capture()
    place: Place = Place()
    color: Color = Color()
    conformance: str = ""  # pikepdf pdfa_status / pdfx_status

    @classmethod
    def from_logical(cls, flat: Mapping[str, object], /) -> Self:
        # one logical dict -> the five facets via per-facet `convert(strict=False)`; unknown keys fall
        # away per facet, the standard string values coerce to each field's type, `marked` lifts to bool.
        data = dict(flat)
        if isinstance(mark := data.get("marked"), str):
            data["marked"] = mark.strip().lower() in ("true", "1", "yes")
        return cls(
            descriptive=convert(data, Descriptive, strict=False),
            rights=convert(data, Rights, strict=False),
            capture=convert(data, Capture, strict=False),
            place=convert(data, Place, strict=False),
            color=convert(data, Color, strict=False),
            conformance=str(data.get("conformance", "")),
        )

    @property
    def populated(self) -> int:
        return len(_flat(self)) + bool(self.descriptive.keywords) + bool(self.place.gps) + bool(self.color.icc_present) + bool(self.color.space)


@dataclass(frozen=True, slots=True, kw_only=True)
class CarrierPolicy:  # the `(reader, writer, lane)` triple one `MetaCarrier` keys, the carrier collapse
    reader: MetaReader
    writer: MetaWriter
    lane: Offload


# `Metadata` IS the closed union: the two read/write cases, the carrier-polymorphic mints, the carrier
# property, the async entry, and the dispatch all fold onto one `tagged_union` (mirroring
# `exchange/credential#PROVENANCE`), never a one-field `Metadata` wrapper over a separate `MetaOp`.
@tagged_union(frozen=True)
class Metadata:
    tag: MetaDir = tag()
    read: tuple[MetaCarrier, bytes] = case()
    write: tuple[MetaCarrier, bytes, MetaFacts, MetaBind] = case()

    @classmethod
    def Read(cls, carrier: MetaCarrier, payload: bytes, /) -> Self:
        return cls(read=(carrier, payload))

    @classmethod
    def Write(cls, carrier: MetaCarrier, payload: bytes, facts: MetaFacts, bind: MetaBind = MetaBind.MERGE, /) -> Self:
        return cls(write=(carrier, payload, facts, bind))

    @property
    def carrier(self) -> MetaCarrier:
        match self:
            case Metadata(tag="read", read=(carrier, *_)) | Metadata(tag="write", write=(carrier, *_)):
                return carrier
            case _ as unreachable:
                assert_never(unreachable)

    async def of(self) -> RuntimeRail[tuple[ContentKey, MetaFacts, bytes]]:
        return await async_boundary(f"metadata.{self.carrier.value}.{self.tag}", self._emit)

    # no `@receipted`: a READ recovers the rich `MetaFacts` and a WRITE the re-encoded bytes, both
    # exceeding the four-scalar receipt, so the owner returns the evidence triple and the consumer
    # mints `ArtifactReceipt.Metadata` — exactly as `exchange/credential`/`conformance` return their
    # `(ContentKey, evidence)` pair rather than the write-only producer's `@receipted` weave.
    async def _emit(self) -> tuple[ContentKey, MetaFacts, bytes]:
        payload, facts = await self._run()
        return ContentIdentity.of(f"metadata.{self.carrier.value}.{self.tag}", payload), facts, payload

    async def _run(self) -> tuple[bytes, MetaFacts]:
        policy = _CARRIER[self.carrier]
        match self:
            case Metadata(tag="read", read=(_carrier, payload)):
                return payload, await policy.lane(policy.reader, payload)
            case Metadata(tag="write", write=(_carrier, payload, facts, bind)):
                return await policy.lane(policy.writer, payload, facts, bind), facts
            case _ as unreachable:
                assert_never(unreachable)


# --- [OPERATIONS] -----------------------------------------------------------------------
def _flat(facts: MetaFacts) -> dict[str, str]:
    # the one facts->logical projection; `structs.asdict` over the bindable facets, dropping empties
    # and tuples (keywords/gps ride their own writer arms) so each writer derives bindings from one map.
    fields: dict[str, object] = {"conformance": facts.conformance}
    for facet in (facts.descriptive, facts.rights, facts.capture, facts.place):
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
    if slash and top.strip(".-") and bottom.strip(".-"):
        return float(top) / float(bottom)
    return float(top) if top.strip(".-") else 0.0


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
    from lxml import etree

    root = etree.fromstring(blob, parser=etree.XMLParser(resolve_entities=False, no_network=True, recover=True))
    ns = dict(_XMP_NS)
    # XMP simple props ride as `rdf:Description` attributes; Alt/Bag values ride as nested `rdf:li` text.
    text = lambda qname: " ; ".join(t.strip() for t in root.xpath(f"//{qname}//text()[normalize-space()] | //@{qname}", namespaces=ns) if t.strip())
    return {logical: value for logical, keys in _FIELD_KEYS.items() if keys.xmp and (value := text(keys.xmp))}


def _xmp_packet(facts: MetaFacts) -> bytes:
    from lxml import etree

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


def _icc_name(blob: bytes) -> str:
    from PIL import ImageCms

    try:  # Exemption: ImageCms raises on a malformed ICC blob; the name is best-effort descriptive evidence.
        return ImageCms.getProfileDescription(ImageCms.ImageCmsProfile(BytesIO(blob))).strip()
    except (OSError, ValueError, ImageCms.PyCMSError):
        return ""


# --- raster carrier (pyvips + pillow + lxml, gated companion via to_process) -------------
def _read_raster(payload: bytes) -> MetaFacts:
    import pyvips

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
    flat["icc_name"] = _icc_name(bytes(image.get_value("icc-profile-data"))) if "icc-profile-data" in present else ""
    return MetaFacts.from_logical(flat)


def _write_raster(payload: bytes, facts: MetaFacts, bind: MetaBind) -> bytes:
    import pyvips

    image = pyvips.Image.new_from_buffer(payload, "", access=pyvips.Access.SEQUENTIAL).copy()
    if bind in (MetaBind.REPLACE, MetaBind.STRIP):
        for name in image.get_fields():
            if name.startswith(("exif-", "iptc-", "xmp-", "icc-")) or name == "orientation":
                image.remove(name)
    if bind is not MetaBind.STRIP:
        for logical, value in _flat(facts).items():
            if (keys := _FIELD_KEYS.get(logical)) and keys.exif:
                image.set(keys.exif, value)
        if packet := _xmp_packet(facts):
            image.set("xmp-data", packet)
    return image.write_to_buffer(_FORMAT.get(str(image.get("vips-loader")), ".tif"))


# --- pdf carrier (pikepdf, in-process via to_thread) ------------------------------------
def _read_pdf(payload: bytes) -> MetaFacts:
    import pikepdf

    with pikepdf.open(BytesIO(payload)) as pdf, pdf.open_metadata(set_pikepdf_as_editor=False, update_docinfo=True) as meta:
        join = lambda value: "; ".join(value) if isinstance(value, list) else str(value)
        flat: dict[str, object] = {logical: join(meta[keys.xmp]) for logical, keys in _FIELD_KEYS.items() if keys.xmp and keys.xmp in meta}
        flat["keywords"] = tuple(meta.get("dc:subject", ()))
        flat["conformance"] = meta.pdfa_status or meta.pdfx_status
        return MetaFacts.from_logical(flat)


def _write_pdf(payload: bytes, facts: MetaFacts, bind: MetaBind) -> bytes:
    import pikepdf

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


# --- media carrier (av, in-process via to_thread; write is a no-re-encode remux) ---------
def _read_media(payload: bytes) -> MetaFacts:
    import av

    with av.open(BytesIO(payload), mode="r") as container:
        tags = {key.lower(): value for key, value in container.metadata.items()}
    flat: dict[str, object] = {logical: tags[keys.media] for logical, keys in _FIELD_KEYS.items() if keys.media and keys.media in tags}
    flat["keywords"] = tuple(k.strip() for k in tags.get("keywords", "").split(",") if k.strip())
    return MetaFacts.from_logical(flat)


def _write_media(payload: bytes, facts: MetaFacts, bind: MetaBind) -> bytes:
    import av

    sink = BytesIO()
    with av.open(BytesIO(payload), mode="r") as source, av.open(sink, mode="w", format=source.format.name) as out:
        kept = {} if bind in (MetaBind.REPLACE, MetaBind.STRIP) else dict(source.metadata)
        if bind is not MetaBind.STRIP:
            kept.update({keys.media: value for logical, value in _flat(facts).items() if (keys := _FIELD_KEYS.get(logical)) and keys.media})
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
})
# RASTER crosses the gated companion band (pyvips/pillow/lxml off the cp315 core) bounded by the
# shared execution/lanes GATED_BAND CapacityLimiter the detect/raster/io siblings share — the lane
# pre-folds the limiter so the dispatch never re-pairs it; PDF/MEDIA run in-process (pikepdf
# cp314-abi3, av cp311-abi3) folded onto the structured-concurrency thread.
_CARRIER: Final[frozendict[MetaCarrier, CarrierPolicy]] = frozendict({
    MetaCarrier.RASTER: CarrierPolicy(reader=_read_raster, writer=_write_raster, lane=partial(to_process.run_sync, limiter=GATED_BAND)),
    MetaCarrier.PDF: CarrierPolicy(reader=_read_pdf, writer=_write_pdf, lane=to_thread.run_sync),
    MetaCarrier.MEDIA: CarrierPolicy(reader=_read_media, writer=_write_media, lane=to_thread.run_sync),
})
```

## [03]-[RESEARCH]

- [RASTER_ADMITTED] [RESOLVED]: the raster EXIF/IPTC/XMP/ICC concern is owned in-process-or-gated-companion by the admitted `pyvips`/`pillow`/`lxml` cluster, never the un-admitted `exif`/`iptcinfo3`/`python-xmp-toolkit` packages absent from both `.api` tiers. `pyvips` (`.api` `3.1.1`) `Image.new_from_buffer(data, options, *, access=Access.SEQUENTIAL)` (ENTRYPOINTS row `[02]`, `Access` enum `[02]` row `[01]`), `Image.get`/`set`/`get_fields`/`copy` and `Image.get_value("icc-profile-data")` (the `[03]` generated-ops metadata family "read/write image metadata (EXIF/ICC/orientation)" plus the explicit embedded-ICC recovery — `remove` rides the same get/set metadata family and is keyed off `get_fields()`), and `Image.write_to_buffer(format_string)` (`[03]` row `[02]`, the `strip`/`keep` egress options on the same call) verify against the folder `pyvips` `.api`; the libvips metadata namespace (`exif-ifd0-*`/`exif-ifd2-*`/`exif-ifd3-*`, `iptc-data`, `xmp-data`, `icc-profile-data`, `orientation`, `interpretation`, `vips-loader`) is read through the confirmed `get`, the EXIF string value carrying a `" (type, N bytes)"` libvips suffix `_clean` strips and the EXIF `num/den` rational `_num` evaluates (numerator over denominator, never the numerator alone) for the `_RATIONAL` float logicals, while the GPS `exif-ifd3-GPSLatitude`/`GPSLongitude` degree/minute/second triple plus its `GPSLatitudeRef`/`GPSLongitudeRef` hemisphere ref fold through `_dms` into the signed `Place.gps` decimal-degree pair. The extended capture/place set (`aperture`/`exposure_bias`/`exposure_program`/`metering_mode`/`flash`/`white_balance`/`serial_number`/`lens_make` over `exif-ifd2-*`, `gps_direction`/`gps_timestamp`/`map_datum`/`gps_lat_ref`/`gps_lon_ref` over `exif-ifd3-*`) is standard EXIF the confirmed `get` reads over more namespace strings, no new member. `pillow` (`.api` `12.2.0`, cp315-gated) `ImageCms.ImageCmsProfile` (`[02]` row `[09]`) is the admitted ICC owner; `getProfileDescription`/`PyCMSError` ride that `ImageCms` surface. `lxml` (`.api` `6.1.1`, cp315-gated companion) `etree.fromstring` (row `[02]`), `XMLParser(resolve_entities=False, no_network=True, recover=True)` (rows `[03]`/`[10]`), `_Element.xpath` (row `[02]`), `Element`/`SubElement` (row `[04]`), `tostring(xml_declaration=, encoding=)` (row `[05]`) verify against the folder `lxml` `.api` — `lxml` is the admitted XML owner the XMP-RDF parse and packet-author compose, never a hand-rolled RDF codec, and `lxml.html.clean` is NOT cited. All three are gated off the cp315 core (`pyvips` sdist-only over Forge-provisioned libvips; `pillow`/`lxml` manifest-gated `python_version<'3.15'`), so the raster arm crosses the `execution/lanes#LANE` `GATED_BAND` `CapacityLimiter`-bounded `anyio.to_process.run_sync(_read_raster, payload, limiter=GATED_BAND)` companion band importing the three at boundary scope — the same gated lane `exchange/detect#DETECT` and `graphic/raster/io#RASTER` cross under the one shared limiter — and folds the four standards in one process crossing returning the picklable `MetaFacts`; the `_read_raster`/`_write_raster` carriers are module-level so `to_process.run_sync` resolves them by qualified name across the seam (a bound method or closure is unresolvable there). Close-condition: an `assay api resolve` pass on the provisioned companion confirms the `pillow` `ImageCms.getProfileDescription` ICC-name accessor spelling and the libvips GPS `exif-ifd3-GPSLatitude`/`GPSLatitudeRef` component-separator and rational value spelling the `Place.gps` `_dms` degree/minute/second projection folds (the EXIF-spec DMS-triple-plus-ref structure is stable regardless of the exact libvips separator, which `_dms` tolerates by splitting on whitespace and commas alike).
- [GATED_BAND] [OPEN]: the RASTER `to_process` crossing bounds against the shared `execution/lanes#LANE` `GATED_BAND` `CapacityLimiter` per `concurrency.md` `SCOPE_CHOOSER` row `[11]` (GIL-hostile native call → `to_process.run_sync(limiter=)`), folded onto the lane value as `partial(to_process.run_sync, limiter=GATED_BAND)` so the dispatch never re-pairs a bound to a lane the value already carries — the doctrine-correct explicit-limiter form replacing the prior unbounded bare `to_process.run_sync` and the false `reliability/faults#FAULT`-owns-the-band attribution (`faults` mints `BoundaryFault`/`RuntimeRail`/`async_boundary` and owns no `CapacityLimiter`). The cross-file resolution is `execution/lanes#LANE` exporting the shared `GATED_BAND` (sized by `os.process_cpu_count()`) for the host-native companion band, the same import `exchange/detect#DETECT` and every gated raster sibling (`graphic/raster/io`/`measure`/`process`, `graphic/color/managed`) take; this page imports `from rasm.runtime.lanes import GATED_BAND` and passes `limiter=GATED_BAND` as the settled consumer edge.
- [PDF_ADMITTED] [RESOLVED]: the PDF docinfo/XMP/conformance carrier is the admitted in-process `pikepdf` (`.api` `10.9.1`, `cp314-abi3`, ungated). `Pdf.open`/`Pdf.save` (`[03]` rows `[01]`/`[03]`), `Pdf.open_metadata(set_pikepdf_as_editor=, update_docinfo=) -> PdfMetadata` (row `[08]`), and the `PdfMetadata` mapping with `pdfa_status`/`pdfx_status` (`[04]` metadata axis: "`Pdf.open_metadata` yields a `PdfMetadata` mapping … `pdfa_status`/`pdfx_status` conformance probes") verify against the folder `pikepdf` `.api`. `PdfMetadata` is a FULL mapping over the Dublin-Core/XMP-Basic/Photoshop/xmpRights namespaces, so the PDF reader folds every `_FIELD_KEYS` xmp qname present in the mapping (`dc:*`/`xmp:*`/`photoshop:*`/`xmpRights:*`) — not a hand-picked field slice — and a list-valued key (`dc:creator`/`dc:subject`) joins or lands as the keyword tuple. The scalar XMP read/write runs synchronously folded onto `anyio.to_thread.run_sync` over the cp314-abi3 wheel so the rail never blocks the loop; the PDF carrier needs no gated band and no `lxml` (qpdf parses the XMP into the `PdfMetadata` mapping directly).
- [MEDIA_ADMITTED] [RESOLVED]: the media container/stream descriptive metadata carrier is the admitted in-process `av` (`.api` `17.1.0`, `cp311-abi3`, cp315-clean). `av.open(file, mode)` (`[03]` rows `[01]`/`[02]`), `Container.metadata` (read on `InputContainer` `[02]` row `[02]`, set on `OutputContainer` — both inherit `Container.metadata`/`Container.format`, confirmed by reflection), `InputContainer.demux`/`streams` (rows `[01]`/`[04]`), `source.format.name` (`Container.format` + `ContainerFormat.name` row `[19]`, confirmed by reflection), `OutputContainer.add_stream_from_template` (row `[05]`), `OutputContainer.mux` (row `[09]`), and `Packet.stream`/`Packet.dts` (row `[12]`) verify against the folder `av` `.api`. The read folds the FFmpeg container tag set (`title`/`comment`/`artist`/`copyright`/`encoder`/`creation_time`/`genre`/`language`/`publisher` through the `_FIELD_KEYS` media column over the free-form `Container.metadata` dict, no new member); the write is a `demux` → `add_stream_from_template` → `mux` bitstream-copy remux rebinding `OutputContainer.metadata` with NO re-encode (distinct from the `media/video#MEDIA` encode/transcode owner), folded onto `anyio.to_thread.run_sync` over the wheel-bundled FFmpeg. The prior `OutputContainer.metadata` settable-dict and `InputContainer.format.name` close-conditions are resolved — both members are inherited from `Container` and confirmed present.
- [CARRIER_DISCRIMINANT] [RESOLVED]: the owner discriminates by the `read`/`write` VERB over the `(MetaCarrier, payload)` shape, not by standard and not by a carrier×direction case product — a single raster carries EXIF + IPTC + XMP + ICC simultaneously, so a per-standard `ReadExif`/`ReadIptc`/`ReadXmp` op family fragments one carrier into three reads where one pass over `pyvips.Image.get_fields()` + the `lxml` XMP parse recovers all four into the one `MetaFacts`, and a six-case `read_raster`/`write_raster`/`read_pdf`/… union is the carrier×direction explosion the two-case `Metadata` tagged_union collapses by riding the carrier as the verb payload's first field. The standards are field-namespace facets the carrier readers fold (`Capture`/`Place` from EXIF, `Descriptive`/`Rights` from XMP/IPTC, `Color` from ICC, `conformance` from PDF/A·PDF/X) through the one `_FIELD_KEYS` correspondence and the one `MetaFacts.from_logical` materialization, and the `MetaBind` (`MERGE`/`REPLACE`/`STRIP`) write policy carries the disposition as a behaviour-bearing value rather than a `clear: bool` knob, the `STRIP` arm the privacy-scrub (`pyvips.Image.remove` over the metadata namespace, `pikepdf` `del meta[key]`, `av` empty `OutputContainer.metadata`).
- [FIELD_CORRESPONDENCE] [RESOLVED]: the three former per-standard tables (`_XMP_KEYS`/`_EXIF_FIELDS`/`_MEDIA_TAGS`) collapse into one primary `_FIELD_KEYS: frozendict[str, FieldKeys]` keyed by logical field name, each `FieldKeys(xmp, exif, media)` row carrying the three standard spellings of one field, per the DERIVED_LOGIC one-primary-correspondence law — `_xmp`/`_read_pdf`/`_read_media` read their own column, `_flat`/`_xmp_packet`/the writers write their own column, and the `_CARRIER` row collapses the former `_CARRIER_TABLE` acceptor pair and `_LANE` offload map into one `CarrierPolicy(reader, writer, lane)` per the algorithms.md ROUTE_UNION "one policy row carries the whole axis bundle" law. `MetaFacts.from_logical` (one `msgspec.convert(strict=False)` per facet, verified to coerce the standard string values to each facet field's type and drop foreign keys) is the one materializer all three readers fold into, and `_flat` (`structs.asdict` over the facets) is its inverse the three writers derive from — the logical name is the facet field name, so a new field needs no per-standard table edit.
- [META_RECEIPT_CASE] [RESOLVED]: the descriptive-metadata operation returns the rich `(ContentKey, MetaFacts, bytes)` evidence triple, and the consumer projects the ARCHITECTURE `[02]-[SEAMS]` `exchange/metadata → python:artifacts/core/receipt` edge — the carrier, the populated descriptive-field count, and the payload byte length onto the shared `core/receipt#RECEIPT` `ArtifactReceipt.Metadata` case `metadata: tuple[ContentKey, str, int, int]` mirroring the flat-scalar `Credential`/`Media` cases, so the receipt owner imports no `MetaFacts` value object. `_emit` carries NO `@receipted` weave — exactly as `exchange/credential#PROVENANCE` `_emit` carries `@stamina.retry` alone and `exchange/conformance#CONFORMANCE` `close` carries none, both returning the rich `(ContentKey, evidence)` pair the consumer mints the receipt from — because a metadata READ recovers the full `MetaFacts` (the descriptive field set the reading caller needs) and a WRITE the re-encoded artifact bytes, both exceeding the four-scalar summary. The prior `@receipted(Redaction.STRUCTURAL)` weave was the deleted form on two counts: it discarded the read's recovered fields and the write's re-encoded bytes (a write-only producer's pattern wrongly applied to a read/write owner), and it cited the phantom `Redaction.STRUCTURAL` the runtime `observability/receipts#RECEIPT` `Redaction` owner — a `Struct(classified, salt)` with no named policy instances — does not define. `_emit` keys the (source or re-encoded) payload through `ContentIdentity.of` and returns `(key, facts, payload)`; the consumer spreads the carrier value (off the `Metadata` it admitted), `facts.populated`, and `len(payload)` onto `ArtifactReceipt.Metadata` exactly as it mints `ArtifactReceipt.Credential` from the credential pair and `ArtifactReceipt.Verdict` from the conformance pair; the write arms key the re-encoded bytes so a metadata-bound artifact carries a fresh content key the `csharp:Rasm.Persistence` store re-derives, the read arms key the source bytes, and the metadata owner re-mints no canonical content key (the runtime `content_identity` owns it).
