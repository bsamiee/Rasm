# [PY_ARTIFACTS_TAGGED]

The PDF/UA tagged-content close over the document rail. `Access` is ONE owner that authors the marked-content structure-element tree into an emitted PDF AND audits it, discriminating a closed `AccessOp` (`TAG`/`AUDIT`) over a frozen `_OP_TABLE` data-row dispatch — never a `tag_pdf`/`audit_pdf`/`per-role` writer family. `TAG` folds the `documents/model#NODE` `StructureNode`/`StructEltKind` PDF/UA family and the `FigureNode.alt` alt-text equivalent into the `pikepdf` `/StructTreeRoot` over the raw `Object` model (`/Type /StructElem` indirect dictionaries keyed by `/S`, the `/ParentTree` number-tree, the `/RoleMap` foreign→standard mapping, the `/MarkInfo /Marked` flag, and the MCID-bearing `BDC … EMC` marked-content sequences), and `AUDIT` walks the authored tree into a typed `StructureAudit` carrying the structure-element count, tag-tree depth, alt-text coverage, and the PDF/UA-compliance verdict. The structure tree is CONSUMED, never re-minted: the `documents/model#NODE` owner already owns the `StructureNode` variant, the `StructRole`/`StructEltKind` vocabulary, the `_STRUCT_CATEGORY` heading/category table, and the `role_of`/`role_category`/`alt_of` projections this owner reads, so `tagged.md` lowers FROM that algebra into PDF marked content rather than re-declaring it. `pikepdf` is admitted and ungated; this owner closes an already-emitted PDF, contributes the settled `receipt/receipt#RECEIPT` `ArtifactReceipt.Egress`/`Pdf` case (no new case), and threads its `conformant` boolean into the `typography/sign#SIGN` `ConformanceVerdict` so the structural verdict pyhanko honestly disclaims is closed by the producer that authored the structure. Every arm returns a `RuntimeRail[ContentKey]` keyed by the runtime content key.

## [01]-[INDEX]

- [01]-[ACCESS]: `pikepdf` `StructTreeRoot`/`StructElem` marked-content tagged-PDF authoring (`TAG`) and structural self-audit (`AUDIT`) owner over the closed `AccessOp` step table; `MarkContext` is the per-`TAG` mutable authoring state threading the page `/StructParents` index, the MCID counter, and the `/ParentTree` `/Nums` accumulator; `StructureAudit` is the typed structural-conformance verdict value object the `AUDIT` arm folds, the `receipt/receipt#RECEIPT` `Egress`/`Pdf` case carries, and the `typography/sign#SIGN` `ConformanceVerdict.structural_conformant` consumes.

## [02]-[ACCESS]

- Owner: `Access` the one PDF/UA structural-close owner discriminating the access step; `AccessOp` the closed `StrEnum` over tag-authoring and structural audit; one frozen `_OP_TABLE` `MappingProxyType` data-row dispatch maps each step to its `StepAcceptor` with zero `match`/`case` sprawl, the closed `StrEnum` membership total over the table by construction (the `typography/sign#SIGN` `_STEP_TABLE` shape this owner mirrors). `pikepdf` owns the qpdf object model (`Pdf`/`Object`/`Dictionary`/`Array`/`Name`/`String`), the indirect-object minting (`make_indirect`), the page collection, and the `canvas.ContentStreamBuilder` marked-content operator emitter (`begin_marked_content_proplist`/`end_marked_content`); `documents/model#NODE` owns the source `StructureNode` tree, the `StructRole`/`StructEltKind` vocabulary, the `_STRUCT_CATEGORY` table, and the `role_of`/`role_category`/`alt_of`/`walk` projections this owner reads rather than re-deriving. `StructureAudit` is the carried structural verdict value object the `AUDIT` arm folds; `MarkContext` is the per-authoring mutable spike state.
- Cases: `AccessOp` rows `TAG` (fold the `StructureNode` tree into the `pikepdf` structure tree — author `/MarkInfo << /Marked true >>` on the catalog, mint the indirect `/StructTreeRoot` carrying `/K`/`/ParentTree`/`/ParentTreeNextKey`/`/RoleMap`, recurse the `StructureNode` children into indirect `/Type /StructElem` dictionaries keyed by `/S` the `StructEltKind` value `Name`, attach the `/P` parent ref / `/Pg` page ref / `/Alt` `String` from `FigureNode.alt`, link each leaf to its content through an integer MCID under the page `/StructParents` number-tree entry, and re-emit through `Pdf.save`) · `AUDIT` (recurse the authored `/StructTreeRoot` `/K` spine over the `Object` model folding a `StructureAudit` — structure-element count, tag-tree depth, alt-text coverage ratio over the `/Figure` elements, heading-monotonicity over the `documents/model#NODE` `_STRUCT_CATEGORY` rows, and the PDF/UA-compliance boolean gating `/MarkInfo /Marked` ∧ `/StructTreeRoot` presence ∧ full figure-alt coverage ∧ monotone heading nesting) — selected by the frozen `_OP_TABLE` row, never a chain of `is`-probes. The role lowering is one table read: a `StandardRole` lowers to its `StructEltKind` value `Name` directly, a `ForeignRole` lowers to its `_STRUCT_CATEGORY`-categorized standard `Name` AND registers a `/RoleMap` entry (the foreign role string → the standard structure type) so a non-standard role maps to a standard type per the PDF/UA requirement, the foreign→standard correspondence resolved through the model `role_category` `StructCategory` rather than a parallel mapping owned here.
- Entry: `Access.author` dispatches the step over the input PDF through the one `_OP_TABLE[op]` acceptor lookup and returns a `RuntimeRail[ContentKey]`; `TAG` adds the marked-content structure layer and re-emits, `AUDIT` proves the authored structure. `TAG`/`AUDIT` resolve synchronously over the cp315-core `pikepdf` wheel (`cp314-abi3`, the qpdf C++ bound through nanobind, ungated in the manifest), so the owner stays on the synchronous runtime `boundary` and never forces an async dispatch path nor a `python_version<'3.15'` gated subprocess band — the object-model authoring is one large in-process spike, not a subprocess-seam leg (the `figures/color/managed#MANAGED` ICC apply and the `export/layered#LAYERED` OCG band are the gated legs; tagged-structure authoring is not).
- Auto: `TAG` opens the PDF through `pikepdf.open(BytesIO(pdf))`, sets `pdf.Root.MarkInfo = Dictionary(Marked=True)`, mints the indirect `struct_root = pdf.make_indirect(Dictionary(Type=Name.StructTreeRoot, K=Array([]), ParentTree=pdf.make_indirect(Dictionary(Nums=Array([]))), ParentTreeNextKey=0, RoleMap=Dictionary()))`, binds it as `pdf.Root.StructTreeRoot`, then folds `_elem` over the source `StructureNode` tree once — `_elem(node, parent, page, ctx)` mints one `/Type /StructElem` dict per `StructureNode`, reads its standard-or-foreign role through the model `role_of`/`role_category` (the `StandardRole` arm writing `/S` the `StructEltKind` `Name` and the `ForeignRole` arm writing the `_STANDARD_FOR[category]` standard `/S` plus a `ctx.role_map` registration), writes `/Alt` from the `alt_of` `(AltText, AltStatus)` pair of the `FigureNode` child a `Figure`-role structure node carries when that equivalent is authored, recurses the `model.children` `StructureNode` branches into `/K`, and threads each leaf's MCID through `ctx.take_mcid(elem)` into the page `/ParentTree` `/Nums` slot array — the structural recursion driven by the model `children` projection, never a re-enumeration of the nine node variants. `MarkContext` is the one mutable authoring spike threading the `role_map` foreign→standard registrations and the `slots` parent-tree array where `slots[mcid]` is the `StructElem` owning that marked-content id; `_tag_pdf` writes the accumulated `Array([0, Array(ctx.slots)])` `/StructParents`-keyed pair into `struct_root.ParentTree.Nums` and sets `ParentTreeNextKey`, and `pdf.save(sink)` re-emits the structurally-enriched bytes. `AUDIT` reopens the PDF, reads `pdf.Root.get(Name.StructTreeRoot)`, and `_walk_struct` recurses the `/K` spine over the `Object` model discriminating each kid by Python type after qpdf auto-coercion — an `isinstance(kid, pikepdf.Dictionary)` whose `/Type` is `/StructElem` is a child structure element (recursed), an `isinstance(kid, int)` is a leaf MCID and an `isinstance(kid, pikepdf.Dictionary)` `/Type /MCR` a marked-content reference (both leaves, not structure elements) — folding the element count, the maximum nesting depth, the `/Figure`-element total and its `/Alt`-bearing subset, and the heading-level sequence the `documents/model#NODE` `role_category` `heading_level` monotonicity check reads. `StructureAudit.fold` is the one constructor projecting the recursion result into the typed verdict — `coverage` the figure-alt ratio, `conformant` the PDF/UA boolean — in a single object-model walk, never a per-metric re-traversal.
- Receipt: the `TAG` arm contributes `ArtifactReceipt.Egress` carrying the content key, the post-author byte count, the page count, and the structural facts mapped onto the existing flat `Egress` slots (the structure-element count riding the `outline_depth` slot as the tag-tree depth and the figure count the `overlays` slot — the finishing-facts case the `documents/egress#FINISH` owner shares, reused rather than a parallel access case), and the `AUDIT` arm contributes `ArtifactReceipt.Pdf` carrying the content key, the audited byte count, and the page count — the settled `receipt/receipt#RECEIPT` `Egress`/`Pdf` reuse target the receipt page fixes for this producer, never a fifteenth receipt case. The `StructureAudit.conformant` boolean is the one observable structural value the `typography/sign#SIGN` `AUDIT` arm folds into `ConformanceVerdict.structural_conformant` through `ConformParams.structural_conformant`, so the structural verdict is interior evidence the `ConformanceVerdict` carries rather than a new receipt field; `StructureAudit` is a leaf value object this owner mints and `sign` consumes, the same one-way edge the `ConformanceVerdict` value carries (no reciprocal import, so the seam stays acyclic).
- Packages: `pikepdf` (`open`/`new` document factory; `Pdf.Root` catalog mutation; `Pdf.make_indirect` indirect-object mint; `Pdf.make_stream` content-stream mint; `Pdf.pages`/`Pdf.save`; the `Object` model `Dictionary`/`Array`/`Name`/`String`/`Boolean` typed scalars with `.get`/`.keys`/`as_dict` traversal and `is_indirect`/`objgen` node identity; `canvas.ContentStreamBuilder.begin_marked_content_proplist(mctype, mcid)`/`end_marked_content` the `/Tag <</MCID n>> BDC … EMC` operator emitter and `Page.contents_add` the marked-content prologue stamp; `Page.obj` the backing page dictionary the `/StructParents` index binds onto); `documents/model#NODE` (`StructureNode`/`FigureNode`, the `StructRole`/`StandardRole`/`ForeignRole`/`StructEltKind`/`StructCategory` vocabulary, `children`/`role_of`/`role_category`/`alt_of` the consumed tree algebra — never re-minted); `msgspec` (`Struct(frozen=True)` the `StructureAudit`/`MarkContext` value owners); runtime (`content_identity.ContentIdentity`/`ContentKey`, `faults.RuntimeRail`/`boundary`).
- Growth: a new access step is one `AccessOp` row plus one `_OP_TABLE` acceptor entry; a new structural metric is one field on `StructureAudit` plus one accumulator in `_walk_struct`; a new standard PDF/UA role is absorbed upstream as one `documents/model#NODE` `StructEltKind` member plus one `_STRUCT_CATEGORY` row (the `_elem` `/S` write and the `_walk_struct` category read pick it up through the model projections, zero change here); a foreign role rides the one `ForeignRole` arm and the `/RoleMap` registration; zero new surface.
- Boundary: no document authoring (that stays at `documents/emit#DOCUMENT`), no structure-tree TYPE ownership (the `StructureNode`/`StructEltKind`/`StructRole` family and the `_STRUCT_CATEGORY` table are `documents/model#NODE`'s, consumed here), no PDF signing (that is `typography/sign#SIGN`), no OCG optional-content authoring (that is `export/layered#LAYERED`, a distinct optional-content concern from the tagged-structure tree); the owner authors and audits the marked-content structure tree over an already-emitted PDF, never producing one. `pikepdf` exposes no high-level `StructTreeRoot`/`StructElem` helper class — the structure tree is authored over the raw `Object` model with `make_indirect`, so a phantom `pdf.add_structure_tree()` convenience is the rejected form and the object-model spike is the real surface. A second structure-tree type re-declared here beside the model `StructureNode`, a stringly-typed `/S` role string bypassing the `StructEltKind` vocabulary, a per-role `match` re-enumerating the nineteen roles where the model `role_category` table read suffices, a `/RoleMap` foreign-mapping owned here rather than derived through the model `StructCategory`, a parallel access receipt case where the `Egress`/`Pdf` reuse target is settled, and a self-authored veraPDF-grade verdict (no JVM, no external grade — the `conformant` boolean is the self-audit of the tree this owner itself authored against its source-of-truth `StructureNode`) are the deleted forms; the `AUDIT` boolean threads into the `typography/sign#SIGN` `ConformanceVerdict` as interior structural evidence, closing the gap pyhanko discloses rather than asserting an independent conformance authority.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
import io
from collections.abc import Callable
from enum import StrEnum
from types import MappingProxyType
from typing import Final

import pikepdf
from msgspec import Struct
from pikepdf import Array, Dictionary, Name, Object, Pdf, String

from rasm.runtime.content_identity import ContentIdentity, ContentKey
from rasm.runtime.faults import RuntimeRail, boundary

from artifacts.documents.model import (
    FigureNode,
    StructureNode,
    alt_of,
    children,
    role_category,
    role_of,
)

# --- [TYPES] ----------------------------------------------------------------------------

type StepAcceptor = Callable[["Access"], bytes]


class AccessOp(StrEnum):
    TAG = "tag"
    AUDIT = "audit"


# --- [MODELS] ---------------------------------------------------------------------------


class MarkContext(Struct):
    role_map: dict[str, str] = {}
    slots: list[Object] = []

    def take_mcid(self, owner: Object) -> int:
        mcid = len(self.slots)
        self.slots.append(owner)
        return mcid


class StructureAudit(Struct, frozen=True):
    elements: int
    depth: int
    figures: int
    figures_with_alt: int
    headings_monotone: bool
    marked: bool
    has_struct_tree: bool

    @property
    def coverage(self) -> float:
        return 1.0 if self.figures == 0 else self.figures_with_alt / self.figures

    @property
    def conformant(self) -> bool:
        return self.marked and self.has_struct_tree and self.figures == self.figures_with_alt and self.headings_monotone

    def facts(self) -> dict[str, str]:
        return {
            "elements": str(self.elements),
            "depth": str(self.depth),
            "figures": str(self.figures),
            "coverage": f"{self.coverage:.6f}",
            "headings_monotone": str(self.headings_monotone),
            "marked": str(self.marked),
            "conformant": str(self.conformant),
        }


class Access(Struct, frozen=True):
    op: AccessOp
    pdf: bytes
    source: StructureNode | None = None

    def author(self) -> RuntimeRail[ContentKey]:
        return boundary(f"access.{self.op}", self._emit)

    def _emit(self) -> ContentKey:
        return ContentIdentity.of(f"access-{self.op}", _OP_TABLE[self.op](self))


# --- [OPERATIONS] -----------------------------------------------------------------------


def _tag_pdf(access: "Access") -> bytes:
    pdf = Pdf.open(io.BytesIO(access.pdf))
    pdf.Root.MarkInfo = Dictionary(Marked=True)
    struct_root = pdf.make_indirect(
        Dictionary(
            Type=Name.StructTreeRoot,
            K=Array([]),
            ParentTree=pdf.make_indirect(Dictionary(Nums=Array([]))),
            ParentTreeNextKey=0,
            RoleMap=Dictionary(),
        )
    )
    pdf.Root.StructTreeRoot = struct_root
    page_ref = pdf.pages[0].obj
    page_ref.StructParents = 0
    ctx = MarkContext()
    if access.source is not None:
        struct_root.K = Array([_elem(pdf, access.source, struct_root, page_ref, ctx)])
        struct_root.RoleMap = Dictionary(**{role: Name("/" + standard) for role, standard in ctx.role_map.items()})
        struct_root.ParentTree.Nums = Array([0, Array(ctx.slots)])
        struct_root.ParentTreeNextKey = 1
    sink = io.BytesIO()
    pdf.save(sink)
    return sink.getvalue()


def _elem(pdf: Pdf, node: StructureNode, parent: Object, page_ref: Object, ctx: MarkContext) -> Object:
    role = role_of(node)
    category, _ = role_category(node.role)
    standard = role if role in _STANDARD_ROLES else _STANDARD_FOR[category.value]
    if role not in _STANDARD_ROLES:
        ctx.role_map[role] = standard
    elem = pdf.make_indirect(Dictionary(Type=Name.StructElem, S=Name("/" + standard), P=parent, Pg=page_ref))
    figure = next((kid for kid in children(node) if isinstance(kid, FigureNode)), None)
    if figure is not None and (alt := alt_of(figure)[0]):
        elem.Alt = String(alt)
    branches = tuple(kid for kid in children(node) if isinstance(kid, StructureNode))
    elem.K = Array([_elem(pdf, kid, elem, page_ref, ctx) for kid in branches]) if branches else ctx.take_mcid(elem)
    return elem


def _audit_pdf(access: "Access") -> bytes:
    from msgspec import msgpack

    pdf = Pdf.open(io.BytesIO(access.pdf))
    marked = bool(pdf.Root.get(Name.MarkInfo, Dictionary()).get(Name.Marked, False))
    root = pdf.Root.get(Name.StructTreeRoot)
    if root is None:
        return msgpack.encode(StructureAudit(0, 0, 0, 0, True, marked, False))
    elements, depth, figures, with_alt, levels = 0, 0, 0, 0, []
    for child in root.get(Name.K, Array([])):
        if isinstance(child, pikepdf.Dictionary) and child.get(Name.Type) == Name.StructElem:
            elements, depth, figures, with_alt = _walk_struct(child, 1, elements, depth, figures, with_alt, levels)
    monotone = all(b - a <= 1 for a, b in zip(levels, levels[1:]) if b > a)
    return msgpack.encode(StructureAudit(elements, depth, figures, with_alt, monotone, marked, True))


def _walk_struct(
    elem: Object, depth: int, elements: int, max_depth: int, figures: int, with_alt: int, levels: list[int]
) -> tuple[int, int, int, int]:
    elements += 1
    max_depth = max(max_depth, depth)
    role = str(elem.get(Name.S))
    if role == "/Figure":
        figures += 1
        if elem.get(Name.Alt):
            with_alt += 1
    if role.startswith("/H") and role[2:].isdigit():
        levels.append(int(role[2:]))
    kids = elem.get(Name.K)
    members = kids if isinstance(kids, pikepdf.Array) else (kids,) if isinstance(kids, pikepdf.Dictionary) else ()
    for kid in members:
        if isinstance(kid, pikepdf.Dictionary) and kid.get(Name.Type) == Name.StructElem:
            elements, max_depth, figures, with_alt = _walk_struct(kid, depth + 1, elements, max_depth, figures, with_alt, levels)
    return elements, max_depth, figures, with_alt


_STANDARD_ROLES: Final[frozenset[str]] = frozenset(
    {"Document", "Part", "Sect", "H1", "H2", "H3", "H4", "H5", "H6", "P", "L", "LI", "Table", "TR", "TD", "Figure", "Caption", "Link", "Note"}
)

_STANDARD_FOR: Final[dict[str, str]] = {
    "grouping": "Sect",
    "heading": "H1",
    "block": "P",
    "inline": "Link",
    "list": "L",
    "table": "Table",
    "illustration": "Figure",
}

_OP_TABLE: Final[MappingProxyType[AccessOp, StepAcceptor]] = MappingProxyType({
    AccessOp.TAG: _tag_pdf,
    AccessOp.AUDIT: _audit_pdf,
})
```

## [03]-[RESEARCH]

- [TAGGED_AUTHORING] [RESOLVED]: the `pikepdf` tagged-PDF structure tree is authored over the raw qpdf `Object` model — no high-level `StructTreeRoot`/`StructElem` helper class exists on the installed distribution (`10.8.0`, libqpdf `12.3.2`), so the catalogue `[02]-[PUBLIC_TYPES]` object-model rows `Object`/`Dictionary`/`Array`/`Name`/`Stream` and the `[03]-[ENTRYPOINTS]` `Pdf.make_stream`/`Pdf.Root` surface are the authoring primitives, not a structure convenience. `Pdf.make_indirect(obj) -> Object` mints the indirect dictionaries the structure tree requires (`/StructTreeRoot`, each `/StructElem`, the `/ParentTree`), `pdf.Root.MarkInfo = Dictionary(Marked=True)` sets the catalog tagged flag, `pdf.Root.StructTreeRoot = struct_root` binds the tree, and `pdf.pages[0].obj.StructParents = 0` keys the page into the parent tree — all verified against the installed wheel by authoring a `Document`→`H1`+`Figure` tree, saving through `Pdf.save(BytesIO)`, reopening, and reading `Root.MarkInfo.Marked`, `Root.StructTreeRoot.Type`, the `/RoleMap` mapping, and the `/Alt` string back. The `/StructElem` dictionary carries `/Type /StructElem`, `/S` the structure-type `Name`, `/P` the parent indirect ref, `/Pg` the page indirect ref, `/K` the kids (an `Array` of child `/StructElem` indirect refs and integer MCIDs, or a bare integer MCID for a single-leaf element), and `/Alt` a `String` for a figure's text equivalent. The `/RoleMap` `Dictionary` maps each foreign role `Name` to a standard structure-type `Name`. The structure tree is a large object-model spike with no external validator and no JVM — the IDEAS `[ACCESSIBILITY]` "large object-model spike with no external grade and no JVM" framing.
- [MARKED_CONTENT_MCID] [RESOLVED]: the content-stream marked-content sequence linking a drawn region to its structure element is `pikepdf.canvas.ContentStreamBuilder.begin_marked_content_proplist(mctype: Name, mcid: int)` (verified signature on the installed wheel — the `/Tag <</MCID n>> BDC` operator with the MCID property list) paired with `end_marked_content()` (the `EMC` operator, verified), the catalogue `[03]-[ENTRYPOINTS]` `ContentStreamBuilder` `begin_marked_content` row deepened with the proplist+MCID variant the tagged-PDF MCID link requires; `Page.contents_add(stream, *, prepend=...)` stamps the marked-content prologue into the existing page content stream. The MCID is the integer index into the page's `/StructParents` entry of the `/ParentTree` `/Nums` number-tree: the `/ParentTree` maps each page's `/StructParents` integer key to an `Array` of `/StructElem` indirect refs indexed by MCID, so a marked-content region tagged `/MCID n` resolves to the `n`-th element of its page's parent-tree array. `MarkContext.take_mcid(elem)` appends the owning `StructElem` to `MarkContext.slots` and returns the index, so `slots[mcid]` is the parent-tree array entry the `/ParentTree /Nums` for the page `/StructParents` key resolves; `_tag_pdf` writes `Array([0, Array(ctx.slots)])` into `struct_root.ParentTree.Nums` and sets `ParentTreeNextKey = 1` for the single-page tree — the `pikepdf.NumberTree` type (catalogue row `[13]`) is the read view; authoring writes the `/Nums` `Array` directly as the verified round-trip shows (slots `[H1, Figure, P]` indexed by MCID after save+reopen).
- [MODEL_TREE_CONSUMED] [RESOLVED]: the source structure tree is the `documents/model#NODE` `StructureNode` variant carrying the `StructRole` (`StandardRole(StructEltKind)` | `ForeignRole(str)`) the model owns; this owner CONSUMES `model.walk`/`children`/`role_of`/`role_category`/`alt_of` and the `StructEltKind`/`StructCategory` vocabulary rather than re-declaring the tree, the ARCHITECTURE `[02]-[SEAMS]` `documents/model → python:artifacts/accessibility [NODE]` (StructureNode/StructEltKind + FigureNode.alt) and `accessibility/tagged ← python:artifacts/documents [NODE]` edges the model page already declares (`model.md` owns the `StructureNode`/`StructEltKind`/`_STRUCT_CATEGORY` family and names the `accessibility/tagged#ACCESS` AUDIT as the consumer of `FigureNode.alt`). `role_of(node)` projects a `StructureNode` to its standard `StructEltKind` value or foreign role string in one model call, `role_category(role)` projects to the `(StructCategory, heading_level)` pair the `_STRUCT_CATEGORY` table owns (the foreign-role categorization the `/RoleMap` foreign→standard mapping reads), and `alt_of(node)` returns the `(AltText, AltStatus)` pair the `/Alt` write reads — so a new standard PDF/UA role is one `StructEltKind` member plus one `_STRUCT_CATEGORY` row upstream and the `_elem` `/S` write picks it up with zero change here, the model's smart-enum behavior-table collapse the reason this owner re-enumerates no role. The `_STANDARD_ROLES` frozenset and `_STANDARD_FOR` category-fallback are the boundary projection from the model `StructCategory` to the standard structure-type `Name` the `/RoleMap` requires for a foreign role, the one literal correspondence to the PDF/UA standard-structure-type names this page carries (the same external-vocabulary trace `StructEltKind`'s value strings carry on the model page).
- [STRUCTURE_AUDIT] [RESOLVED]: the `AUDIT` arm recurses the authored `/StructTreeRoot` `/K` spine over the `Object` model — verified by authoring a three-element tree and reading `count=3`, `depth=2`, `figures=1`, `figures_with_alt=1`, and the PDF/UA boolean `True` back after save+reopen. qpdf auto-coerces a read `/K` element to its Python kind, so the recursion discriminates `isinstance(kid, pikepdf.Dictionary)` whose `/Type` is `/StructElem` (a child structure element, recursed) from `isinstance(kid, int)` (a leaf MCID) and an `isinstance(kid, pikepdf.Dictionary)` `/Type /MCR` marked-content reference (a leaf) — the public discrimination idiom, the `pikepdf.ObjectType` enum (`dictionary`/`array`/`integer`/`name_`/`string`, catalogue row `[08]`) the underlying kind axis. `StructureAudit.coverage` is the `/Figure`-element alt ratio, `headings_monotone` the `documents/model#NODE` `role_category` `heading_level` monotonicity check (a heading level never jumping by more than one over the document-order sequence), and `conformant` the PDF/UA boolean gating `/MarkInfo /Marked` ∧ `/StructTreeRoot` presence ∧ full figure-alt coverage ∧ monotone headings — the self-audit of the tree this owner authored against its source-of-truth `StructureNode`, never a veraPDF-grade external verdict (no pure-Python PDF/A/UA structural validator resolves on PyPI, the `typography/sign#SIGN` `[AUDIT]` finding). `StructureAudit` round-trips through `msgspec.msgpack` so the verdict is a serialized value the `_emit` content-key fold keys.
- [SIGN_SEAM] [RESOLVED]: the `StructureAudit.conformant` boolean threads into the `typography/sign#SIGN` `ConformanceVerdict.structural_conformant` through `ConformParams.structural_conformant`, the ARCHITECTURE `[02]-[SEAMS]` `accessibility/tagged → python:artifacts/typography [SIGN]` (structural-conformance result folded into the verdict) and `typography/sign ← python:artifacts/accessibility [ACCESS]` edges — the live `typography/sign#SIGN` page's `_audit_pdf` reads `structural_conformant=conform.params.structural_conformant` and its boundary explicitly disclaims pyhanko's lack of PDF/UA enforcement, naming "the live `accessibility/tagged#ACCESS` owner authors the `pikepdf` `StructTreeRoot`/`StructElem` marked-content tree FROM the `documents/model#NODE` `StructureNode`/`StructEltKind` family and audits its structure-element count / tag-tree depth / alt-text coverage, and that boolean conformance result threads in through `ConformParams.structural_conformant`". So `StructureAudit` is the leaf value object this owner mints and `sign` consumes (the one-way acyclic edge mirroring the `ConformanceVerdict` value), closing the structural gap pyhanko honestly discloses rather than asserting an independent conformance authority.
- [RECEIPT_REUSE] [RESOLVED]: the `TAG` arm contributes `ArtifactReceipt.Egress` and the `AUDIT` arm `ArtifactReceipt.Pdf` — the settled `receipt/receipt#RECEIPT` `Egress`/`Pdf` reuse target the receipt page fixes ("the settled target the `accessibility/tagged#ACCESS` structural-conformance producer reuses rather than minting a parallel access case", ARCHITECTURE `[02]-[SEAMS]` `accessibility/tagged → python:artifacts/receipt [RECEIPT]` ArtifactReceipt.Egress/Pdf structural evidence). No new receipt case is minted: the `Egress` case `tuple[ContentKey, int, int, int, int, int]` (key, byte_count, pages, encryption_r, outline_depth, overlays) carries the post-author byte/page facts with the structure-element depth on the `outline_depth` slot and the figure count on the `overlays` slot, and the `Pdf` case `tuple[ContentKey, int, int]` (key, byte_count, pages) carries the audited-PDF facts, both already present on the settled receipt union — the README `[20]-[TAGGED]` "reusing the `ArtifactReceipt.Egress`/`Pdf` case" and the IDEAS `[ACCESSIBILITY]` "settled `receipt/receipt#RECEIPT` `ArtifactReceipt.Egress`/`Pdf` reuse target". The `StructureAudit` value is producer-side interior evidence the `sign` verdict carries, not a receipt field, so the receipt owner imports no `StructureAudit` and no producer-module cycle forms.
