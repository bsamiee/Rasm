# [PY_ARTIFACTS_METADATA]

`Metadata` is the descriptive-metadata read/write owner at the exchange boundary — ONE `tagged_union` binding and recovering the EXIF/IPTC/XMP/ICC/KTX2 standards (title, creator, copyright, keyword, camera, exposure, GPS, rights, the full ICC profile header, the xmpMM asset identity/lineage, and the raster, media, and deep-texture container structures) on an already-emitted raster, PDF, media, or KTX2 artifact, discriminating the `read`/`write` verb over its `(MetaCarrier, payload)` shape — the carrier riding as the verb payload's leading field, not a tag multiplier, so the op is two cases over four carriers, not eight. Those standards are field-namespace facets every carrier read folds into ONE `MetaFacts` through one `MetaFacts.from_logical` materialization keyed by the one `_FIELD_KEYS` logical→standard correspondence, never five parallel per-standard key tables.

Each carrier rides its admitted owner through the caller-threaded `LanePolicy.offload` seam at the runtime placement its package dictates: `pikepdf` resolves as a `KernelTrait.RELEASING` kernel on the thread arm, while the GIL-holding `av` carrier — the same engine ruling every `media/` crossing carries — the raster cluster, and the `pyktx` deep-texture carrier the texture plane already crosses on that arm all cross as `KernelTrait.HOSTILE` onto the warm process pool under their trait-row worker-death retry — the `CarrierPolicy` row carries `(reader, writer, trait)` so dispatch is one lookup, and the `pyexiftool` `-stay_open` driver is a worker-process-static resource acquired once per worker under a `stamina` spawn-retry, never a per-artifact respawn. `close(lane)` is the rich public egress — `RuntimeRail[tuple[ContentKey, bytes, MetaFacts]]`, the produced payload keyed by `ContentIdentity.key` over its OWN bytes beside the recovered/bound `MetaFacts` — and `emit(lane)` is the pipeline node whose `_emit` projects that triple onto the flat `core/receipt#RECEIPT` `ArtifactReceipt.Metadata(key, carrier, fields, byte_len)` case under the node's PRE-RUN key (`receipt.slot == node.key`; the produced-bytes key rides the triple alone), exactly as `exchange/credential#CREDENTIAL` and `exchange/conformance#CONFORMANCE` project their `close` triples: a READ recovers the full field set over the source bytes, a WRITE returns the re-encoded artifact, and the receipt is a projection of the triple, never the sole egress.

## [01]-[INDEX]

- [02]-[METADATA]: the one descriptive-metadata owner — a two-case `read`/`write` `tagged_union` folding EXIF/IPTC/XMP/ICC plus raster/media container structure into one `MetaFacts` per the `_FIELD_KEYS` correspondence, egressing the `(ContentKey, bytes, MetaFacts)` triple through `close` and projecting `ArtifactReceipt.Metadata` through `emit`.

## [02]-[METADATA]

- Owner: `Metadata` IS the one owner — a `tagged_union` of exactly TWO cases discriminating the read/write verb directly (mirroring `exchange/credential#CREDENTIAL` `Provenance`): `read: tuple[MetaCarrier, bytes]` and `write: tuple[MetaCarrier, bytes, WriteSpec]` (the write instruction `facts`+`bind` bundled in one named `WriteSpec`, never a naked positional tuple decoded by index) — the carrier riding as each payload's leading field and the direction the tag, so a new carrier adds zero cases. `MetaCarrier` (`RASTER`/`PDF`/`MEDIA`/`TEXTURE`) keys the `_CARRIER` `CarrierPolicy(reader, writer, trait)` row, so the operation routes by one lookup over `self.carrier`, never a per-standard reader/writer class family nor a `gated: bool` knob the body re-pairs to a trait the value already selects. `MetaBind` (`MERGE`/`REPLACE`/`STRIP`) is the write disposition. `MetaFacts` is the ONE composite value every read materializes and every write consumes — nine nested frozen facets plus the `conformance` PDF/A·PDF/X status, recovering the full field set across every standard plus the three container-structure reads in one shape. Writable XMP/EXIF/IPTC/KTX facets fold through `_flat`; the read-only `Color` ICC header, `RasterInfo`, `MediaInfo`, and `TextureInfo` are evidence-only siblings of `conformance` the writers never re-emit. The three CONTAINER-structure facets ride `Option` because their scalars spell real container facts with zero — a KTX2 header carries `pixelHeight = 0` on a 1-D texture and `pixelDepth = 0` on a non-3-D one — so a defaulted facet on a carrier that read no container is indistinguishable from a header the container genuinely filled with zeros; presence is the discriminant, and every zero inside a `Some` facet is admitted as measured.
- Cases: `read(carrier, payload)` and `write(carrier, payload, WriteSpec(facts, bind))` — one total `match` over `self` in `close`, the read arm one reader offload and the write arm one writer offload (the `WriteSpec` unpacked once to `spec.facts`/`spec.bind`), never a parallel reader/writer per carrier or standard. Each carrier's reader folds its native namespace into the one `MetaFacts`: RASTER reads EXIF + IPTC + XMP + ICC + GPS + maker-notes in ONE `pyexiftool` cross-format pass, PDF the full Dublin-Core/XMP/xmpMM/xmpRights namespace (the `pdf:Producer` converting-application fact and the `xmpMM:DocumentID`/`InstanceID` identity included) plus `pdfa_status`/`pdfx_status` plus the `/OutputIntents` ICC profile header through `pikepdf` + `PIL.ImageCms`, MEDIA the FFmpeg container tag set plus the container structure (chapters/tracks/duration) through `av`, TEXTURE the KTX2 key-value block plus the container structure (extent, pyramid depth, face count, `VkFormat`, transfer ordinal, supercompression scheme, orientation, swizzle) through `pyktx`. Every reader emits one `dict[str, object]` keyed by logical field name — a provider list or LangAlt container reduced through `_scalar` before it reaches a scalar field — and folds it through the one `MetaFacts.from_logical`, so a carrier read is one logical projection plus one materialization.
- Auto: `close` lifts the carrier's `CarrierPolicy` row once, so the per-carrier body is data, never a six-arm enumeration. `_helper` is the worker-process-static `ExifToolHelper(common_args=["-G"])` — acquired once per PROCESS worker under `@stamina.retry(on=(RuntimeError, ExifToolVersionError))` (the spawn/version transient), `set_json_loads(msgjson.decode)` swapping the msgspec parser onto the `get_tags` decode path, `atexit`-terminated — so the Perl startup cost is paid once per worker, never per artifact. Reading a raster spills `payload` to a `NamedTemporaryFile`, reads the `_EXIFTOOL_TAGS` set once — each numeric-typed logical requested with the per-tag `#` ValueConv suffix (`_NUMERIC` derives the split from the facet field types, so string fields keep PrintConv words `convert(strict=False)` admits while numeric fields arrive machine-parsable; a global `-n` pushes integer codes into every string field) — projects every logical through `_FIELD_KEYS[logical].exiftool`, reads the signed `Composite:GPSLatitude#`/`Longitude#` decimals directly (no DMS fold), splits the keyword list through `_as_tuple`, and resolves the ICC header off the `ICC_Profile:` group tags directly. A raster write clears the DESCRIPTIVE namespace on `REPLACE`/`STRIP` (`-all=` group-wildcard scrub, REPLACE excluding `--icc_profile:all` so the profile survives the bind the enum promises, the keyword tuple bound as a `list` for the repeated `-IPTC:Keywords` directives) and returns the metadata-only re-encoded bytes — a container mutation WITHOUT a pixel re-encode, higher-fidelity than a raster re-encode, the provider owning the IFD-type round-trip. PDF arms open one `PdfMetadata` context, the read folding every present `_FIELD_KEYS` xmp qname plus `pdfa_status`/`pdfx_status` plus the `_icc` fold over the `/OutputIntents` `DestOutputProfile` bytes, the write deleting-then-assigning under the bind policy. MEDIA arms open the `av` container, the read folding the `_media_info` container-structure read (duration/chapters/tracks) while the container is still open, the write a `demux` → `add_stream_from_template` → `mux` bitstream-copy remux that rebinds `OutputContainer.metadata` without re-encoding while `set_chapters` and each templated stream's `metadata` carry the navigation and labelling structure across every bind. `_flat` is the one facts→logical projection every writer derives its bindings from (`structs.asdict` over the writable facets, dropping empties and tuples), and `from_logical` its inverse (one `msgspec.convert(strict=False)` per facet), so a field reaches every provider from one declared `_FIELD_KEYS` correspondence.
- Receipt: `close(lane)` returns `RuntimeRail[tuple[ContentKey, bytes, MetaFacts]]` — the key minted by `ContentIdentity.key` over the PRODUCED bytes, so a metadata-bound artifact carries a fresh key the `dotnet:Rasm.Persistence` store re-derives while a read keys the source bytes unchanged. `emit(lane, parents=...)` returns the `ArtifactWork` node under the PRE-RUN `_key` (the msgpack input canon `keyed` admission elides on) and `_emit` projects the triple onto `ArtifactReceipt.Metadata(key, carrier, fields, byte_len)` under that same PRE-RUN key — `receipt.slot == node.key`, so the plan seam reads one identity while the fresh produced-bytes key stays the `close` triple's — per the ARCHITECTURE `exchange/metadata → core/receipt` edge — the carrier the `MetaCarrier` value, the field count the `MetaFacts.populated` tally, the byte length the produced payload. `core/receipt#RECEIPT` carries the flat-scalar `Metadata` case (`tuple[ContentKey, str, int, int]`) mirroring `Credential`/`Media`, so the receipt owner imports no `MetaFacts` value object — the rich `MetaFacts` rides the `close` triple to the reading caller, the flat case lives on the receipt page. `_emit` then awaits `Journal.record` over `receipt.evidence()` — descriptive metadata is a rights and provenance ASSERTION, so the fact lands `REGULATORY` under the case's own retention row and its diff names the carrier, the populated field count, and the byte volume a later claim reads back; a STRIP write records that same fact against its emptied ledger, which is the removal evidence itself. The seat is that awaitable fold, because recording suspends where `contribute` cannot.
- Packages: `pyexiftool` (`ExifToolHelper.get_tags`/`set_tags`/`execute`/`run`/`running`/`terminate` the `-stay_open` cross-format driver over EXIF+IPTC+XMP+ICC+GPS+maker-notes, the per-tag `#` ValueConv request keyed off `_NUMERIC` with the response key staying bare, `set_json_loads` swapping the msgspec parser onto the `get_tags` decode path, `ExifToolVersionError` the spawn-retry target); `pikepdf` (`open`/`open_metadata`/`PdfMetadata` the XMP/docinfo namespace + `pdfa_status`/`pdfx_status`, `Pdf.Root` `/OutputIntents` the ICC profile bytes via `Object.read_bytes`, `save(deterministic_id=True)` the stable /ID); `av` (`open`/`Container.metadata`/`chapters()`/`streams`/`duration`/`bit_rate` the read, `add_stream_from_template`/`demux`/`mux` the no-re-encode remux, `set_chapters`/`Stream.metadata` the structure carried across it); `PIL.ImageCms` (`getOpenProfile` + `getProfile*`/`getDefaultIntent` the ICC header, `PyCMSError` the exact profile boundary); `msgspec` (`Struct(frozen=True[, gc=False])`, `convert(strict=False)` the per-facet materialization, `structs.asdict` the `_flat`/`populated` derivation, `json` the exiftool decode, `msgpack.encode` the `_key` canon); `expression` (`tagged_union`, `Map`, `Option`/`Some`/`Nothing` the container-structure presence discriminant, `extra.result.catch`); `beartype`; `stamina` (the helper spawn-retry weave); `pydantic-settings` (the `MetaSettings` `RASM_META_` exiftool-path owner); runtime (`identity.ContentIdentity.key`/`ContentKey`, `faults.FAULT_CONF`/`RuntimeRail`, `journal.Journal` the durable seat's one writer, `lanes.LanePolicy`, `workers.Kernel`/`KernelTrait`); core (`plan.ArtifactWork`/`Admission`, `receipt.ArtifactReceipt`).
- Growth: a new metadata carrier is one `MetaCarrier` member plus one `_CARRIER` `CarrierPolicy` row plus its two carrier functions — zero new op cases; a new descriptive field one field on the owning `MetaFacts` facet plus one `_FIELD_KEYS` row, the `_flat`/`from_logical` derivation reaching every provider and `_NUMERIC` deriving its ValueConv-vs-PrintConv request from the declared type; a new provider spelling for a carrier is one more column on the `FieldKeys` row plus one fold in that carrier's reader/writer; a new write disposition one `MetaBind` member plus one writer arm; a new writable facet (the `History` xmpMM row) one nested `Struct` plus one `from_logical` convert leg plus its `_flat` rows; a new read-only structured facet (the `MediaInfo`/`RasterInfo` reads) one nested `Struct` plus its reader population plus one `Option`-typed `from_logical` keyword and one `map`/`default_value(0)` `populated` term, never a `_flat` write leg and never a defaulted facet whose zeros a reader cannot tell from the container's own; a new ICC-header fact one `Color` field plus one `ICC_Profile:` read; a new binary-path or worker knob one `MetaSettings` field; zero new surface.
- Boundary: `Metadata` binds and recovers descriptive metadata over already-emitted bytes and owns no artifact production — it re-encodes no pixels (the raster writer mutates container metadata in place, higher-fidelity than a raster re-encode) and mints no content key beyond the produced-bytes `ContentIdentity.key` at `close`. A PDF signature/conformance close routes to `exchange/conformance#CONFORMANCE`, a signed C2PA content credential to `exchange/credential#CREDENTIAL`: the `Iptc4xmpExt:DigitalSourceType` this owner reads/writes is the UNSIGNED descriptive AI-provenance label, distinct from the SIGNED C2PA `DigitalSource` assertion. Rich `MetaFacts` rides the returned triple to the reading caller; the flat `ArtifactReceipt.Metadata` case lives on the receipt page, neither minted here. Signer key material never enters this plane, and the `_key` canon covers verb, carrier, bind, flattened facts, keywords, and payload — the provider binary path is deployment environment, declared non-identity.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
import atexit
import re
import shutil
from collections.abc import Callable, Mapping
from dataclasses import dataclass
from enum import StrEnum
from functools import partial
from io import BytesIO
from pathlib import Path
from tempfile import NamedTemporaryFile
from typing import Final, Literal, Self, assert_never, get_args

import stamina
from beartype import beartype
from expression import Error, Nothing, Option, Result, Some, case, tag, tagged_union
from expression.collections import Map
from expression.extra.result import catch
from msgspec import Struct, convert, msgpack, structs
from msgspec import json as msgjson
from pydantic_settings import BaseSettings, SettingsConfigDict

from rasm.artifacts.core.plan import Admission, ArtifactWork
from rasm.artifacts.core.receipt import ArtifactReceipt
from rasm.runtime.faults import FAULT_CONF, RuntimeRail
from rasm.runtime.identity import ContentIdentity, ContentKey
from rasm.runtime.journal import Journal
from rasm.runtime.lanes import LanePolicy
from rasm.runtime.workers import Kernel, KernelTrait

lazy from exiftool import ExifToolHelper
lazy from exiftool.exceptions import ExifToolVersionError
lazy from PIL import ImageCms
lazy import pikepdf
lazy import av
lazy from pyktx import KtxSupercmpScheme, KtxTexture2, KtxTextureCreateFlagBits

# --- [TYPES] ----------------------------------------------------------------------------
type MetaReader = Callable[[bytes], "MetaFacts"]
type MetaWriter = Callable[[bytes, "MetaFacts", "MetaBind"], bytes]


class MetaCarrier(StrEnum):
    RASTER = "raster"
    PDF = "pdf"
    MEDIA = "media"
    TEXTURE = "texture"


class MetaBind(StrEnum):
    MERGE = "merge"
    REPLACE = "replace"
    STRIP = "strip"


# --- [CONSTANTS] ------------------------------------------------------------------------
_INTENT_NAME: Final[Map[int, str]] = Map.of_seq([(0, "perceptual"), (1, "relative"), (2, "saturation"), (3, "absolute")])
_AV_TIME_BASE: Final[int] = 1_000_000
_SC_PARAMS: Final[re.Pattern[str]] = re.compile(r"--zstd\s+(\d+)")


# --- [MODELS] ---------------------------------------------------------------------------
class FieldKeys(Struct, frozen=True, gc=False):
    xmp: str = ""
    exiftool: str = ""
    media: str = ""
    ktx: str = ""


class Descriptive(Struct, frozen=True):
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
    comment: str = ""
    album: str = ""
    album_artist: str = ""
    composer: str = ""


class Rights(Struct, frozen=True):
    creator: str = ""
    credit: str = ""
    source: str = ""
    copyright: str = ""
    usage_terms: str = ""
    web_statement: str = ""
    publisher: str = ""
    marked: bool | None = None


class Capture(Struct, frozen=True):
    make: str = ""
    model: str = ""
    lens: str = ""
    lens_make: str = ""
    software: str = ""
    producer: str = ""
    digital_source_type: str = ""
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


class Place(Struct, frozen=True):
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


class History(Struct, frozen=True):
    document_id: str = ""
    instance_id: str = ""
    original_id: str = ""


class Color(Struct, frozen=True):
    space: str = ""
    icc_name: str = ""
    icc_present: bool = False
    render_intent: str = ""
    icc_maker: str = ""
    icc_model: str = ""
    icc_copyright: str = ""


class RasterInfo(Struct, frozen=True):
    width: int = 0
    height: int = 0
    mime: str = ""
    thumbnail: bytes = b""


class Chapter(Struct, frozen=True, gc=False):
    start: float = 0.0
    end: float = 0.0
    title: str = ""


class Track(Struct, frozen=True, gc=False):
    kind: str = ""
    language: str = ""
    title: str = ""


class TextureInfo(Struct, frozen=True):
    width: int = 0
    height: int = 0
    depth: int = 0
    levels: int = 0
    faces: int = 0
    layered: bool = False
    block_compressed: bool = False
    vk_format: str = ""
    transfer: int = 0
    supercompression: str = ""
    orientation: str = ""
    swizzle: str = ""
    sc_params: str = ""


class MediaInfo(Struct, frozen=True):
    duration: float = 0.0
    bit_rate: int = 0
    chapters: tuple[Chapter, ...] = ()
    tracks: tuple[Track, ...] = ()


class MetaFacts(Struct, frozen=True):
    descriptive: Descriptive = Descriptive()
    rights: Rights = Rights()
    capture: Capture = Capture()
    place: Place = Place()
    history: History = History()
    color: Color = Color()
    raster: Option[RasterInfo] = Nothing
    media: Option[MediaInfo] = Nothing
    texture: Option[TextureInfo] = Nothing
    conformance: str = ""

    @classmethod
    def from_logical(
        cls,
        flat: Mapping[str, object],
        /,
        *,
        media: Option[MediaInfo] = Nothing,
        raster: Option[RasterInfo] = Nothing,
        texture: Option[TextureInfo] = Nothing,
    ) -> Self:
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
            texture=texture,
            conformance=str(data.get("conformance", "")),
        )

    @property
    def populated(self) -> int:
        color = sum(1 for value in structs.asdict(self.color).values() if value)
        return (
            len(_flat(self))
            + bool(self.conformance)
            + bool(self.descriptive.keywords)
            + bool(self.place.gps)
            + color
            + self.raster.map(lambda info: bool(info.width) + bool(info.mime) + bool(info.thumbnail)).default_value(0)
            + self.texture.map(
                lambda info: bool(info.width) + bool(info.vk_format) + bool(info.orientation) + bool(info.swizzle) + bool(info.sc_params)
            ).default_value(0)
            + self.media.map(
                lambda info: len(info.chapters) + len(info.tracks) + bool(info.duration) + bool(info.bit_rate)
            ).default_value(0)
        )


@dataclass(frozen=True, slots=True, kw_only=True)
class CarrierPolicy:
    reader: MetaReader
    writer: MetaWriter
    trait: KernelTrait


class WriteSpec(Struct, frozen=True):
    facts: MetaFacts
    bind: MetaBind = MetaBind.MERGE


# --- [SERVICES] -------------------------------------------------------------------------
class MetaSettings(BaseSettings):
    model_config = SettingsConfigDict(env_prefix="RASM_META_", frozen=True, extra="forbid")
    exiftool: str | None = None




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

    @property
    def payload(self) -> bytes:
        match self:
            case Metadata(tag="read", read=(_, payload)) | Metadata(tag="write", write=(_, payload, _)):
                return payload
            case _ as unreachable:
                assert_never(unreachable)

    def emit(self, lane: LanePolicy, /, *, parents: tuple[ContentKey, ...] = ()) -> ArtifactWork:
        return ArtifactWork(key=self._key, work=partial(self._emit, lane), parents=parents, admission=Admission(keyed=None), cost=1.0)

    @property
    def _key(self) -> ContentKey:
        spec = self.write[2] if self.tag == "write" else None
        canon = (
            self.tag,
            self.carrier.value,
            spec.bind.value if spec is not None else "",
            tuple(sorted(_flat(spec.facts).items())) if spec is not None else (),
            spec.facts.descriptive.keywords if spec is not None else (),
            self.payload,
        )
        return ContentIdentity.key(f"metadata.{self.carrier.value}.{self.tag}", msgpack.encode(canon))

    async def close(self, lane: LanePolicy, /) -> RuntimeRail[tuple[ContentKey, bytes, MetaFacts]]:
        policy = _CARRIER[self.carrier]
        match self:
            case Metadata(tag="read", read=(_, payload)):
                railed = (await lane.offload(Kernel.of(policy.reader, policy.trait), payload)).map(
                    lambda facts: (payload, facts)
                )
            case Metadata(tag="write", write=(_, payload, spec)):
                railed = (await lane.offload(Kernel.of(policy.writer, policy.trait), payload, spec.facts, spec.bind)).map(
                    lambda blob: (blob, MetaFacts() if spec.bind is MetaBind.STRIP else spec.facts)
                )
            case _ as unreachable:
                assert_never(unreachable)
        return railed.map(lambda pair: (ContentIdentity.key(f"metadata.{self.carrier.value}", pair[0]), pair[0], pair[1]))

    async def _emit(self, lane: LanePolicy, /) -> RuntimeRail[ArtifactReceipt]:
        match (await self.close(lane)).map(lambda kbe: ArtifactReceipt.Metadata(self._key, self.carrier.value, kbe[2].populated, len(kbe[1]))):
            case Result(tag="ok", ok=receipt):
                return (await Journal.record(receipt.evidence())).map(lambda _landed: receipt)
            case refused:
                return Error(refused.error)


# --- [OPERATIONS] -----------------------------------------------------------------------
def _flat(facts: MetaFacts) -> dict[str, str]:
    fields: dict[str, object] = {}
    for facet in (facts.descriptive, facts.rights, facts.capture, facts.place, facts.history):
        fields.update(structs.asdict(facet))
    out: dict[str, str] = {}
    for logical, value in fields.items():
        match value:
            case str() as text if text:
                out[logical] = text
            case bool() as flag:
                out[logical] = str(flag)
            case int() | float() as num if num:
                out[logical] = str(num)
    return out


def _scalar(value: object, /) -> object:
    match value:
        case list() as items:
            return "; ".join(str(item).strip() for item in items if str(item).strip())
        case dict() as langalt:
            return str(langalt["x-default"]) if "x-default" in langalt else str(langalt[min(langalt, key=str)]) if langalt else ""
        case _:
            return value


def _as_tuple(value: object, /) -> tuple[str, ...]:
    match value:
        case list() | tuple() | set() | frozenset() as items:
            return tuple(str(item).strip() for item in items if str(item).strip())
        case str() as text:
            return tuple(part.strip() for part in text.replace(",", ";").split(";") if part.strip())
        case None:
            return ()
        case _:
            return (str(value),)


def _icc(blob: bytes) -> dict[str, object]:
    if not blob:
        return {}
    return catch(exception=ImageCms.PyCMSError)(ImageCms.getOpenProfile)(BytesIO(blob)).map(_icc_header).default_value({})


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
_ET: "ExifToolHelper | None" = None


@stamina.retry(on=(RuntimeError, ExifToolVersionError), attempts=3)
def _helper() -> "ExifToolHelper":
    global _ET
    if _ET is None or not _ET.running:
        _ET = ExifToolHelper(executable=_EXIFTOOL, common_args=["-G"])
        _ET.set_json_loads(msgjson.decode)
        _ET.run()
        atexit.register(_ET.terminate)
    return _ET


def _exiftool_read(payload: bytes) -> MetaFacts:
    et = _helper()
    with NamedTemporaryFile(suffix=".img", delete_on_close=False) as tmp:
        tmp.write(payload)
        tmp.close()
        rows = et.get_tags(tmp.name, list(_EXIFTOOL_TAGS))
        thumb = et.execute("-b", "-ThumbnailImage", tmp.name, raw_bytes=True)
    grouped = rows[0] if rows else {}
    flat: dict[str, object] = {
        logical: _scalar(grouped[keys.exiftool]) for logical, keys in _FIELD_KEYS.items() if keys.exiftool and keys.exiftool in grouped
    }
    flat["keywords"] = _as_tuple(grouped.get("IPTC:Keywords") or grouped.get("XMP-dc:Subject"))
    lat, lon = grouped.get("Composite:GPSLatitude"), grouped.get("Composite:GPSLongitude")
    flat["gps"] = (float(lat), float(lon)) if lat is not None and lon is not None else None
    flat["icc_present"] = any(key.startswith("ICC_Profile:") for key in grouped)
    raster = RasterInfo(
        width=int(grouped.get("File:ImageWidth") or 0),
        height=int(grouped.get("File:ImageHeight") or 0),
        mime=str(grouped.get("File:MIMEType", "")),
        thumbnail=thumb if isinstance(thumb, bytes) else b"",
    )
    return MetaFacts.from_logical(flat, raster=Some(raster))


def _exiftool_write(payload: bytes, facts: MetaFacts, bind: MetaBind) -> bytes:
    et = _helper()
    with NamedTemporaryFile(suffix=".img", delete_on_close=False) as tmp:
        tmp.write(payload)
        tmp.close()
        if bind in (MetaBind.REPLACE, MetaBind.STRIP):
            keep = () if bind is MetaBind.STRIP else ("--icc_profile:all",)
            et.execute("-all=", *keep, "-overwrite_original", tmp.name)
        if bind is not MetaBind.STRIP:
            tags: dict[str, str | list[str]] = {
                keys.exiftool: value
                for logical, value in _flat(facts).items()
                if (keys := _FIELD_KEYS.try_find(logical).default_value(None)) and keys.exiftool
            }
            if facts.descriptive.keywords:
                tags["IPTC:Keywords"] = tags["XMP-dc:Subject"] = list(facts.descriptive.keywords)
            if tags:
                et.set_tags(tmp.name, tags, params=["-overwrite_original"])
        return Path(tmp.name).read_bytes()


# --- [PDF_CARRIER] ----------------------------------------------------------------------
def _read_pdf(payload: bytes) -> MetaFacts:
    with pikepdf.open(BytesIO(payload)) as pdf:
        with pdf.open_metadata(set_pikepdf_as_editor=False, update_docinfo=True) as meta:
            flat: dict[str, object] = {
                logical: _scalar(meta[keys.xmp]) for logical, keys in _FIELD_KEYS.items() if keys.xmp and keys.xmp in meta
            }
            flat["keywords"] = _as_tuple(meta.get("dc:subject", ()))
            flat["conformance"] = meta.pdfa_status or meta.pdfx_status or ""
        if "/OutputIntents" in pdf.Root:
            intent = pdf.Root.OutputIntents[0]
            if "/DestOutputProfile" in intent:
                flat.update(_icc(bytes(intent.DestOutputProfile.read_bytes())))
                flat["icc_present"] = True
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
        if bind is MetaBind.STRIP and "/OutputIntents" in pdf.Root:
            del pdf.Root.OutputIntents
        pdf.save(sink, deterministic_id=True)
    return sink.getvalue()


# --- [MEDIA_CARRIER] --------------------------------------------------------------------
def _media_info(container: "av.container.InputContainer") -> MediaInfo:
    duration = container.duration / _AV_TIME_BASE if container.duration else 0.0
    spanned = lambda ch, edge: float(ch[edge] * ch["time_base"]) if ch["time_base"] else float(ch[edge])
    chapters = tuple(
        Chapter(start=spanned(ch, "start"), end=spanned(ch, "end"), title=ch["metadata"].get("title", "")) for ch in container.chapters()
    )
    tracks = tuple(Track(kind=stream.type, language=stream.language or "", title=stream.metadata.get("title", "")) for stream in container.streams)
    return MediaInfo(duration=duration, bit_rate=container.bit_rate or 0, chapters=chapters, tracks=tracks)


def _read_media(payload: bytes) -> MetaFacts:
    with av.open(BytesIO(payload), mode="r") as container:
        tags = {key.lower(): value for key, value in container.metadata.items()}
        media = _media_info(container)
    flat: dict[str, object] = {logical: tags[keys.media] for logical, keys in _FIELD_KEYS.items() if keys.media and keys.media in tags}
    flat["keywords"] = tuple(k.strip() for k in tags.get("keywords", "").split(",") if k.strip())
    return MetaFacts.from_logical(flat, media=Some(media))


def _write_media(payload: bytes, facts: MetaFacts, bind: MetaBind) -> bytes:
    sink = BytesIO()
    with av.open(BytesIO(payload), mode="r") as source, av.open(sink, mode="w", format=source.format.name) as out:
        kept = {} if bind in (MetaBind.REPLACE, MetaBind.STRIP) else dict(source.metadata)
        if bind is not MetaBind.STRIP:
            kept.update({
                keys.media: value
                for logical, value in _flat(facts).items()
                if (keys := _FIELD_KEYS.try_find(logical).default_value(None)) and keys.media
            })
            if facts.descriptive.keywords:
                kept["keywords"] = ",".join(facts.descriptive.keywords)
        out.metadata.update(kept)
        out.set_chapters(source.chapters())
        streams: dict[int, "av.stream.Stream"] = {}
        for stream in source.streams:
            streams[stream.index] = templated = out.add_stream_from_template(stream)
            templated.metadata.update(stream.metadata)
        for packet in source.demux():
            if packet.dts is None:
                continue
            packet.stream = streams[packet.stream.index]
            out.mux(packet)
    return sink.getvalue()


# --- [TEXTURE_CARRIER] ------------------------------------------------------------------
def _ktx_text(raw: bytes, /) -> str:
    return raw.decode("utf-8", "replace").rstrip("\x00")


def _ktx_read(payload: bytes) -> MetaFacts:
    with NamedTemporaryFile(suffix=".ktx2", delete_on_close=False) as tmp:
        tmp.write(payload)
        tmp.close()
        texture = KtxTexture2.create_from_named_file(tmp.name, KtxTextureCreateFlagBits.NO_FLAGS)
        held = texture.kv_data.copy()
        info = TextureInfo(
            width=texture.base_width,
            height=texture.base_height,
            depth=texture.base_depth,
            levels=texture.num_levels,
            faces=texture.num_faces,
            layered=texture.is_array,
            block_compressed=texture.is_compressed,
            vk_format=texture.vk_format.name,
            transfer=texture.oetf,
            supercompression=texture.supercompression_scheme.name,
            orientation=_ktx_text(held.get("KTXorientation", b"")),
            swizzle=_ktx_text(held.get("KTXswizzle", b"")),
            sc_params=_ktx_text(held.get("KTXwriterScParams", b"")),
        )
    flat: dict[str, object] = {logical: _ktx_text(held[keys.ktx]) for logical, keys in _FIELD_KEYS.items() if keys.ktx and keys.ktx in held}
    return MetaFacts.from_logical(flat, texture=Some(info))


def _ktx_bound(held: Mapping[str, bytes], facts: MetaFacts, bind: MetaBind, /) -> dict[str, bytes]:
    structural = {key: value for key, value in held.items() if key not in _KTX_DESCRIPTIVE}
    if bind is MetaBind.STRIP:
        return structural
    written = {
        keys.ktx: f"{value}\x00".encode()
        for logical, value in _flat(facts).items()
        if (keys := _FIELD_KEYS.try_find(logical).default_value(None)) and keys.ktx
    }
    return {**(structural if bind is MetaBind.REPLACE else dict(held)), **written}


def _redeflate(scheme: "KtxSupercmpScheme", held: Mapping[str, bytes], /) -> int:
    match scheme:
        case KtxSupercmpScheme.NONE:
            return 0
        case KtxSupercmpScheme.ZSTD if (recorded := _SC_PARAMS.search(_ktx_text(held.get("KTXwriterScParams", b"")))) is not None:
            return int(recorded[1])
        case _:
            raise ValueError(f"ktx metadata bind cannot re-apply supercompression {scheme.name}")


def _ktx_write(payload: bytes, facts: MetaFacts, bind: MetaBind) -> bytes:
    with NamedTemporaryFile(suffix=".ktx2", delete_on_close=False) as tmp:
        tmp.write(payload)
        tmp.close()
        probe = KtxTexture2.create_from_named_file(tmp.name, KtxTextureCreateFlagBits.NO_FLAGS)
        held = probe.kv_data.copy()
        level = _redeflate(probe.supercompression_scheme, held)
        bound = _ktx_bound(held, facts, bind)
        flags = KtxTextureCreateFlagBits.LOAD_IMAGE_DATA_BIT | KtxTextureCreateFlagBits.SKIP_KVDATA_BIT
        texture = KtxTexture2.create_from_named_file(tmp.name, flags)
        for key, value in sorted(bound.items()):
            texture.kv_data.add_kv_pair(key, value)
        if level:
            texture.deflate_zstd(level)
        return texture.write_to_memory()


# --- [TABLES] ---------------------------------------------------------------------------
_FIELD_KEYS: Final[Map[str, FieldKeys]] = Map.of_seq([
    ("title", FieldKeys(xmp="dc:title", exiftool="XMP-dc:Title", media="title")),
    ("headline", FieldKeys(xmp="photoshop:Headline", exiftool="XMP-photoshop:Headline")),
    ("caption", FieldKeys(xmp="dc:description", exiftool="XMP-dc:Description", media="comment")),
    ("keywords", FieldKeys(xmp="dc:subject", exiftool="XMP-dc:Subject", media="keywords")),
    ("rating", FieldKeys(xmp="xmp:Rating", exiftool="XMP-xmp:Rating")),
    ("label", FieldKeys(xmp="xmp:Label", exiftool="XMP-xmp:Label")),
    ("category", FieldKeys(xmp="photoshop:Category", exiftool="XMP-photoshop:Category")),
    ("genre", FieldKeys(xmp="Iptc4xmpCore:IntellectualGenre", exiftool="XMP-iptcCore:IntellectualGenre", media="genre")),
    ("language", FieldKeys(xmp="dc:language", exiftool="XMP-dc:Language", media="language")),
    ("instructions", FieldKeys(xmp="photoshop:Instructions", exiftool="XMP-photoshop:Instructions")),
    ("description_writer", FieldKeys(xmp="photoshop:CaptionWriter", exiftool="XMP-photoshop:CaptionWriter")),
    ("comment", FieldKeys(exiftool="File:Comment")),
    ("album", FieldKeys(media="album")),
    ("album_artist", FieldKeys(media="album_artist")),
    ("composer", FieldKeys(media="composer")),
    ("creator", FieldKeys(xmp="dc:creator", exiftool="XMP-dc:Creator", media="artist")),
    ("credit", FieldKeys(xmp="photoshop:Credit", exiftool="IPTC:Credit")),
    ("source", FieldKeys(xmp="photoshop:Source", exiftool="IPTC:Source")),
    ("copyright", FieldKeys(xmp="dc:rights", exiftool="XMP-dc:Rights", media="copyright")),
    ("usage_terms", FieldKeys(xmp="xmpRights:UsageTerms", exiftool="XMP-xmpRights:UsageTerms")),
    ("web_statement", FieldKeys(xmp="xmpRights:WebStatement", exiftool="XMP-xmpRights:WebStatement")),
    ("publisher", FieldKeys(xmp="dc:publisher", exiftool="XMP-dc:Publisher", media="publisher")),
    ("marked", FieldKeys(xmp="xmpRights:Marked", exiftool="XMP-xmpRights:Marked")),
    ("make", FieldKeys(exiftool="EXIF:Make")),
    ("model", FieldKeys(exiftool="EXIF:Model")),
    ("lens", FieldKeys(exiftool="EXIF:LensModel")),
    ("lens_make", FieldKeys(exiftool="EXIF:LensMake")),
    ("software", FieldKeys(xmp="xmp:CreatorTool", exiftool="EXIF:Software", media="encoder")),
    ("producer", FieldKeys(xmp="pdf:Producer", exiftool="XMP-pdf:Producer", ktx="KTXwriter")),
    ("digital_source_type", FieldKeys(xmp="Iptc4xmpExt:DigitalSourceType", exiftool="XMP-iptcExt:DigitalSourceType")),
    ("orientation", FieldKeys(exiftool="EXIF:Orientation")),
    ("exposure_time", FieldKeys(exiftool="EXIF:ExposureTime")),
    ("f_number", FieldKeys(exiftool="EXIF:FNumber")),
    ("aperture", FieldKeys(exiftool="EXIF:ApertureValue")),
    ("iso", FieldKeys(exiftool="EXIF:ISO")),
    ("focal_length", FieldKeys(exiftool="EXIF:FocalLength")),
    ("exposure_bias", FieldKeys(exiftool="EXIF:ExposureCompensation")),
    ("exposure_program", FieldKeys(exiftool="EXIF:ExposureProgram")),
    ("metering_mode", FieldKeys(exiftool="EXIF:MeteringMode")),
    ("flash", FieldKeys(exiftool="EXIF:Flash")),
    ("white_balance", FieldKeys(exiftool="EXIF:WhiteBalance")),
    ("serial_number", FieldKeys(exiftool="EXIF:SerialNumber")),
    ("created", FieldKeys(xmp="xmp:CreateDate", exiftool="EXIF:DateTimeOriginal", media="creation_time")),
    ("modified", FieldKeys(xmp="xmp:ModifyDate", exiftool="EXIF:ModifyDate")),
    ("altitude", FieldKeys(exiftool="EXIF:GPSAltitude")),
    ("gps_direction", FieldKeys(exiftool="EXIF:GPSImgDirection")),
    ("gps_timestamp", FieldKeys(exiftool="EXIF:GPSDateStamp")),
    ("map_datum", FieldKeys(exiftool="EXIF:GPSMapDatum")),
    ("city", FieldKeys(xmp="photoshop:City", exiftool="XMP-photoshop:City")),
    ("state", FieldKeys(xmp="photoshop:State", exiftool="XMP-photoshop:State")),
    ("country", FieldKeys(xmp="photoshop:Country", exiftool="XMP-photoshop:Country")),
    ("country_code", FieldKeys(xmp="Iptc4xmpCore:CountryCode", exiftool="XMP-iptcCore:CountryCode")),
    ("sublocation", FieldKeys(xmp="Iptc4xmpCore:Location", exiftool="XMP-iptcCore:Location")),
    ("document_id", FieldKeys(xmp="xmpMM:DocumentID", exiftool="XMP-xmpMM:DocumentID")),
    ("instance_id", FieldKeys(xmp="xmpMM:InstanceID", exiftool="XMP-xmpMM:InstanceID")),
    ("original_id", FieldKeys(xmp="xmpMM:OriginalDocumentID", exiftool="XMP-xmpMM:OriginalDocumentID")),
    ("space", FieldKeys(exiftool="EXIF:ColorSpace")),
    ("icc_name", FieldKeys(exiftool="ICC_Profile:ProfileDescription")),
    ("render_intent", FieldKeys(exiftool="ICC_Profile:RenderingIntent")),
    ("icc_maker", FieldKeys(exiftool="ICC_Profile:DeviceManufacturer")),
    ("icc_model", FieldKeys(exiftool="ICC_Profile:DeviceModel")),
    ("icc_copyright", FieldKeys(exiftool="ICC_Profile:ProfileCopyright")),
])

_KTX_DESCRIPTIVE: Final[frozenset[str]] = frozenset(keys.ktx for keys in _FIELD_KEYS.values() if keys.ktx)

_SETTINGS: Final[MetaSettings] = MetaSettings()
_EXIFTOOL: Final[str] = _SETTINGS.exiftool or shutil.which("exiftool") or "exiftool"

_NUMERIC: Final[frozenset[str]] = frozenset(
    info.name
    for facet in (Descriptive, Rights, Capture, Place, History, Color)
    for info in structs.fields(facet)
    if frozenset({bool, int, float}) & frozenset(get_args(info.type) or (info.type,))
)
_EXIFTOOL_TAGS: Final[tuple[str, ...]] = (
    *(f"{keys.exiftool}#" if logical in _NUMERIC else keys.exiftool for logical, keys in _FIELD_KEYS.items() if keys.exiftool),
    "Composite:GPSLatitude#",
    "Composite:GPSLongitude#",
    "File:ImageWidth",
    "File:ImageHeight",
    "File:MIMEType",
)

_CARRIER: Final[Map[MetaCarrier, CarrierPolicy]] = Map.of_seq([
    (MetaCarrier.RASTER, CarrierPolicy(reader=_exiftool_read, writer=_exiftool_write, trait=KernelTrait.HOSTILE)),
    (MetaCarrier.PDF, CarrierPolicy(reader=_read_pdf, writer=_write_pdf, trait=KernelTrait.RELEASING)),
    (MetaCarrier.MEDIA, CarrierPolicy(reader=_read_media, writer=_write_media, trait=KernelTrait.HOSTILE)),
    (MetaCarrier.TEXTURE, CarrierPolicy(reader=_ktx_read, writer=_ktx_write, trait=KernelTrait.HOSTILE)),
])

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = (
    "Capture",
    "CarrierPolicy",
    "Chapter",
    "Color",
    "Descriptive",
    "FieldKeys",
    "History",
    "MediaInfo",
    "MetaBind",
    "MetaCarrier",
    "MetaFacts",
    "MetaReader",
    "MetaSettings",
    "MetaWriter",
    "Metadata",
    "Place",
    "RasterInfo",
    "Rights",
    "TextureInfo",
    "Track",
    "WriteSpec",
)
```

## [03]-[RESEARCH]

<!-- source-only: research row template; every landed row opens on the list dash this placeholder omits, the census reading `^- [TOKEN]-[OPEN|BLOCKED]:` alone:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
