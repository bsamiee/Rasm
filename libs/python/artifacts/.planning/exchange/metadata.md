# [PY_ARTIFACTS_METADATA]

The descriptive-metadata read/write owner at the exchange boundary. `Metadata` is ONE owner over the EXIF / XMP / IPTC descriptive-metadata standards binding and recovering author/title/copyright/keyword/GPS/capture fields on an already-emitted raster or PDF artifact, discriminating `MetaOp` (`ReadExif`/`WriteExif`/`ReadXmp`/`WriteXmp`/`ReadIptc`/`WriteIptc`) by the standard-and-direction payload shape — never a per-format reader family and never a per-tag accessor proliferation. The metadata standard is the closed `MetaStandard` axis (`EXIF` over `exif` `Image`, `XMP` over the admitted `pikepdf` `Pdf.open_metadata()` scalar path with `python-xmp-toolkit` companion-gated for compound RDF, `IPTC` over `iptcinfo3` `IPTCInfo`), so a new standard is one axis row plus one acceptor, never a parallel owner. The PDF/XMP scalar path resolves in-process on the cp315-core pikepdf wheel; the EXIF/IPTC raster paths ride `exif`/`iptcinfo3` which are NOT-yet-admitted, so those arms are signature-locked behind the gated `anyio.to_process.run_sync` band and realization waits on manifest admission. Every operation contributes one `core/receipt#RECEIPT` `ArtifactReceipt.Metadata(key, standard, fields, byte_len)` descriptive-metadata fact keyed by the runtime content key — the standard the `MetaStandard` value, the field count the populated `MetaFacts` slice, the byte length the read/re-encoded payload — never a re-minted identity and never a producer value object crossing into the receipt owner.

## [01]-[INDEX]

- [01]-[METADATA]: descriptive-metadata read/write owner over EXIF (`exif` `Image`), XMP (`pikepdf` `Pdf.open_metadata()` scalar path admitted, `python-xmp-toolkit` compound companion-gated), and IPTC (`iptcinfo3` `IPTCInfo`) — the closed `MetaOp` read/write family folding into one typed `MetaFacts` over the `MetaStandard` axis, the `_STANDARD_TABLE` acceptor dispatch, and the content-keyed metadata owner contributing the `core/receipt#RECEIPT` `ArtifactReceipt.Metadata` descriptive-metadata facts; the pikepdf XMP scalar arm runs in-process, the `exif`/`iptcinfo3` raster arms are signature-locked behind the gated band pending admission.

## [02]-[METADATA]

- Owner: `Metadata` the one descriptive-metadata owner discriminating standard-and-direction over the closed `MetaOp` family; `MetaOp` an `expression.tagged_union` whose every case carries its own typed payload (the asset bytes plus, on the write arms, the `MetaFacts` to bind), never a shared erased `params` dict; `MetaStandard` the closed `StrEnum` axis (`EXIF`/`XMP`/`IPTC`) whose row selects the acceptor pair; `MetaFacts` the one typed result every read arm folds into and every write arm consumes — `title`/`author`/`copyright`/`description`/`keywords`/`created`/`gps`/`software` recovering the descriptive field set across all three standards in one shape, projected to `core/receipt#RECEIPT` descriptive-metadata facts. The `_STANDARD_TABLE` is the standard collapse: a row carries the `(read, write)` acceptor pair a `MetaStandard` keys, the op routes by one table lookup over `(standard, direction)`, never a per-standard reader/writer class family and never a re-discriminating `match` inside an arm. The pikepdf `PdfMetadata` mapping is the admitted scalar XMP carrier; the `exif` `Image` and `iptcinfo3` `IPTCInfo` raster carriers are signature-locked pending admission.
- Cases: `MetaOp` cases — `ReadExif(payload)`/`WriteExif(payload, facts)` (`exif` `Image(payload)` reading/assigning the EXIF tag set — `make`/`model`/`datetime_original`/`gps_latitude`/`copyright`/`artist`/`software` — then `get_file()` re-encoding the raster) · `ReadXmp(payload)`/`WriteXmp(payload, facts)` (the admitted `pikepdf` `Pdf.open(payload)` then `open_metadata()` `PdfMetadata` mapping read/assign over the Dublin-Core/XMP-Basic namespaces — `dc:title`/`dc:creator`/`dc:rights`/`dc:description`/`dc:subject`/`xmp:CreateDate`/`xmp:CreatorTool` — the scalar XMP path, `python-xmp-toolkit` `XMPMeta` the compound-RDF companion when nested structures or non-PDF sidecar XMP is required) · `ReadIptc(payload)`/`WriteIptc(payload, facts)` (`iptcinfo3` `IPTCInfo(stream)` reading/assigning the IPTC-IIM keyword/caption/byline set — `keywords`/`caption/abstract`/`by-line`/`copyright notice`/`object name` — then `save_as` re-encoding) — matched by one total `match`/`case` over the `(standard, direction)` pair the `_STANDARD_TABLE` row resolves; the standard axis is the `MetaStandard` row, the direction axis is the read/write acceptor selection, never a parallel reader/writer type per standard.
- Entry: `Metadata.of` is `async` over the runtime `async_boundary` and dispatches the `MetaOp` case, returning one `RuntimeRail[ArtifactReceipt]` — the read arms fold the recovered facts and key the source bytes, the write arms bind the facts and key the re-encoded bytes through `ContentIdentity.of`, both projecting the `core/receipt#RECEIPT` `ArtifactReceipt.Metadata(key, standard, fields, byte_len)` case — never a per-tag accessor or a `read_exif`/`write_exif`/`read_xmp` method family. The pikepdf XMP scalar arm (`ReadXmp`/`WriteXmp`) resolves synchronously in-process over the cp315-core pikepdf wheel (`cp314-abi3`, ungated) folded onto `anyio.to_thread.run_sync` so the rail never blocks the event loop; the `exif`/`iptcinfo3` raster arms (`ReadExif`/`WriteExif`/`ReadIptc`/`WriteIptc`) cross the `faults`-owned `anyio.to_process.run_sync` gated-band seam onto the worker that imports `exif`/`iptcinfo3` at boundary scope — but those two packages are NOT-yet-admitted, so the gated arms are signature-locked: the fence rows the settled member surface and the realization waits on the manifest admission, never running unverified.
- Auto: `_run` folds the case through one `match` and the `_STANDARD_TABLE[standard]` acceptor pair — the XMP read holds one `pikepdf.open(BytesIO(payload))` then `open_metadata(set_pikepdf_as_editor=False)` context whose mapping read projects the Dublin-Core/XMP-Basic keys into `MetaFacts`, and the XMP write opens the same context with `set_pikepdf_as_editor=True, update_docinfo=True`, assigns the namespace keys from `MetaFacts`, and saves the document to a fresh `BytesIO`; the EXIF arms construct `exif.Image(payload)`, read the tag attributes into `MetaFacts` (or assign them and re-encode through `get_file()`); the IPTC arms construct `iptcinfo3.IPTCInfo(BytesIO(payload))`, read the IIM dict into `MetaFacts` (or assign and `save_as`). `MetaFacts.from_xmp`/`from_exif`/`from_iptc` are the three projection constructors folding each standard's native key set into the one shape in a single walk, and `MetaFacts.to_xmp`/`to_exif`/`to_iptc` the inverse binders, never a per-tag getter/setter pair; the pikepdf `load_from_docinfo` migrates a legacy `/Info` dictionary into XMP before the read so the recovered facts cover both the modern XMP and the legacy docinfo.
- Receipt: each operation folds into `MetaFacts` and contributes the settled `core/receipt#RECEIPT` `ArtifactReceipt.Metadata(key, standard, fields, byte_len)` case the ARCHITECTURE `[02]-[SEAMS]` `exchange/metadata → python:artifacts/core/receipt` edge names — the standard the `MetaStandard` value the op derives, the field count the populated `MetaFacts` slice (the recovered/bound title, author, copyright, description, keyword-set, capture timestamp, GPS, and software fields the `_field_count` tally measures), the byte length the read/re-encoded payload — never a per-op or per-standard receipt; the write arms key the re-encoded bytes through `ContentIdentity.of` so a metadata-bound artifact carries a fresh content key the persistence store re-derives, and the read arms key the source bytes unchanged. The `core/receipt#RECEIPT` union carries the flat-scalar `Metadata` case (`metadata: tuple[ContentKey, str, int, int]`) mirroring the flat-scalar `Credential`/`Media` cases, so this owner spreads the `MetaStandard`/field-count/byte-length slice onto it and the receipt owner imports no `MetaFacts` value object — the case lives on the receipt page, never minted here.
- Packages: `pikepdf` (admitted: `Pdf.open`, `Pdf.open_metadata(set_pikepdf_as_editor=, update_docinfo=) -> PdfMetadata`, the `PdfMetadata` mapping with `load_from_docinfo`/`pdfa_status`/`pdfx_status`, `Pdf.docinfo` the raw `/Info` dictionary, `Pdf.save`) the cp315-core in-process XMP scalar carrier; `exif` (RESEARCH, NOT-yet-admitted: `Image(img_bytes)`, the `make`/`model`/`datetime_original`/`gps_latitude`/`gps_longitude`/`copyright`/`artist`/`software`/`orientation` tag attributes, `Image.has_exif`, `Image.list_all()`, `Image.get_file()` re-encode) the EXIF raster carrier signature-locked pending admission; `iptcinfo3` (RESEARCH, NOT-yet-admitted: `IPTCInfo(stream)`, the `keywords`/`caption/abstract`/`by-line`/`copyright notice`/`object name`/`headline` IIM keys, `IPTCInfo.save_as(path)`) the IPTC raster carrier signature-locked pending admission; `python-xmp-toolkit` (companion-gated: `XMPMeta`/`XMPFiles` for compound nested-RDF or non-PDF sidecar XMP, beyond the pikepdf scalar path) deferred until a compound-XMP consumer lands; `expression` (`tagged_union`/`tag`/`case`); `msgspec` (`Struct(frozen=True)` for the `MetaFacts` value owner); runtime (`content_identity.ContentIdentity`/`ContentKey`, `faults.RuntimeRail`/`async_boundary` and the `faults`-owned `anyio.to_process.run_sync` gated-band seam the EXIF/IPTC arms cross); `core/receipt#RECEIPT` (`ArtifactReceipt` the shared receipt family, the `Metadata` case this owner contributes flat scalars onto).
- Growth: a new metadata standard is one `MetaStandard` row plus one `_STANDARD_TABLE` `(read, write)` acceptor pair plus one `MetaFacts.from_*`/`to_*` projection pair; a new descriptive field is one field on `MetaFacts` plus one key in each standard's projection; a new operation direction reuses the existing read/write acceptor pair; a compound-XMP nested-RDF read is the `python-xmp-toolkit` `XMPMeta` companion arm folded onto the existing `XMP` row once admitted; the EXIF/IPTC arms realize on admission with zero fence change (signature-locked today); zero new surface.
- Boundary: a per-format reader (`JpegExifReader`/`PngExifReader`), a per-tag getter/setter accessor family (`get_author`/`set_author`/`get_title`/`set_title`), a per-standard `ExifMeta`/`XmpMeta`/`IptcMeta` class family, a hand-rolled APP1/XMP-packet/IIM-marker codec where `exif`/`pikepdf`/`iptcinfo3` own the standard, an erased `dict[str, Any]` metadata bag the consumer re-validates, and an identity re-mint the runtime `content_identity` owns are the deleted forms; this owner reads and writes descriptive metadata on already-emitted bytes and produces no artifact. The XMP scalar path is the admitted pikepdf `Pdf.open_metadata()` `PdfMetadata` mapping — the one settled in-process arm — while the compound nested-RDF path defers to the companion-gated `python-xmp-toolkit` until a consumer needs it, never hand-rolling RDF/XML; the EXIF and IPTC raster paths are signature-locked behind the gated band because `exif` and `iptcinfo3` are NOT-yet-admitted on the manifest, so the fence rows the verified-against-source member surface and the realization waits on admission rather than running an unverified arm. The descriptive-metadata facts are orthogonal to the `exchange/detect#DETECT` format-identification gate (detect sniffs the container type, metadata reads the descriptive fields inside it) and to the `exchange/credential#CREDENTIAL` C2PA provenance bind (credential is the signed tamper-evident manifest, metadata is the plain descriptive field set), each contributing its own fact slice to the shared `ArtifactReceipt`.

```python signature
from collections.abc import Callable
from io import BytesIO
from enum import StrEnum
from types import MappingProxyType
from typing import Any, Final, Literal, assert_never

from anyio import to_process, to_thread
from expression import case, tag, tagged_union
from msgspec import Struct

from rasm.runtime.content_identity import ContentIdentity, ContentKey
from rasm.runtime.faults import RuntimeRail, async_boundary

from artifacts.core.receipt import ArtifactReceipt

type MetaReader = Callable[[bytes], "MetaFacts"]
type MetaWriter = Callable[[bytes, "MetaFacts"], bytes]


class MetaStandard(StrEnum):
    EXIF = "exif"
    XMP = "xmp"
    IPTC = "iptc"


class MetaFacts(Struct, frozen=True):
    title: str = ""
    author: str = ""
    copyright: str = ""
    description: str = ""
    keywords: tuple[str, ...] = ()
    created: str = ""
    gps: tuple[float, float] | None = None
    software: str = ""

    def facts(self) -> dict[str, str]:
        return {
            "title": self.title,
            "author": self.author,
            "copyright": self.copyright,
            "keywords": str(len(self.keywords)),
            "created": self.created,
            "software": self.software,
        }

    @property
    def populated(self) -> int:
        scalars = (self.title, self.author, self.copyright, self.description, self.created, self.software)
        return sum(1 for value in scalars if value) + (1 if self.keywords else 0) + (1 if self.gps else 0)


@tagged_union(frozen=True)
class MetaOp:
    tag: Literal["read_exif", "write_exif", "read_xmp", "write_xmp", "read_iptc", "write_iptc"] = tag()
    read_exif: tuple[bytes] = case()
    write_exif: tuple[bytes, MetaFacts] = case()
    read_xmp: tuple[bytes] = case()
    write_xmp: tuple[bytes, MetaFacts] = case()
    read_iptc: tuple[bytes] = case()
    write_iptc: tuple[bytes, MetaFacts] = case()

    @property
    def standard(self) -> MetaStandard:
        return MetaStandard(self.tag.split("_", 1)[1])


class Metadata(Struct, frozen=True):
    op: MetaOp

    async def of(self) -> RuntimeRail[ArtifactReceipt]:
        return await async_boundary(f"metadata.{self.op.tag}", self._emit)

    async def _emit(self) -> ArtifactReceipt:
        payload, facts = await self._run()
        key = ContentIdentity.of(f"metadata-{self.op.tag}", payload)
        return ArtifactReceipt.Metadata(key, self.op.standard.value, facts.populated, len(payload))

    async def _run(self) -> tuple[bytes, MetaFacts]:
        match self.op:
            case MetaOp(tag="read_xmp", read_xmp=(payload,)):
                return payload, await to_thread.run_sync(_read_xmp, payload)
            case MetaOp(tag="write_xmp", write_xmp=(payload, facts)):
                return await to_thread.run_sync(_write_xmp, payload, facts), facts
            case MetaOp(tag="read_exif", read_exif=(payload,)):
                return payload, await to_process.run_sync(_read_exif, payload)
            case MetaOp(tag="write_exif", write_exif=(payload, facts)):
                return await to_process.run_sync(_write_exif, payload, facts), facts
            case MetaOp(tag="read_iptc", read_iptc=(payload,)):
                return payload, await to_process.run_sync(_read_iptc, payload)
            case MetaOp(tag="write_iptc", write_iptc=(payload, facts)):
                return await to_process.run_sync(_write_iptc, payload, facts), facts
            case _:
                assert_never(self.op)


# --- pikepdf XMP scalar path (admitted, in-process) -------------------------------------

_XMP_KEYS: Final[MappingProxyType[str, str]] = MappingProxyType({
    "title": "dc:title",
    "author": "dc:creator",
    "copyright": "dc:rights",
    "description": "dc:description",
    "created": "xmp:CreateDate",
    "software": "xmp:CreatorTool",
})


def _read_xmp(payload: bytes) -> MetaFacts:
    import pikepdf

    with pikepdf.open(BytesIO(payload)) as pdf, pdf.open_metadata(set_pikepdf_as_editor=False, update_docinfo=True) as meta:
        return MetaFacts(
            title=str(meta.get("dc:title", "")),
            author=str(meta.get("dc:creator", "")),
            copyright=str(meta.get("dc:rights", "")),
            description=str(meta.get("dc:description", "")),
            keywords=tuple(meta.get("dc:subject", ())),
            created=str(meta.get("xmp:CreateDate", "")),
            software=str(meta.get("xmp:CreatorTool", "")),
        )


def _write_xmp(payload: bytes, facts: MetaFacts) -> bytes:
    import pikepdf

    sink = BytesIO()
    with pikepdf.open(BytesIO(payload)) as pdf:
        with pdf.open_metadata(set_pikepdf_as_editor=True, update_docinfo=True) as meta:
            for field, key in _XMP_KEYS.items():
                value = getattr(facts, field)
                if value:
                    meta[key] = value
            if facts.keywords:
                meta["dc:subject"] = list(facts.keywords)
        pdf.save(sink)
    return sink.getvalue()


# --- exif raster path (RESEARCH, signature-locked pending admission) ---------------------

def _read_exif(payload: bytes) -> MetaFacts:
    # RESEARCH: `exif` is NOT-yet-admitted on the manifest. The `exif.Image(payload)` tag-attribute
    # surface (`make`/`model`/`datetime_original`/`gps_latitude`/`gps_longitude`/`copyright`/`artist`/
    # `software`, `has_exif`, `list_all`, `get_file`) is signature-locked against the package source and
    # realizes verbatim once `exif` is admitted; the arm never runs unverified.
    from exif import Image

    image = Image(payload)
    if not image.has_exif:
        return MetaFacts()
    gps = (image.get("gps_latitude"), image.get("gps_longitude"))
    return MetaFacts(
        author=str(image.get("artist", "")),
        copyright=str(image.get("copyright", "")),
        created=str(image.get("datetime_original", "")),
        gps=(gps[0], gps[1]) if gps[0] is not None and gps[1] is not None else None,
        software=str(image.get("software", "")),
    )


def _write_exif(payload: bytes, facts: MetaFacts) -> bytes:
    from exif import Image

    image = Image(payload)
    if facts.author:
        image.artist = facts.author
    if facts.copyright:
        image.copyright = facts.copyright
    if facts.created:
        image.datetime_original = facts.created
    if facts.software:
        image.software = facts.software
    return image.get_file()


# --- iptcinfo3 raster path (RESEARCH, signature-locked pending admission) ----------------

def _read_iptc(payload: bytes) -> MetaFacts:
    # RESEARCH: `iptcinfo3` is NOT-yet-admitted on the manifest. The `IPTCInfo(stream)` IIM-key surface
    # (`keywords`/`caption/abstract`/`by-line`/`copyright notice`/`object name`/`headline`, `save_as`) is
    # signature-locked against the package source and realizes verbatim once `iptcinfo3` is admitted.
    from iptcinfo3 import IPTCInfo

    info = IPTCInfo(BytesIO(payload), force=True)
    return MetaFacts(
        title=str(info["object name"] or ""),
        author=str(info["by-line"] or ""),
        copyright=str(info["copyright notice"] or ""),
        description=str(info["caption/abstract"] or ""),
        keywords=tuple(k.decode() if isinstance(k, bytes) else k for k in info["keywords"]),
    )


def _write_iptc(payload: bytes, facts: MetaFacts) -> bytes:
    from iptcinfo3 import IPTCInfo

    info = IPTCInfo(BytesIO(payload), force=True)
    if facts.title:
        info["object name"] = facts.title
    if facts.author:
        info["by-line"] = facts.author
    if facts.copyright:
        info["copyright notice"] = facts.copyright
    if facts.description:
        info["caption/abstract"] = facts.description
    if facts.keywords:
        info["keywords"] = list(facts.keywords)
    sink = BytesIO()
    info.save_as(sink)
    return sink.getvalue()
```

## [03]-[RESEARCH]

- [XMP_SCALAR_ADMITTED] [RESOLVED]: the pikepdf `Pdf.open_metadata(set_pikepdf_as_editor=True, update_docinfo=True) -> PdfMetadata` context-manager XMP edit (`.api` `[03]-[ENTRYPOINTS]` open/create/save row `[08]`), the `models.PdfMetadata` XMP-metadata mapping with `load_from_docinfo`/`pdfa_status`/`pdfx_status` (`[02]-[PUBLIC_TYPES]` model-helper row `[02]`), the `Pdf.docinfo` raw `/Info` dictionary, `Pdf.open`/`Pdf.save` (entrypoint rows `[01]`/`[03]`), and the `[04]-[IMPLEMENTATION_LAW]` metadata axis ("`Pdf.open_metadata` yields a `PdfMetadata` mapping with `load_from_docinfo` legacy `/Info` -> XMP migration, `pdfa_status`/`pdfx_status` conformance probes, and namespace registration; `Pdf.docinfo` is the raw `/Info` dictionary, never a parallel metadata model") verify against the folder `pikepdf` `.api` (`10.9.1`, `cp314-abi3`, ungated). The scalar XMP path is the admitted in-process arm — the `PdfMetadata` mapping is a `dict`-like over the Dublin-Core (`dc:title`/`dc:creator`/`dc:rights`/`dc:description`/`dc:subject`) and XMP-Basic (`xmp:CreateDate`/`xmp:CreatorTool`) namespaces, the `dc:subject` keyword set a list-valued key — so `_read_xmp`/`_write_xmp` are settled fence code running on the cp315-core pikepdf wheel folded onto `anyio.to_thread.run_sync`, never the gated `to_process` band. The compound nested-RDF path (`python-xmp-toolkit` `XMPMeta`/`XMPFiles`) is companion-gated: it owns nested-structure XMP and non-PDF sidecar XMP beyond the pikepdf scalar mapping, deferred until a compound-XMP consumer lands, never hand-rolling RDF/XML on the scalar path.
- [EXIF_LOCKED] [RESEARCH]: `exif` is NOT-yet-admitted on the manifest. The `exif.Image(img_bytes)` constructor, the tag-attribute surface (`make`/`model`/`datetime_original`/`gps_latitude`/`gps_longitude`/`copyright`/`artist`/`software`/`orientation`/`lens_make`/`lens_model`), the `Image.has_exif` presence flag, the `Image.get(tag, default)` safe accessor, the `Image.list_all()` tag enumeration, and the `Image.get_file()` re-encode are signature-locked against the package source (the `exif` package is the pure-Python EXIF read/write library over the JPEG/TIFF APP1 segment, a tag-attribute model where each EXIF tag is a settable attribute and `get_file()` re-serializes the marker). The `_read_exif`/`_write_exif` arms ride the gated `anyio.to_process.run_sync` band — `exif` is pure-Python so the gate is admission, not a native host dependency — and realize verbatim once `exif` is admitted on the manifest; the arm never runs unverified. Close-condition: `exif` admitted on the manifest, the `.api` catalogue rows the `Image` tag-attribute surface, and an `assay api resolve exif` reflection pass confirms the GPS-coordinate accessor spelling (`gps_latitude` as a `(deg, min, sec)` tuple vs a decimal float — the one spelling the fence's `gps` tuple projection depends on).
- [IPTC_LOCKED] [RESEARCH]: `iptcinfo3` is NOT-yet-admitted on the manifest. The `iptcinfo3.IPTCInfo(stream_or_path, force=True)` constructor, the IIM-key mapping access (`info["keywords"]` list-valued, `info["caption/abstract"]`/`info["by-line"]`/`info["copyright notice"]`/`info["object name"]`/`info["headline"]` byte-or-str-valued IIM datasets), and the `IPTCInfo.save_as(dest)` re-encode are signature-locked against the package source (the `iptcinfo3` package is the pure-Python IPTC-IIM read/write library over the JPEG APP13/Photoshop IRB marker, a `dict`-like over the IIM dataset keys where `keywords` is a `list[bytes]` and `force=True` tolerates a missing IPTC block). The `_read_iptc`/`_write_iptc` arms ride the gated `anyio.to_process.run_sync` band — admission-gated, not host-native — and realize verbatim once `iptcinfo3` is admitted; the arm never runs unverified. Close-condition: `iptcinfo3` admitted on the manifest, the `.api` catalogue rows the `IPTCInfo` IIM-key surface, and an `assay api resolve iptcinfo3` reflection pass confirms the keyword `bytes`-vs-`str` return spelling (the one spelling the fence's `decode()` keyword projection depends on) and the `save_as` sink-vs-path argument.
- [META_RECEIPT_CASE] [RESOLVED]: the descriptive-metadata `ArtifactReceipt` contribution is the ARCHITECTURE `[02]-[SEAMS]` `exchange/metadata → python:artifacts/core/receipt` descriptive-metadata facts edge — the standard, the populated descriptive-field count, and the payload byte length threaded onto the shared `core/receipt#RECEIPT` `ArtifactReceipt.Metadata` case. The `core/receipt#RECEIPT` owner carries the flat-scalar `Metadata` case `metadata: tuple[ContentKey, str, int, int]` (key, standard, fields, byte_len), its `Metadata(key, standard, fields, byte_len)` constructor, its `_facts` arm projecting `{"key", "standard", "fields", "bytes"}`, and its `"metadata"` tag `Literal` token, mirroring the flat-scalar `Credential`/`Media` cases so the receipt owner imports no `MetaFacts` value object. So `_emit` returns `ArtifactReceipt` directly: the read/write arms fold to `MetaFacts`, key the (source or re-encoded) payload through `ContentIdentity.of`, and project `ArtifactReceipt.Metadata(key, op.standard.value, facts.populated, len(payload))` — the standard the `MetaStandard` value the `MetaOp.standard` property strips off the op tag, the field count the `MetaFacts.populated` populated-slice tally, the byte length the read/re-encoded payload. The write arms key the re-encoded bytes through `ContentIdentity.of(f"metadata-{op.tag}", payload)` so a metadata-bound artifact carries a fresh content key the `csharp:Rasm.Persistence` store re-derives over the identical bytes, the read arms return the source bytes unchanged under their own key; the metadata owner re-mints no canonical content key (the runtime `content_identity` owns it), the binding a key thread.
```
