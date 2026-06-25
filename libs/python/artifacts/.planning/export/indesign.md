# [PY_ARTIFACTS_INDESIGN]

The IDML template-mutation hand-off — the editable InDesign deliverable authored by MUTATING an InDesign-exported `.idml` template, never synthesized from scratch. `Idml` is ONE owner over the IDML composition pipeline carrying a closed `IdmlOp` `expression.tagged_union` — a `Compose` case (insert one IDML's XML-tagged content into another at an XPath anchor), a `Combine` case (gather pages from several IDML documents into one), an `Import` case (populate the template's XML structure from an external XML document), and a `Place` case (drop a placed PDF into a block placeholder) — each carrying its own typed payload, never a `StrEnum` keyed against an erased `dict[str, object]` bag — dispatched by one total `match`+`assert_never`. It binds the placed layout `composition/compose#COMPOSE` produces into the IDML template the designer authored in InDesign: the template carries the named XML structure (the InDesign XML-ready document's tag tree), and this owner feeds content INTO that structure rather than emitting page geometry — keeping the data separate from the structure the way the IDML round-trip demands. The `Compose` arm prefixes each source package to avoid reference collisions and inserts the sub-template's tagged content at an XPath anchor; the `Combine` arm adds whole pages from sibling documents; the `Import` arm pushes an external XML document into the template's tag tree honoring the `simpleidml-setcontent`/`simpleidml-ignorecontent`/`simpleidml-forcecontent` content-control flags and the `href` image references; the `Place` arm drops a `composition/compose#COMPOSE`-produced PDF into a named block placeholder. Every side-effect operation returns a NEW package instance through a `with`-context (the IDML side-effect methods return fresh instances whose backing file must be closed), so the owner threads each mutation through the context boundary and serializes the final package bytes. One IDML hand-off surface discriminating the mutation kind, never a per-template writer family and never a from-scratch InDesign-document synthesizer. Every operation returns a `RuntimeRail[ContentKey]` over the runtime `async_boundary` and contributes the existing `core/receipt#RECEIPT` `ArtifactReceipt.Office` case (the IDML package bytes) carrying the mutated-package byte count, never a new receipt case.

## [01]-[INDEX]

- [01]-[INDESIGN]: IDML template-mutation hand-off owner over the closed-payload `IdmlOp` `tagged_union` dispatched to `SimpleIDML` (`IDMLPackage` `prefix`/`insert_idml`/`add_pages_from_idml`/`import_xml`/`import_pdf` template mutation over the InDesign-exported `.idml` package and its XML structure), binding the `composition/compose#COMPOSE` placed layout into the designer's template through its named XML tag tree, threading each side-effect mutation through the `with`-context new-instance boundary, and folding into the shared content key plus the `ArtifactReceipt.Office` case.

## [02]-[INDESIGN]

- Owner: `Idml` the one IDML template-mutation owner discriminating the mutation kind over the closed `IdmlOp` `expression.tagged_union` whose every case carries its own typed payload, never a `StrEnum` keyed against a shared erased `dict[str, object]`; `IdmlSource` the one frozen `msgspec.Struct` binding a template-or-sub-template input — `data` the IDML package bytes (an InDesign-exported `.idml` archive), `prefix` the namespace prefix the `IDMLPackage.prefix` mutation applies before any merge to avoid reference collisions, `at` the XPath anchor in the destination XML structure the inserted content lands at, `only` the XPath selector of the sub-tree to lift from the source — so a source is one row carrying its own bytes, prefix, and placement anchors rather than four parallel `datas`/`prefixes`/`ats`/`onlys` lists zipped at the call site; `PdfCrop` the closed `StrEnum` of IDML `PDFCrop_EnumValue` crop modes (`CONTENT_VISIBLE`/`CONTENT_ALL`/`ART`/`PDF`/`TRIM`/`BLEED`/`MEDIA`/`CONTENT`) projecting one `import_pdf(crop=)` policy through one frozen `_CROP` dispatch row so a bleed-cropped placement is one token, never a hand-typed crop-string literal per call; the `SimpleIDML` `IDMLPackage` is the template-mutation working surface, its `prefix`/`insert_idml`/`add_page_from_idml`/`add_pages_from_idml`/`import_xml`/`import_pdf` side-effect methods (each returning a NEW `with`-context instance) the mutation algebra, and its `spreads`/`stories`/`pages`/`xml_structure`/`export_xml` introspection surface the structure read.
- Cases: `IdmlOp` cases — `Compose(main, modules)` (template composition — prefix the `main` template and each `modules` sub-template through `IDMLPackage.prefix` to avoid reference collisions, then fold each prefixed module into the prefixed main through `main.insert_idml(module, at=module.at, only=module.only)` so the sub-template's XML-tagged content lands at its XPath anchor in the destination structure, each `insert_idml` returning a new `with`-context instance threaded into the next fold) · `Combine(main, pages)` (page combination — prefix the `main` and each page source, then gather whole pages from the sibling documents through `main.add_pages_from_idml([(module, page_number, at, only) …])` so the destination page count grows by the combined pages and the XML structure integrates each added file) · `Import(template, xml)` (XML content import — prefix the `template`, then push the external `xml` document into the template's XML tag tree through `template.import_xml(xml, at=at)` honoring the `simpleidml-setcontent`/`simpleidml-ignorecontent`/`simpleidml-forcecontent` content-control flags and resolving each `href` image reference, so the data populates the structure the designer authored without touching page geometry) · `Place(template, pdf, at, crop)` (PDF placement — drop the `composition/compose#COMPOSE`-produced `pdf` into a named block placeholder through `template.import_pdf(pdf_url, at=at, crop=crop.value)` so the placed flat layout lands inside the InDesign block as a linked PDF the designer re-links and re-crops) — matched by one total `match`/`case`+`assert_never`; never a sibling op per template, never a per-mutation writer family, never an `if compose`/`if import` branch re-deriving the mutation the case already names.
- Entry: `Idml.of` is `async` over the runtime `async_boundary`, dispatches the `IdmlOp` case, and returns a `RuntimeRail[ContentKey]`; every arm materializes each `IdmlSource.data` to a temp IDML file (the `IDMLPackage` constructor opens a path-backed Zip package), threads each side-effect mutation through the `with`-context that returns a fresh instance and closes the prior backing file (the IDML side-effect contract — a lost initial reference leaves an unclosed file the platform cannot unlink), reads the final package bytes off the last instance's backing file, and keys the result through `ContentIdentity.of` over those bytes. SimpleIDML is a pure-Python `IDMLPackage`-over-`zipfile`+`lxml` library expected cp315-clean on the core, so the IDML mutation runs in-capsule with no subprocess seam; the InDesign-Server SOAP conversion path (`simple_idml.indesign.indesign.save_as`) requires a live InDesign Server and a shared working directory and is OUT OF SCOPE — this owner produces the editable `.idml` deliverable, never an `.indd`/`.pdf` server-rendered conversion.
- Auto: `_mutate` folds the op through one `match`+`assert_never` — the `Compose` arm opens the `main` template as an `IDMLPackage`, prefixes it through `main.prefix(main.prefix)`, opens and prefixes each module, then folds each prefixed module into the running package through `package.insert_idml(module, at=source.at, only=source.only)` carrying each returned new instance forward through the `with`-context chain so the reference collisions the prefix prevents never form; the `Combine` arm prefixes the main and each page source, assembles the `[(module, page_number, source.at, source.only) …]` tuple list, and folds it through `package.add_pages_from_idml(specs)` returning the combined package; the `Import` arm prefixes the template and pushes the external XML through `package.import_xml(xml.decode(), at=at)` so the content-control flags and `href` references resolve against the template's tag tree; the `Place` arm writes the `pdf` bytes to a temp path, builds the `file:` URL the IDML block placeholder links, and places it through `package.import_pdf(pdf_url, at=at, crop=crop.value)`; every arm reads the final package bytes off the closed backing file and keys them through `ContentIdentity.of`, never a re-minted seed.
- Receipt: each operation contributes `core/receipt#RECEIPT` `ArtifactReceipt.Office(key, byte_count)` carrying the mutated IDML package's byte count — the same `Office` case the OOXML/ODF office producers contribute (an IDML package IS an Office-class structured-document archive, so a parallel `Idml` receipt case is the rejected form); IDML hand-off adds NO new receipt case — the package-bytes fact is the `Office` byte-count shape, settled.
- Packages: `SimpleIDML` ([RESEARCH] not-yet-admitted, admission-pending, signature-locked — `from simple_idml import idml`/`idml.IDMLPackage(path)` the package root, `IDMLPackage.prefix(prefix) -> IDMLPackage` the collision-avoidance namespace mutation, `IDMLPackage.insert_idml(other, at, only) -> IDMLPackage` the XPath-anchored content insertion, `IDMLPackage.add_page_from_idml(other, page_number, at, only) -> IDMLPackage`/`add_pages_from_idml(specs) -> IDMLPackage` the page combination, `IDMLPackage.import_xml(xml, at) -> IDMLPackage` the external-XML content import, `IDMLPackage.import_pdf(url, at, crop) -> IDMLPackage` the PDF-into-block placement, `IDMLPackage.export_xml() -> str`/`spreads`/`stories`/`pages`/`xml_structure`/`xml_structure_pretty()` the introspection surface, each side-effect method context-managed and returning a NEW instance, Python `3.9+` pure-Python over `zipfile`+`lxml` expected cp315-clean) on the cp315 core pending admission; `lxml` (the IDML XML-structure parse the `xml_structure`/`export_xml` surface returns `etree` Elements over, already admitted on the cp315 core, shared with `document/emit#EMIT`); `msgspec` (`Struct` frozen `IdmlSource` row, frozen `Idml`); runtime (`content_identity.ContentIdentity`/`ContentKey`, `faults.RuntimeRail`/`async_boundary`).
- Growth: a new template-mutation kind (a story-text replace, a spread reorder, a layer-swap) is one `IdmlOp` case plus one `_mutate` arm over the existing `IDMLPackage` mutation algebra — never a re-implemented IDML Zip/XML serializer; a new crop mode is one `PdfCrop` token carrying its own `_CROP` dispatch row threaded into `import_pdf(crop=)` — never a hand-typed crop string per call; a new source attribute (a target spread, a story-style override) is one field on the `IdmlSource` row threaded into the consuming `_mutate` arm — never a parallel attribute list; a new content-control flag is one entry in the imported XML the `import_xml` arm already honors, never a new mutation entrypoint. Zero new surface.
- Boundary: a from-scratch InDesign-document synthesizer, a per-template `_compose`/`_import`/`_place` writer family beside the one `_mutate` dispatch, a `datas`/`prefixes`/`ats` triple-list zipped at the call site beside the one `IdmlSource` row, a hand-typed `PDFCrop_EnumValue` literal per call beside the `PdfCrop` `_CROP` row, a hand-edited IDML Zip-member XML string beside the `IDMLPackage` mutation methods, and a `StrEnum`-plus-`dict[str, object]` erased-bag dispatch are the deleted forms; no UI, no live InDesign editor, no page-geometry synthesis, no `.indd`/`.pdf` server render. `SimpleIDML` owns the IDML package mutation — the Zip-archive structure, the named XML tag tree, the `prefix`/`insert_idml`/`add_pages_from_idml`/`import_xml`/`import_pdf` side-effect algebra, and the `export_xml`/`xml_structure` introspection — this owner composes those methods through the `with`-context new-instance boundary and re-implements none of the IDML serialization; the InDesign-Server SOAP `save_as` conversion to `.indd`/`.pdf`/`.jpeg` stays outside this package (it needs a live server and a shared working directory the host-free engine does not own). The placed flat layout arrives from `composition/compose#COMPOSE` (the base-graphic/registration-overlay/n-up-sheet PDF the `Tile`/`Overlay` arms place) and binds into the template as a placed PDF through the `Place` arm or as XML-tagged content through the `Import` arm; the editable-named-layer SVG/PDF/TIFF hand-off stays `export/layered#LAYERED`'s — IDML is the InDesign template-mutation complement, layered is the Illustrator/Acrobat/Photoshop named-layer complement, each meeting its native editor family. The content key is consumed from runtime over the mutated package bytes, never re-minted off a source key.

```python signature
from enum import StrEnum
from typing import Literal, assert_never

from expression import case, tag, tagged_union
from msgspec import Struct

from rasm.runtime.content_identity import ContentIdentity, ContentKey
from rasm.runtime.faults import RuntimeRail, async_boundary

from artifacts.core.receipt import ArtifactReceipt


class PdfCrop(StrEnum):
    CONTENT_VISIBLE = "CropContentVisibleLayers"
    CONTENT_ALL = "CropContentAllLayers"
    CONTENT = "CropContent"
    ART = "CropArt"
    PDF = "CropPDF"
    TRIM = "CropTrim"
    BLEED = "CropBleed"
    MEDIA = "CropMedia"


class IdmlSource(Struct, frozen=True):
    data: bytes
    prefix: str
    at: str = "/Root"
    only: str | None = None


@tagged_union(frozen=True)
class IdmlOp:
    tag: Literal["compose", "combine", "import_xml", "place"] = tag()
    compose: tuple[IdmlSource, tuple[IdmlSource, ...]] = case()
    combine: tuple[IdmlSource, tuple[tuple[IdmlSource, int], ...]] = case()
    import_xml: tuple[IdmlSource, bytes] = case()
    place: tuple[IdmlSource, bytes, str, PdfCrop] = case()

    @staticmethod
    def Compose(main: IdmlSource, modules: tuple[IdmlSource, ...]) -> "IdmlOp":
        return IdmlOp(compose=(main, modules))

    @staticmethod
    def Combine(main: IdmlSource, pages: tuple[tuple[IdmlSource, int], ...]) -> "IdmlOp":
        return IdmlOp(combine=(main, pages))

    @staticmethod
    def Import(template: IdmlSource, xml: bytes) -> "IdmlOp":
        return IdmlOp(import_xml=(template, xml))

    @staticmethod
    def Place(template: IdmlSource, pdf: bytes, at: str, crop: PdfCrop = PdfCrop.CONTENT_VISIBLE) -> "IdmlOp":
        return IdmlOp(place=(template, pdf, at, crop))


class Idml(Struct, frozen=True):
    op: IdmlOp

    async def of(self) -> RuntimeRail[ContentKey]:
        return await async_boundary(f"export.idml.{self.op.tag}", self._emit)

    async def _emit(self) -> ContentKey:
        data = _mutate(self.op)
        return ContentIdentity.of(f"idml-{self.op.tag}", data)


def _mutate(op: IdmlOp) -> bytes:  # [RESEARCH] simple_idml not yet admitted
    from simple_idml import idml

    match op:
        case IdmlOp(tag="compose", compose=(main, modules)):
            package = idml.IDMLPackage(_spill(main.data)).prefix(main.prefix)
            for module in modules:
                part = idml.IDMLPackage(_spill(module.data)).prefix(module.prefix)
                package = package.insert_idml(part, at=module.at, only=module.only)
            return _drain(package)
        case IdmlOp(tag="combine", combine=(main, pages)):
            package = idml.IDMLPackage(_spill(main.data)).prefix(main.prefix)
            specs = [
                (idml.IDMLPackage(_spill(src.data)).prefix(src.prefix), number, src.at, src.only)
                for src, number in pages
            ]
            return _drain(package.add_pages_from_idml(specs))
        case IdmlOp(tag="import_xml", import_xml=(template, xml)):
            package = idml.IDMLPackage(_spill(template.data)).prefix(template.prefix)
            return _drain(package.import_xml(xml.decode(), at=template.at))
        case IdmlOp(tag="place", place=(template, pdf, at, crop)):
            package = idml.IDMLPackage(_spill(template.data)).prefix(template.prefix)
            return _drain(package.import_pdf(_pdf_url(pdf), at=at, crop=crop.value))
        case _:
            assert_never(op)
```

## [03]-[RESEARCH]

- [IDML_TEMPLATE_MUTATION] [RESEARCH]: the owner is a TEMPLATE-MUTATION hand-off, not a from-scratch InDesign synthesizer — the InDesign deliverable is authored by mutating an InDesign-exported `.idml` template through `SimpleIDML`, verified against the official SimpleIDML README API surface (`Starou/SimpleIDML`, latest `v0.91.6`, active master pushed 2025-10-24, Python `3.9+`, pure-Python over `zipfile`+`lxml`): `from simple_idml import idml`/`idml.IDMLPackage("/path/to/doc.idml")` opens the Zip-archive package; `IDMLPackage.prefix(prefix)` namespaces every internal reference to avoid the collisions a merge would otherwise create (the README mandates prefixing before any `insert_idml`/page combination); `IDMLPackage.insert_idml(other, at="/Root/article[3]", only="/Root/module[1]")` inserts one document's XML-tagged content into another at an XPath anchor; `IDMLPackage.add_page_from_idml(other, page_number=1, at="/Root", only="/Root/page[1]")` and `add_pages_from_idml([(pkg, page, at, only), …])` gather whole pages from sibling documents; `IDMLPackage.import_xml(xml, at)` populates the template's XML tag tree from an external XML document honoring the `simpleidml-setcontent`/`simpleidml-ignorecontent`/`simpleidml-forcecontent` content-control flags and the `href` image references; `IDMLPackage.import_pdf(url, at, crop)` drops a PDF into a block placeholder with the crop one of the IDML `PDFCrop_EnumValue` (`CropContentVisibleLayers` default, plus `CropArt`/`CropPDF`/`CropTrim`/`CropBleed`/`CropMedia`/`CropContentAllLayers`/`CropContent`); `IDMLPackage.export_xml()`/`spreads`/`stories`/`pages`/`xml_structure`/`xml_structure_pretty()` introspect the structure. The README's load-bearing contract is the `with`-context discipline: every side-effect method RETURNS A NEW INSTANCE and the prior instance's backing file must be closed (a lost reference leaves an unclosed file the platform — notably Windows — cannot `os.unlink`), so the `_mutate` fold threads each mutation through the context boundary rather than reassigning a single handle. `SimpleIDML` is NOT-yet-admitted in `pyproject.toml` and the folder `.api` catalogue — the full member chain is SIGNATURE-LOCKED here pending admission: admit `simpleidml` on the cp315 core (pure-Python over the already-admitted `lxml`, expected cp315-clean), author its `.api` catalogue with assay-verified members, then resolve the exact `prefix`/`insert_idml`/`add_pages_from_idml`/`import_xml`/`import_pdf` return-shape and the `_spill`/`_drain`/`_pdf_url` temp-file plumbing against the verified surface. Until admitted the `_mutate` fence is the signature target, not realized source — the `from simple_idml import idml` boundary import marks the not-yet-admitted dependency.
- [COMPOSE_HANDOFF] [RESOLVED]: the placed layout binds into the IDML template through two complementary arms — the `Place` arm drops a `composition/compose#COMPOSE`-produced PDF into a named block placeholder through `import_pdf(pdf_url, at, crop)` so the placed flat layout lands inside the InDesign block as a linked, re-croppable PDF, and the `Import` arm pushes structured content into the template's XML tag tree through `import_xml(xml, at)` so the data populates the structure the designer authored. The compose owner produces the placed PDF and the structured XML; this owner binds them into the template the designer authored in InDesign, re-synthesizing no page geometry and re-laying-out nothing (the template owns the layout, the data owns the content — the IDML round-trip's data/structure separation). The content key arrives from `composition/compose#COMPOSE` and is re-minted over the mutated package bytes through `ContentIdentity.of`, never re-minted off a source key.
- [RECEIPT_REUSE] [RESOLVED]: IDML hand-off contributes the EXISTING `core/receipt#RECEIPT` `ArtifactReceipt.Office(key, byte_count)` case (`office: tuple[ContentKey, int]`) carrying the mutated IDML package's byte count — an IDML package IS an Office-class structured-document Zip archive (the same case the OOXML/ODF producers contribute), so a parallel `Idml`/`Indesign` receipt case is the rejected form. The owner imports `ArtifactReceipt` and `expression`/`msgspec`/`simple_idml`/`lxml` only, re-mints no content key, and projects the flat byte-count scalar so the receipt owner imports no producer value object — the `Office` reuse is the settled target, never a fifteenth case.
- [SERVER_CONVERSION_OUT_OF_SCOPE] [RESOLVED]: the SimpleIDML InDesign-Server SOAP conversion path (`simple_idml.indesign.indesign.save_as(path, [{"fmt": "indd"|"pdf"|"jpeg"|"idml"|"zip"}], server_url, client_workdir, server_workdir)`) converts an `.idml` to `.indd`/`.pdf`/`.jpeg` through a LIVE InDesign Server and a shared readable/writable working directory — this is OUT OF SCOPE for the host-free durable-output engine: it requires a network InDesign Server seat and a co-mounted filesystem the engine does not own, and the deliverable this owner produces is the editable `.idml` package the designer opens in InDesign directly. The `.indd`/`.pdf` server render is the rejected arm; the IDML template-mutation hand-off is the capability — a faithful editable InDesign deliverable authored host-free.
- [LAYERED_SIBLING_BOUNDARY] [RESOLVED]: IDML is the InDesign template-mutation hand-off; `export/layered#LAYERED` is the Illustrator/Acrobat/Photoshop named-layer hand-off (SVG `<g id=>` Groups, PDF OCG optional-content groups, layered TIFF). The two share the editable-export domain but own disjoint editor families and disjoint formats — IDML mutates a designer's `.idml` template through its XML structure, layered authors named layers into SVG/PDF/TIFF from the placed sources — so a layered arm grafted onto `IdmlOp` or an IDML arm grafted onto `ExportOp` is the rejected form. Both consume the same `composition/compose#COMPOSE` placed layout keyed by the same `ContentKey`; the named-layer binding the layered owner reads as a `Layer(name, source, bbox, …)` row is the IDML owner's `IdmlSource(data, prefix, at, only)` template-binding counterpart — each export owner binds the placed layout into its native editor format through its own typed source row, never a shared erased source bag.
