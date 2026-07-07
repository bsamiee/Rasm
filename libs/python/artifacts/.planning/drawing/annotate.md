# [PY_ARTIFACTS_DRAWING_ANNOTATE]

The ISO 128-2 annotation producer of the drawing-production plane: `Annotate` is ONE owner over a closed `AnnotateOp` `expression.tagged_union` — `Leader`/`TextNote`/`RevisionCloud` — that lowers every leader, keynote, flag-note, general-note, and revision-cloud mark onto a named SVG annotation layer AND an `ezdxf` DXF layout, the structural twin of `drawing/symbol#SYMBOL` (a dual-lowering owner over a bounded mark vocabulary) and `visualization/diagram/draw#DRAW` (named-layer SVG emission). The three top-level cases are the deepest collapse the domain admits: `Leader` carries the shared ISO 128-2 leader geometry (the pointed `targets`, the `landing` shoulder, the `LeaderPath` straight/spline reference line, and the `SymbolStyle.terminator` line-end) and discriminates its LANDING CONTENT over a closed `LeaderContent` sub-family — `Note` mtext, `Keynote` code-bubble, `Flag` note-flag — so a plain leader, a keynote, and a flag-note are ONE leader owner varying only in what sits at the landing, exactly the `ezdxf` `add_multileader_mtext`-versus-`add_multileader_block` content split; `TextNote` carries a standalone (leader-less) block whose `NoteBody` is either `Prose` (a `typography/layout#LAYOUT` Knuth-Plass `LineBrokenRun` outlined line-by-line through `ziafont`) or `Math` (a `ziamath` mixed text-and-`$math$` engineering note); `RevisionCloud` carries the scalloped revision enclosure and its optional delta tag. A new leader landing kind is one `LeaderContent` case plus one arm; a new note body is one `NoteBody` case; a new mark grammar is one `AnnotateOp` case — never a per-marker `Keynote`/`FlagNote` class family and never an erased attribute `dict`.

`Annotate` composes the drawing plane's ONE mark-style owner rather than re-declaring it: `drawing/symbol#SYMBOL` `SymbolStyle` carries every mark's `fill`/`stroke` palette index into `visualization/chart/spec#CHART`, its ISO 128 `weight` `LineWeight`, its ISO 3098 `text_height` `TextHeight`, its named `layer` `LayerName`, and its `terminator`, all composed from `drawing/regime#REGIME`'s owned vocabulary, and `drawing/regime#REGIME` `Terminator` carries the ISO 129-1/128-2 line-end family (`FILLED_ARROW`/`OPEN_ARROW`/`OBLIQUE_STROKE`/`DOT`/`NONE`) the shared `drawsvg.Marker(orient='auto')` defs auto-orient along each leader — so the annotation plane declares no parallel `AnnotateStyle` and no second terminator family, and a mark's pen, lettering height, layer, and line-end all trace to one owned ISO row. Leader/Keynote/Flag lower onto `ezdxf` `add_multileader_mtext`/`add_multileader_block` (the `MTextEditor`-formatted mtext content or the reusable keynote/flag block with an `add_attdef` code ATTRIB, landing + dogleg + `ConnectionSide` leader lines), the note text outlines to a font-independent `ziafont` `<path>` and the formula note typesets through `ziamath` (never a font-dependent `drawsvg.Text`), the revision-cloud scallop chain folds through `drawsvg.Path` arc bumps (SVG) and an `ezdxf` bulged `add_lwpolyline` (DXF) — the stroked scallop the correct self-contained default, a filled-band variant composing the LANDED `graphic/vector/region#REGION` `outline`/`boolean` (present on that owner) — a keynote/flag column distributes collinear and equally spaced through one `kiwisolver` `Solver`, and the keynote/flag background masks through `ezdxf` `add_wipeout` so the code reads over drawing geometry. Every mark palette-indexes its `SymbolStyle`, the synchronous `ezdxf`/`drawsvg`/`ziafont`/`ziamath`/`kiwisolver` render offloads onto the runtime thread lane off the event loop (the shared-address-space lane the `numpy` palette and `msgspec` receipt owners force), and every provider raise converts at the `async_boundary` seam onto the runtime `BoundaryFault` rail — no decorative page-local fault union the boundary never reads. `Annotate` mints no IFC (that stays `csharp:Rasm.Bim`), computes no sheet placement (that stays `composition/sheet#SHEET`, which consumes the placed layers), re-shapes no text (that stays `typography/shape#SHAPE`, whose `PositionedGlyphRun` this owner consumes), and re-implements no line-break (that stays `typography/layout#LAYOUT`, whose `LineBrokenRun` this owner consumes).

## [01]-[INDEX]

- [01]-[ANNOTATE]: the `Annotate` owner over the closed `AnnotateOp` `expression.tagged_union` (`Leader`/`TextNote`/`RevisionCloud`) with the nested `LeaderContent` (`Note`/`Keynote`/`Flag`) and `NoteBody` (`Prose`/`Math`) sub-families, dual-lowering each mark over the `drawing/symbol#SYMBOL` `SymbolTarget` policy value into a `drawsvg` named-layer `Group` (leader `Path` + auto-oriented `Marker` terminator + `ziafont`-outlined content + `ziamath` formula + `drawsvg.Path` scallops) or an `ezdxf` layout (`add_multileader_mtext`/`add_multileader_block` + `MTextEditor` content + `add_attdef` bubble block + `add_wipeout` mask + `add_mtext` note + bulged `add_lwpolyline` cloud), composing `drawing/symbol#SYMBOL` `SymbolStyle` for the mark style and `drawing/regime#REGIME` `Terminator`/`LineWeight`/`TextHeight`/`LayerName` for the ISO line-end/pen/lettering/layer, `typography/layout#LAYOUT` `LineBrokenRun`/`typography/shape#SHAPE` `PositionedGlyphRun` for the Knuth-Plass general note, and `kiwisolver` `Solver` for the keynote/flag-column distribution (a filled-band revision-cloud variant composing the landed `graphic/vector/region#REGION` `outline`/`boolean`, the stroked scallop the self-contained default) — palette-indexed to `visualization/chart/spec#CHART`, layer-bound for `export/layered#LAYERED`, placed onto `composition/sheet#SHEET`, railed through `expression` `Result`, offloaded via the runtime thread lane, contributing one `core/receipt#RECEIPT` `ArtifactReceipt.Drawing` case and one `core/plan#PLAN` `ArtifactWork` node.

## [02]-[ANNOTATE]

- Owner: `Annotate` the one drawing-annotation owner holding `marks: tuple[AnnotateOp, ...]`, the `graphic/color/derive#DERIVE` `Palette`, and the `SymbolTarget` egress policy value, discriminating operation over the closed `AnnotateOp` `expression.tagged_union` whose three cases each carry their own typed geometry-and-`SymbolStyle` payload — `Leader` the ISO 128-2 reference-line-plus-content mark (its content the `LeaderContent` sub-union), `TextNote` the standalone note block (its body the `NoteBody` sub-union), `RevisionCloud` the scalloped enclosure — never a per-marker `KeynoteMark`/`FlagMark`/`GeneralNote` class family and never a `StrEnum` keyed against an erased `dict[str, object]`. `LeaderContent` (`Note`/`Keynote`/`Flag`) is the closed landing-content sub-family that collapses the leader-geometry sharing: a plain note-leader, a keynote, and a flag-note carry identical `targets`/`landing`/`LeaderPath` and the same `SymbolStyle.terminator`, differing ONLY in the `LeaderContent` at the landing, so the leader geometry is declared once and the content varies — the exact `ezdxf` mtext-versus-block multileader split, never three parallel leader payloads. `NoteBody` (`Prose`/`Math`) is the closed body sub-family: a total-fit-wrapped prose note carries the plain source, the shaped `PositionedGlyphRun` bytes, and its Knuth-Plass `LineBrokenRun`, a formula note carries the `ziamath` source string, each mode carrying ONLY its own fields rather than one permissive bag. `drawing/symbol#SYMBOL` owns the shared drawing-plane `SymbolStyle` (the palette-indexed mark style `annotate` composes, never a parallel `AnnotateStyle`), which itself composes `drawing/regime#REGIME`'s owned `LayerName`/`LineWeight`/`TextHeight`/`Terminator` ISO vocabulary; `drawsvg` owns the named-layer `Group` container, the leader `Path` command builder, the reusable `Marker(orient='auto')` terminator defs, and the `Path.arc`/`Path.A` revision-cloud scallop fold; `ziafont` owns the ISO 3098 text-to-`<path>` outline; `ziamath` owns the mixed text-and-math typeset; `ezdxf` owns the DXF `MultiLeader`/`MText`/`Wipeout`/`LWPolyline` model and the `MTextEditor` formatted-content builder; `typography/layout#LAYOUT`/`typography/shape#SHAPE` own the line-break and shaping; `kiwisolver` owns the keynote/flag-column constraint solve; `graphic/vector/region#REGION` owns the landed `skia-pathops` `outline`/`boolean` a filled-band revision-cloud variant composes (the base stroked scallop is self-contained `drawsvg`/`ezdxf`, the correct default). No sheet-set, dimension, or symbol logic crosses this owner — those are `composition/sheet#SHEET`, `drawing/dimension#DIMENSION`, and `drawing/symbol#SYMBOL`.
- Cases: `AnnotateOp` cases — `Leader(targets, landing, path, content, style)` (the ISO 128-2 leader: one or many `targets` the terminator points at, a `landing` where the horizontal shoulder ends, a `LeaderPath` straight/spline reference line, the `style.terminator` line-end, and a `LeaderContent` at the landing — `Note(text)` an `MTextEditor`-formatted mtext note, `Keynote(code, bubble, sheet_ref, mask)` a code carried in a `BubbleShape` bubble over an optional sheet/legend reference with an optional wipeout mask, `Flag(number, shape, mask)` a note-number in a flag shape — an empty `targets` the leader-less landing bubble) · `TextNote(insert, body, style)` (the standalone note block at `insert`, its `NoteBody` either `Prose(source, run, broken)` — the plain source, its shaped `PositionedGlyphRun` bytes, and its Knuth-Plass `LineBrokenRun` outlined line-by-line through `ziafont` — or `Math(source)` — a mixed text-and-`$math$` engineering note typeset through `ziamath.Text`) · `RevisionCloud(region, radius, mark, style)` (the revision enclosure: a closed `region` polyline whose every edge folds into a chain of convex `radius`-scallop bumps, carrying an optional `mark` revision-number delta tag at the first vertex) — matched by one total `match`/`case` over `tag`, the nested `LeaderContent`/`NoteBody` each matched by their own total `match`, never a per-content special case. `SymbolStyle` (the `fill`/`stroke` `visualization/chart/spec#CHART` index, the ISO 128 `weight` `LineWeight`, the ISO 3098 `text_height` `TextHeight`, the `layer` `LayerName`, the `terminator` `Terminator`) rides as each case's last slot; the `style.terminator` (`FILLED_ARROW`/`OPEN_ARROW`/`OBLIQUE_STROKE`/`DOT`/`NONE`) keys the shared `drawsvg.Marker` def; `LeaderPath` (`STRAIGHT`/`SPLINE`) and `BubbleShape` (`CIRCLE`/`HEXAGON`/`TRIANGLE`/`DIAMOND`/`RECTANGLE`) are the closed leader-and-bubble axes.
- Entry: `Annotate.over(marks, palette, target=SymbolTarget.SVG)` is the one modal-arity entrypoint normalizing `AnnotateOp | Iterable[AnnotateOp]` into the `marks` tuple by a structural `match` at the head (a lone mark the singleton case, a mixed sheet the multi-element case), never a `batch` knob or a per-marker sibling; `render` is `async` over the runtime `async_boundary`, returns `RuntimeRail[ArtifactReceipt]` beside the `layered()` `LayerPlan` projection`, and offloads the whole synchronous fold onto the runtime thread lane (`LanePolicy.offload(..., retry=RetryClass.OCCT)`) — the shared-address-space thread arm (the `ezdxf`/`drawsvg`/`ziafont`/`ziamath`/`kiwisolver`/`pathops` render touches the `numpy` palette and returns the `msgspec`-backed `Layer`/`ArtifactReceipt` owners a `to_interpreter` isolate cannot load, the same lane `drawing/symbol#SYMBOL` and `visualization/diagram/draw#DRAW` take). The `_svg_engine` folds each mark through `_svg_mark` into its `SymbolStyle.layer.compose()` `drawsvg.Group` (the leader `Path` with its auto-oriented `Marker` terminator, the `ziafont`-outlined content, the `ziamath` formula, the scallop `Path`), serializes each named `Group` under one `drawsvg.Drawing` carrying the shared reusable `Terminator` `Marker` defs into a per-layer `Layer(name, source, bbox)` row, and derives the content key over the joined layer bytes; the `_dxf_engine` lowers each mark through `_dxf_mark` (the `add_multileader_mtext`/`add_multileader_block` builder with `MTextEditor` content and `ConnectionSide` leader lines, the `add_attdef` bubble block placed once and referenced, the `add_wipeout` mask, the `add_mtext` note, the bulged `add_lwpolyline` cloud) and writes the DXF as one `Layer("dxf", data, bbox)` row.
- Auto: `SymbolStyle.fill`/`stroke` are integer indices into the `visualization/chart/spec#CHART` palette the `hex_ramp` projection resolves to hex once per render, so a recolor is a palette swap never a per-mark hex literal, exactly as `drawing/symbol#SYMBOL` and `visualization/diagram/glyphset#GLYPHSET` index their marks; `SymbolStyle.layer.compose()` buckets each mark into its named `drawsvg.Group` the `export/layered#LAYERED` owner binds and pens the `ezdxf` `GfxAttribs`, `SymbolStyle.weight` resolves through `_mm`/`_lw` to the SVG drawn width and the DXF 1/100 mm lineweight, and `SymbolStyle.text_height.mm` the ISO 3098 lettering size. The leader geometry is `drawsvg` — one `Path` per pointed target from the terminator end through the elbow to the horizontal `_SHOULDER` landing shoulder, its `marker_start` referencing the shared `Terminator` `Marker(orient='auto')` def keyed by `style.terminator` so the ISO 128-2 line-end auto-orients along the leader, the `LeaderPath.SPLINE` case routing a quadratic through the elbow where `STRAIGHT` routes a polyline. The leader content lowers over `LeaderContent`: `Note` outlines its text to a font-independent `ziafont` `Font.text(...)` run that `_outlined` MEASURES via `getyofst()` and baseline-seats on the landing shoulder (never a `drawsvg.Text` that renders wrong without the font, and never the blind translate that hangs the outline below the landing); `Keynote`/`Flag` draw a `_bubble_outline` `BubbleShape` shape (a `drawsvg.Circle`/`Rectangle`/`Lines` polygon) with an optional white paper-fill backing (the SVG twin of the `add_wipeout` mask) and the code/number `ziafont`-outlined at the centre over the sheet reference. `TextNote.Prose` decodes the `PositionedGlyphRun`, slices its `source` per `LineBrokenRun` line through the run's glyph clusters, and outlines each Knuth-Plass line at its own baseline through `ziafont`; `TextNote.Math` typesets the mixed source through `ziamath.Text(...)` measured via `getyofst()`/`getsize()` and baseline-seated (not a blind translate), both wrapped as a positioned `drawsvg.Raw`. `_text_span` reads the Prose line advances and the Math `getsize()` so `_bbox` extent-checks the note against the canvas, and `_svg_engine` pins `ziafont.config.precision`/`ziamath.config.precision` once inside the offload lane so the emitted `d`-floats — and the content key over them — are deterministic. `RevisionCloud` folds `_scallop_path` — for each closed-region edge a chain of `max(1, round(length / (2·radius)))` convex `radius`-arc bumps appended to one `drawsvg.Path` (SVG) or one bulged `add_lwpolyline` (DXF), a filled scalloped-band variant composing the landed `graphic/vector/region#REGION` `outline`/`boolean` `skia-pathops` surface — and draws the `mark` revision-delta tag at the first vertex. A keynote/flag column threads one `kiwisolver.Solver`: each landing's `y` is a `Variable` sharing the one column `x` (the collinear axis is the constant `x0`, never a constraint), a `required` anchor pins the first landing, a `required` `_MIN_SEP` minimum-separation enforces HARD no-overlap between adjacent landings (the finalized `drawing/dimension#DIMENSION` `_stack` stance, never a soft `medium` band an overlap survives), and a `weak` equal-gap distributes the remainder, `updateVariables()` writing each solved `value()` into the `_route` landing override the engine passes to each mark — the same Cassowary system `drawing/symbol#SYMBOL` reaches for grid-bubble alignment, actually re-keying the landing here rather than leaving the solve unread.
- Growth: a new leader landing content (a datum tag, a section-cut reference) is one `LeaderContent` case plus one `_content_group` arm and one `_dxf_leader` arm — the leader geometry, terminator, and routing are untouched; a new note body (a table-cell note, a callout balloon) is one `NoteBody` case plus one arm; a new mark grammar (a match-line note, a north-annotation) is one `AnnotateOp` case; a new terminator is one `drawing/regime#REGIME` `Terminator` member plus one `_marker` arm; a new bubble is one `BubbleShape` member plus one `_SIDES` row (its polygon derived); a new leader-reference-line kind is one `LeaderPath` member plus one `Path` arm; a new mark visual axis (a background hatch, a text mask) is one `SymbolStyle` field on `drawing/symbol#SYMBOL` threaded into the consuming arm; a new column-distribution rule (radial keynotes, mirrored flags) is one `kiwisolver` constraint at its `strength` band; a filled scalloped revision-cloud band composes the landed `graphic/vector/region#REGION` `outline`/`boolean` `skia-pathops` surface (a composable variant, not a load-bearing dependency for the base, since `drawsvg.Path` already draws the stroked scallop and `ezdxf` the bulged polyline as the correct default); a new receipt fact is one scalar the `ArtifactReceipt.Drawing` case already carries; zero new surface for a new annotation kind or a new layer.
- Boundary: the deleted forms are a per-marker `KeynoteMark`/`FlagNote`/`GeneralNote` class family where one closed `AnnotateOp` union with nested `LeaderContent`/`NoteBody` states them; three parallel leader payloads repeating the `targets`/`landing`/`LeaderPath` fields where one `Leader` case with a `LeaderContent` content discriminant carries the shared geometry once; a permissive `TextNote` bag carrying both a shaped run and a math source where the `NoteBody` sub-union carries only each mode's own fields; a parallel `AnnotateStyle` or a bare-float pen where `drawing/symbol#SYMBOL` `SymbolStyle` composing `drawing/regime#REGIME`'s `LineWeight`/`TextHeight`/`LayerName`/`Terminator` owns the ISO codes; a hand-drawn leader line-and-text pair where `ezdxf` `add_multileader_mtext`/`add_multileader_block` owns the landing/dogleg/content and `drawsvg.Path` + `Marker` owns the SVG leader; a font-dependent `drawsvg.Text` where `ziafont` outlines the note to a font-independent `<path>` and `ziamath` typesets the formula; a per-placement bubble geometry copy where `doc.blocks.new` + `add_blockref` place one keynote/flag block definition; a hand-emitted revision-cloud arc string where `drawsvg.Path` scallop bumps and the `ezdxf` bulged `add_lwpolyline` author it and `graphic/vector/region#REGION` `skia-pathops` owns the filled-band offset; an f-string SVG splice of dynamic note text where the `drawsvg` structured builders and the `ziafont`/`ziamath` `ElementTree` egress escape it; a per-mark color literal where the `SymbolStyle` palette index binds through `visualization/chart/spec#CHART`; a `batch`/`mode` knob where `SymbolTarget` and the modal `over` head discriminate; a `Variable`-keyed dict where the stable mark index keys the `kiwisolver` column solve; a synchronous render on the event loop where the runtime lane offloads it; a re-shape or re-break of the note where `typography/shape#SHAPE` `PositionedGlyphRun` and `typography/layout#LAYOUT` `LineBrokenRun` are consumed; a parallel drawing receipt where the shared `ArtifactReceipt.Drawing` case carries the mark/entity/byte facts. `drawing/symbol#SYMBOL` owns the `SymbolStyle` mark style, `drawing/regime#REGIME` the owned ISO `Terminator`/`LineWeight`/`TextHeight`/`LayerName` vocabulary, `drawsvg` the named-layer SVG container and leader/scallop path builders, `ziafont` text-to-outline, `ziamath` math typeset, `ezdxf` the DXF multileader/mtext/wipeout/lwpolyline model, `graphic/vector/region#REGION` the boolean/offset, `kiwisolver` the constraint solve, `typography/layout#LAYOUT`/`typography/shape#SHAPE` the line-break and shaping, `export/layered#LAYERED` the layer binding, `composition/sheet#SHEET` the placement, and `csharp:Rasm.Bim` the IFC semantics; identity minting is the runtime's.
- Packages: `drawsvg` (`Drawing`/`Group`/`Path`/`Path.A`/`Path.arc`/`Circle`/`Rectangle`/`Lines`/`Line`/`Marker`/`Raw`/`append_def`/`as_svg` the named-layer container, the leader/scallop path builder, and the reusable auto-oriented terminator defs); `ziafont` (`Font`/`Font.text`/`Text.svg`/`Text.getyofst`/`Text.getsize`/`config.precision` the ISO 3098 text-to-`<path>` outline, MEASURED, baseline-seated, content-key-deterministic); `ziamath` (`Text`/`Text.svg`/`Text.getyofst`/`Text.getsize`/`config.precision` the mixed text-and-math typeset for engineering notes, measured and seated); `ezdxf` (`new`/`add_multileader_mtext`/`add_multileader_block`/`MultiLeaderMTextBuilder.set_content`/`add_leader_line`/`build`/`MultiLeaderBlockBuilder.set_content`/`set_attribute`/`render.mleader.ConnectionSide`/`tools.text.MTextEditor.font`/`.height`/`.append`/`.bullet_list`/`.underline`/`add_mtext`/`MText.set_location`/`enums.MTextEntityAlignment`/`add_wipeout`/`add_lwpolyline`/`doc.blocks.new`/`add_attdef`/`bbox.extents`/`math.Vec2`/`gfxattribs.GfxAttribs` the DXF annotation model + true rendered extents); `kiwisolver` (`Solver`/`Variable`/`strength` the keynote/flag-column required-anchor + hard-no-overlap + weak-distribution solve); `beartype` (`@beartype` the `over` construction contract, matching the finalized `drawing/dimension#DIMENSION`); `typography/layout#LAYOUT` (`LineBrokenRun` the Knuth-Plass line break the prose note consumes); `typography/shape#SHAPE` (`PositionedGlyphRun` the shaped run the prose note decodes for its glyph clusters); `drawing/regime#REGIME` (`Terminator`/`LineWeight`/`TextHeight`/`LayerName` the owned ISO line-end/pen/lettering/layer vocabulary `SymbolStyle` composes); `expression` (`tagged_union`/`tag`/`case` the vocabulary); `msgspec` (`Struct`/`msgpack.decode` the value objects and run decode); `builtins.frozendict` (the `_ENGINES`/`_SIDES` tables); runtime (`identity.ContentIdentity`, `faults.RuntimeRail`/`async_boundary`); `core/receipt#RECEIPT` (`ArtifactReceipt.Drawing`); `export/layered#LAYERED` (`Layer`); `graphic/color/derive#DERIVE` (`Palette`/`hex_ramp`).

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
import io
import math
import os
from collections.abc import Callable, Iterable
from enum import StrEnum
from functools import lru_cache
from itertools import pairwise
from typing import Literal, Self, assert_never

import msgspec
from beartype import beartype
from expression import Some, case, tag, tagged_union
from expression.collections import Block
from msgspec import Struct

from rasm.runtime.identity import CANONICAL_POLICY, ContentIdentity, ContentKey
from rasm.runtime.lanes import LanePolicy, Modality
from rasm.runtime.resilience import RetryClass
from rasm.runtime.faults import RuntimeRail, async_boundary

from artifacts.core.plan import Admission, ArtifactWork
from artifacts.core.receipt import ArtifactReceipt
from artifacts.drawing.regime import LineWeight, Terminator
from artifacts.drawing.symbol import SymbolStyle, SymbolTarget
from artifacts.graphic.layer import LayerContent, LayerIntent, LayerNode, LayerPlan, NamingSchema
from artifacts.typography.layout import LineBrokenRun
from artifacts.typography.shape import PositionedGlyphRun
from rasm.artifacts.graphic.color.derive import Palette, hex_ramp

# each proxy reifies on first render-arm use in the offloaded worker
lazy import drawsvg
lazy import ezdxf
lazy import kiwisolver
lazy import ziafont
lazy import ziamath
lazy from ezdxf import bbox
lazy from ezdxf.enums import MTextEntityAlignment
lazy from ezdxf.gfxattribs import GfxAttribs
lazy from ezdxf.math import Vec2
lazy from ezdxf.render.mleader import ConnectionSide
lazy from ezdxf.tools.text import MTextEditor

# --- [TYPES] ----------------------------------------------------------------------------
type Point = tuple[float, float]
type Box = tuple[float, float, float, float]
type AnnotateTag = Literal["leader", "textnote", "revcloud"]

_SHOULDER: float = 2.0  # landing-shoulder length, in text-height multiples (ISO 128-2 leader landing)
_BUBBLE: float = 1.4  # keynote/flag bubble radius, in text-height multiples
_MIN_SEP: float = 8.0  # keynote/flag-column minimum vertical separation (drawing units)
_LEADER_STYLE: str = "ISO-128-2"  # the annotation convention the `ArtifactReceipt.Drawing` style slot carries
_PRECISION: int = 3  # ziafont/ziamath emitted-d-float places — the content-key determinism lever set once per offloaded arm


class LeaderPath(StrEnum):  # ISO 128-2 leader reference-line geometry (ezdxf `render.mleader.LeaderType` twin)
    STRAIGHT = "straight"  # straight polyline — the drawing-office default
    SPLINE = "spline"  # smoothed spline leader through the elbow


class BubbleShape(StrEnum):  # the keynote/flag landing-mark outline
    CIRCLE = "circle"  # keynote circle bubble (the `drawing/symbol#SYMBOL` Detail twin)
    HEXAGON = "hexagon"  # keynote hexagon
    TRIANGLE = "triangle"  # flag-note delta
    DIAMOND = "diamond"  # flag-note diamond
    RECTANGLE = "rectangle"  # boxed keynote


# --- [VOCABULARY] -----------------------------------------------------------------------
@tagged_union(frozen=True)
class LeaderContent:
    # the closed leader-landing content sub-family — the `ezdxf` mtext-versus-block multileader split as data:
    # `note` -> `add_multileader_mtext`, `keynote`/`flag` -> `add_multileader_block` with an `add_attdef` label.
    tag: Literal["note", "keynote", "flag"] = tag()
    note: str = case()  # MTextEditor-formatted mtext content
    keynote: tuple[str, BubbleShape, str, bool] = case()  # (code, bubble, sheet_ref, mask)
    flag: tuple[str, BubbleShape, bool] = case()  # (number, shape, mask)

    @staticmethod
    def Note(text: str) -> "LeaderContent":
        return LeaderContent(note=text)

    @staticmethod
    def Keynote(code: str, bubble: BubbleShape = BubbleShape.HEXAGON, sheet_ref: str = "", *, mask: bool = True) -> "LeaderContent":
        return LeaderContent(keynote=(code, bubble, sheet_ref, mask))

    @staticmethod
    def Flag(number: str, shape: BubbleShape = BubbleShape.TRIANGLE, *, mask: bool = True) -> "LeaderContent":
        return LeaderContent(flag=(number, shape, mask))


@tagged_union(frozen=True)
class NoteBody:
    # the closed note-body sub-family — each mode carries ONLY its own fields, never one permissive bag:
    # `prose` the Knuth-Plass total-fit note (source + shaped run + line break), `math` the ziamath source.
    tag: Literal["prose", "math"] = tag()
    prose: tuple[str, bytes, LineBrokenRun] = case()  # (source, PositionedGlyphRun bytes, Knuth-Plass break)
    math: str = case()  # mixed text + $math$ ziamath source

    @staticmethod
    def Prose(source: str, run: bytes, broken: LineBrokenRun) -> "NoteBody":
        return NoteBody(prose=(source, run, broken))

    @staticmethod
    def Math(source: str) -> "NoteBody":
        return NoteBody(math=source)


@tagged_union(frozen=True)
class AnnotateOp:
    tag: AnnotateTag = tag()
    leader: tuple[tuple[Point, ...], Point, LeaderPath, LeaderContent, SymbolStyle] = case()
    textnote: tuple[Point, NoteBody, SymbolStyle] = case()
    revcloud: tuple[tuple[Point, ...], float, str, SymbolStyle] = case()

    @staticmethod
    def Leader(
        targets: tuple[Point, ...], landing: Point, content: LeaderContent, style: SymbolStyle, *, path: LeaderPath = LeaderPath.STRAIGHT
    ) -> "AnnotateOp":
        return AnnotateOp(leader=(targets, landing, path, content, style))

    @staticmethod
    def TextNote(insert: Point, body: NoteBody, style: SymbolStyle) -> "AnnotateOp":
        return AnnotateOp(textnote=(insert, body, style))

    @staticmethod
    def RevisionCloud(region: tuple[Point, ...], radius: float, style: SymbolStyle, *, mark: str = "") -> "AnnotateOp":
        return AnnotateOp(revcloud=(region, radius, mark, style))


# --- [SERVICES] -------------------------------------------------------------------------
class Annotate(Struct, frozen=True):
    marks: tuple[AnnotateOp, ...]
    palette: Palette
    target: SymbolTarget = SymbolTarget.SVG

    @classmethod
    @beartype
    def over(cls, marks: AnnotateOp | Iterable[AnnotateOp], palette: Palette, /, *, target: SymbolTarget = SymbolTarget.SVG) -> Self:
        match marks:  # the one modal-arity head — a lone mark is the singleton, an iterable the multi-mark sheet
            case AnnotateOp():
                return cls(marks=(marks,), palette=palette, target=target)
            case _:
                return cls(marks=tuple(marks), palette=palette, target=target)

    def emit(self, /) -> ArtifactWork:
        return ArtifactWork(key=self._key, work=self._emit, parents=(), admission=Admission(keyed=None), cost=float(len(self.marks)))

    @property
    def _key(self) -> ContentKey:
        # key-over-INPUT: the canonical frozen spec minted PRE-RUN so keyed admission probes the warm
        # seed BEFORE the native fold runs — never a key over rendered layer bytes.
        return ContentIdentity.of(f"drawing-annotate-{self.target}", (self.marks, self.palette, self.target), policy=CANONICAL_POLICY)

    async def _emit(self) -> RuntimeRail[ArtifactReceipt]:
        # the renamed private render thunk — the terminal receipt threads the PRE-RUN key (receipt.slot == node.key);
        # the layer payload is the layered() LayerPlan projection (V14), never a tuple on the producer rail.
        return (await async_boundary(f"drawing.annotate.{self.target}", self._crossed)).map(lambda pair: pair[1])

    async def layered(self) -> RuntimeRail[LayerPlan]:
        # the V14 projection: the engine fold's rows as ONE semantic LayerPlan tree — substrate DATA the
        # layered/sheet consumers compose as parents, never part of the producer rail.
        return (await async_boundary(f"drawing.annotate.{self.target}", self._crossed)).map(
            lambda pair: LayerPlan(schema=NamingSchema.ISO13567, roots=pair[0])
        )

    async def _crossed(self) -> tuple[tuple[LayerNode, ...], ArtifactReceipt]:
        # synchronous native/CPU fold — crosses the runtime thread lane, never a folder-minted limiter.
        crossed = await LanePolicy.offload(_ENGINES[self.target].arm, self, modality=Modality.THREAD, retry=RetryClass.OCCT)
        return crossed.default_with(lambda fault: _fold_raise(fault))


def _fold_raise(fault: object) -> tuple[tuple[LayerNode, ...], ArtifactReceipt]:
    # terminal collapse at the render boundary: an offload fault reconstructs the raise the node's rail folds.
    raise ValueError(str(fault))


def _row(*, name: str, source: bytes, bbox: tuple[float, float, float, float] | None = None, group: str | None = None) -> LayerNode:
    # lowers one engine row into the graphic/layer vocabulary (V14): the SVG/DXF fragment carries its own
    # extent, a group name nests as the path prefix the LayerPlan fold groups on, z rides row order.
    return LayerNode(name=name if group is None else f"{group}/{name}", intent=LayerIntent.ANNOTATION, content=Some(LayerContent(fragment=source)))


class AnnotateEngine(Struct, frozen=True):
    arm: Callable[[Annotate], tuple[tuple[LayerNode, ...], ArtifactReceipt]]
    engine: str  # the `ArtifactReceipt.Drawing` engine descriptor


# --- [OPERATIONS] -----------------------------------------------------------------------
def _mm(weight: LineWeight, /) -> float:
    return float(weight.value)  # the ISO 128-20 line-weight group (mm); the StrEnum value keys the drawn width


def _lw(weight: LineWeight, /) -> int:
    return round(float(weight.value) * 100)  # the DXF 1/100 mm integer the `GfxAttribs.lineweight` carries


def _style(mark: AnnotateOp, /) -> SymbolStyle:
    match mark:  # every case's style is its last payload slot; one total projection, never a per-tag getattr
        case (
            AnnotateOp(tag="leader", leader=(*_, style))
            | AnnotateOp(tag="textnote", textnote=(*_, style))
            | AnnotateOp(tag="revcloud", revcloud=(*_, style))
        ):
            return style
        case _ as unreachable:
            assert_never(unreachable)


def _points(mark: AnnotateOp, /) -> tuple[Point, ...]:
    match mark:  # the defining points the extent fold unions over
        case AnnotateOp(tag="leader", leader=(targets, landing, *_)):
            return (*targets, landing)
        case AnnotateOp(tag="textnote", textnote=(insert, *_)):
            return (insert,)
        case AnnotateOp(tag="revcloud", revcloud=(region, *_)):
            return region
        case _ as unreachable:
            assert_never(unreachable)


def _text_span(mark: AnnotateOp, /) -> Box | None:
    # the MEASURED extent of a standalone note so the SVG canvas sizes against the real run, never clipping a long
    # note the point-hull misses: a Prose note reads its line advances straight off the Knuth-Plass `LineBrokenRun`
    # (no re-measure — the layout owner already carries them), a Math note reads `ziamath.Text(...).getsize()`.
    match mark:
        case AnnotateOp(tag="textnote", textnote=(insert, NoteBody(tag="prose", prose=(_source, _run, broken)), style)):
            width = max((line.advance for line in broken.lines), default=0.0)
            return (insert[0], insert[1], insert[0] + width, insert[1] + len(broken.lines) * style.text_height.mm * 1.4)
        case AnnotateOp(tag="textnote", textnote=(insert, NoteBody(tag="math", math=source), style)):
            width, height = ziamath.Text(source, size=style.text_height.mm).getsize()
            return (insert[0], insert[1], insert[0] + width, insert[1] + height)
        case _:
            return None


def _bbox(marks: tuple[AnnotateOp, ...], /) -> Box:
    pts = tuple(point for mark in marks for point in _points(mark)) or ((0.0, 0.0),)
    spans = tuple(span for mark in marks if (span := _text_span(mark)) is not None)
    xs = tuple(p[0] for p in pts) + tuple(span[0] for span in spans)
    ys = tuple(p[1] for p in pts) + tuple(span[1] for span in spans)
    hi_x = max((*(x + 1.0 for x in (p[0] for p in pts)), *(span[2] for span in spans)))
    hi_y = max((*(y + 1.0 for y in (p[1] for p in pts)), *(span[3] for span in spans)))
    return (min(xs), min(ys), hi_x, hi_y)


@lru_cache(maxsize=1)
def _drawing_font() -> "ziafont.Font":
    # the ISO 3098 lettering face — the SAME face `typography/shape#SHAPE` shapes the note run with, so the
    # decoded run's glyph clusters index the source the outlining reads; loaded once inside the worker.
    return ziafont.Font(None)


def _route(marks: Block[AnnotateOp], /) -> frozendict[int, Point]:
    # a keynote/flag column threads one `kiwisolver.Solver`: each landing's y is a `Variable` sharing the one
    # column x (the collinear axis is the constant `x0`, no constraint needed), a `required` anchor pins the
    # first landing and a `required` `_MIN_SEP` min-separation keeps adjacent landings from overlapping — the
    # hard no-overlap the finalized `drawing/dimension#DIMENSION` `_stack` takes, never a soft band an overlap
    # survives — and a `weak` equal-gap distributes the remainder; `updateVariables()` writes each solved
    # `value()` into the routed landing override the engines pass.
    column = tuple((index, mark.leader[1]) for index, mark in enumerate(marks) if mark.tag == "leader" and mark.leader[3].tag in ("keynote", "flag"))
    if len(column) < 2:
        return frozendict()
    solver = kiwisolver.Solver()
    ys = tuple(kiwisolver.Variable(f"y{index}") for index, _ in column)
    x0 = sum(landing[0] for _, landing in column) / len(column)
    solver.addConstraint(ys[0] == column[0][1][1])  # required: anchor the first landing at its own y
    for lo, hi in pairwise(ys):  # Exemption: kiwisolver Solver is the stateful native sink; constraints add in place
        solver.addConstraint(hi - lo >= _MIN_SEP)  # required: hard no-overlap between adjacent landings
        solver.addConstraint((hi - lo == ys[1] - ys[0]) | kiwisolver.strength.weak)  # weak: even distribution
    solver.updateVariables()
    return frozendict({index: (x0, var.value()) for (index, _), var in zip(column, ys, strict=True)})


def _marker(term: Terminator, /) -> "drawsvg.Marker":
    # the reusable ISO 129-1/128-2 line-end def the leader `marker_start` auto-orients along (`context-stroke`
    # inherits the leader pen); composes the `drawing/regime#REGIME` `Terminator` vocabulary, no parallel family.
    marker = drawsvg.Marker(-1.0, -1.0, 1.0, 1.0, orient="auto", id=f"term-{term.value}")
    match term:
        case Terminator.FILLED_ARROW:
            marker.append(drawsvg.Lines(-1.0, -1.0, 1.0, 0.0, -1.0, 1.0, close=True, fill="context-stroke"))
        case Terminator.OPEN_ARROW:
            marker.append(drawsvg.Lines(-1.0, -1.0, 1.0, 0.0, -1.0, 1.0, close=False, fill="none", stroke="context-stroke", stroke_width=0.3))
        case Terminator.OBLIQUE_STROKE:
            marker.append(drawsvg.Line(-1.0, 1.0, 1.0, -1.0, stroke="context-stroke", stroke_width=0.3))
        case Terminator.DOT:
            marker.append(drawsvg.Circle(0.0, 0.0, 0.6, fill="context-stroke"))
        case Terminator.NONE:
            pass
        case _ as unreachable:
            assert_never(unreachable)
    return marker


def _polygon(sides: int, radius: float, /) -> tuple[float, ...]:
    # the flat coord run a `drawsvg.Lines` closed polygon takes, derived from the side count and radius
    return tuple(
        coord
        for i in range(sides)
        for coord in (radius * math.cos(math.tau * i / sides - math.pi / 2.0), radius * math.sin(math.tau * i / sides - math.pi / 2.0))
    )


def _bubble_outline(shape: BubbleShape, radius: float, *, fill: str, stroke: str, width: float) -> "drawsvg.DrawingBasicElement":
    match shape:  # the keynote/flag mark outline; a filled `stroke="none"` backing is the SVG wipeout-mask twin
        case BubbleShape.CIRCLE:
            return drawsvg.Circle(0.0, 0.0, radius, fill=fill, stroke=stroke, stroke_width=width)
        case BubbleShape.RECTANGLE:
            return drawsvg.Rectangle(-radius, -radius, 2.0 * radius, 2.0 * radius, fill=fill, stroke=stroke, stroke_width=width)
        case BubbleShape.HEXAGON | BubbleShape.TRIANGLE | BubbleShape.DIAMOND:
            return drawsvg.Lines(*_polygon(_SIDES[shape], radius), close=True, fill=fill, stroke=stroke, stroke_width=width)
        case _ as unreachable:
            assert_never(unreachable)


def _outlined(text: str, at: Point, style: SymbolStyle, ramp: list[str], *, anchor: str = "left") -> "drawsvg.Group":
    # ISO 3098 note text as a font-independent ziafont `<path>` — never a `drawsvg.Text` font-dependent `<text>`;
    # the machine-generated SVG rides a `drawsvg.Raw` (no f-string splice of the dynamic text into markup). The run
    # is MEASURED and baseline-seated: `getyofst()` is the baseline-to-top offset, so the outline's baseline lands
    # ON `at` (the leader shoulder / bubble centre) rather than its bounding-box top — the blind translate the
    # finalized `drawing/dimension#DIMENSION` `_annotation_layer` already avoids; `getsize()` feeds `_text_span`.
    run = _drawing_font().text(text, size=style.text_height.mm, halign=anchor, color=ramp[style.stroke % len(ramp)])
    return drawsvg.Group(drawsvg.Raw(run.svg()), transform=f"translate({at[0]},{at[1] - run.getyofst()})")


def _bubble_group(label: str, shape: BubbleShape, at: Point, style: SymbolStyle, ramp: list[str], sheet_ref: str, *, mask: bool) -> "drawsvg.Group":
    radius = _BUBBLE * style.text_height.mm
    stroke = ramp[style.stroke % len(ramp)]
    group = drawsvg.Group(transform=f"translate({at[0] + radius},{at[1]})")
    if mask:  # the `add_wipeout` twin — a paper backing so the code reads over drawing geometry
        group.append(_bubble_outline(shape, radius, fill="white", stroke="none", width=0.0))
    group.append(_bubble_outline(shape, radius, fill="none", stroke=stroke, width=_mm(style.weight)))
    group.append(_outlined(label, (0.0, 0.0), style, ramp, anchor="center"))
    if sheet_ref:  # the keynote sheet/legend reference under the bisector
        group.append(_outlined(sheet_ref, (0.0, radius * 0.5), style, ramp, anchor="center"))
    return group


def _content_group(content: LeaderContent, at: Point, style: SymbolStyle, ramp: list[str]) -> "drawsvg.Group":
    match content:  # the leader-landing content; the total sub-match over `LeaderContent`
        case LeaderContent(tag="note", note=text):
            return _outlined(text, at, style, ramp)
        case LeaderContent(tag="keynote", keynote=(code, bubble, sheet_ref, mask)):
            return _bubble_group(code, bubble, at, style, ramp, sheet_ref, mask=mask)
        case LeaderContent(tag="flag", flag=(number, shape, mask)):
            return _bubble_group(number, shape, at, style, ramp, "", mask=mask)
        case _ as unreachable:
            assert_never(unreachable)


def _leader_group(
    targets: tuple[Point, ...], landing: Point, path: LeaderPath, content: LeaderContent, style: SymbolStyle, ramp: list[str]
) -> "drawsvg.Group":
    stroke = ramp[style.stroke % len(ramp)]
    home = (landing[0] + _SHOULDER * style.text_height.mm, landing[1])
    group = drawsvg.Group()
    for target in targets:  # one ISO 128-2 reference line per pointed feature (multileader), terminator at the point
        line = drawsvg.Path(stroke=stroke, stroke_width=_mm(style.weight), fill="none", marker_start=f"url(#term-{style.terminator.value})")
        line.M(target[0], target[1])
        _elbow(line, target, landing, path)
        line.L(home[0], home[1])  # the horizontal landing shoulder
        group.append(line)
    group.append(_content_group(content, home, style, ramp))
    return group


def _elbow(line: "drawsvg.Path", target: Point, landing: Point, path: LeaderPath, /) -> None:
    match path:  # Exemption: drawsvg.Path is the stateful cursor builder; the reference line appends in place
        case LeaderPath.SPLINE:
            line.Q(landing[0], target[1], landing[0], landing[1])
        case LeaderPath.STRAIGHT:
            line.L(landing[0], landing[1])
        case _ as unreachable:
            assert_never(unreachable)


def _note_group(insert: Point, body: NoteBody, style: SymbolStyle, ramp: list[str]) -> "drawsvg.Group":
    match body:  # the standalone note body; the total sub-match over `NoteBody`
        case NoteBody(tag="prose", prose=(source, run_bytes, broken)):
            return _prose_group(source, run_bytes, broken, insert, style, ramp)
        case NoteBody(tag="math", math=source):
            # the mixed text-and-`$math$` engineering note MEASURED and baseline-seated through `ziamath.Text`
            # (the mixed-content owner, not the bare `Math`): `getyofst()` seats the run baseline on `insert` and
            # `getsize()` (read in `_text_span`) sizes the canvas, never the blind top-left translate.
            run = ziamath.Text(source, size=style.text_height.mm, color=ramp[style.stroke % len(ramp)])
            return drawsvg.Group(drawsvg.Raw(run.svg()), transform=f"translate({insert[0]},{insert[1] - run.getyofst()})")
        case _ as unreachable:
            assert_never(unreachable)


def _prose_group(source: str, run_bytes: bytes, broken: LineBrokenRun, insert: Point, style: SymbolStyle, ramp: list[str]) -> "drawsvg.Group":
    # the Knuth-Plass general note: decode the shaped run, slice `source` per `LineBrokenRun` line through the
    # run's glyph clusters, and outline each line at its own baseline through ziafont — the total-fit break is
    # the layout owner's, the outlining this owner's.
    run = msgspec.msgpack.decode(run_bytes, type=PositionedGlyphRun).glyphs
    group = drawsvg.Group(transform=f"translate({insert[0]},{insert[1]})")
    for order, line in enumerate(broken.lines):
        lo = run[line.start][1] if line.start < len(run) else 0
        hi = run[line.stop][1] if line.stop < len(run) else len(source)
        group.append(_outlined(source[lo:hi], (0.0, order * style.text_height.mm * 1.4), style, ramp))
    return group


def _revcloud_group(region: tuple[Point, ...], radius: float, mark: str, style: SymbolStyle, ramp: list[str]) -> "drawsvg.Group":
    # the revision enclosure as one self-contained `drawsvg.Path` scallop chain (the correct default); a filled
    # scalloped-band variant composes the landed `graphic/vector/region#REGION` `outline`/`boolean` skia-pathops surface.
    band = drawsvg.Path(stroke=ramp[style.stroke % len(ramp)], stroke_width=_mm(style.weight), fill="none")
    _scallop_path(band, region, radius)
    group = drawsvg.Group()
    group.append(band)
    if mark:  # the revision-delta tag at the first vertex
        group.append(_bubble_group(mark, BubbleShape.TRIANGLE, region[0], style, ramp, "", mask=True))
    return group


def _scallop_path(path: "drawsvg.Path", region: tuple[Point, ...], radius: float, /) -> None:
    # Exemption: drawsvg.Path is the stateful cursor builder; the scallop chain appends convex arc bumps in place
    path.M(*region[0])
    for a, b in zip(region, (*region[1:], region[0]), strict=True):
        bumps = max(1, round(math.dist(a, b) / (2.0 * radius)))
        for i in range(1, bumps + 1):
            end = (a[0] + (b[0] - a[0]) * i / bumps, a[1] + (b[1] - a[1]) * i / bumps)
            path.A(radius, radius, 0, False, True, end[0], end[1])
    path.Z()


# --- [BOUNDARIES] -----------------------------------------------------------------------
def _svg_mark(mark: AnnotateOp, ramp: list[str], landing: Point | None) -> "drawsvg.Group":
    match mark:
        case AnnotateOp(tag="leader", leader=(targets, home, path, content, style)):
            return _leader_group(targets, landing or home, path, content, style, ramp)
        case AnnotateOp(tag="textnote", textnote=(insert, body, style)):
            return _note_group(insert, body, style, ramp)
        case AnnotateOp(tag="revcloud", revcloud=(region, radius, mark_no, style)):
            return _revcloud_group(region, radius, mark_no, style, ramp)
        case _ as unreachable:
            assert_never(unreachable)


def _layer_svg(name: str, groups: tuple["drawsvg.Group", ...], box: Box) -> bytes:
    # bucket one `SymbolStyle.layer`'s marks into a named `drawsvg.Group` under one `Drawing` carrying the shared
    # reusable `Terminator` `Marker(orient='auto')` defs the leader `marker_start` refs resolve against.
    canvas = drawsvg.Drawing(box[2] - box[0], box[3] - box[1], origin=(box[0], box[1]))
    for term in Terminator:  # Exemption: drawsvg Drawing is the mutable def/child container; defs and groups append in place
        if term is not Terminator.NONE:
            canvas.append_def(_marker(term))
    layer = drawsvg.Group(id=name)
    for group in groups:
        layer.append(group)
    canvas.append(layer)
    return canvas.as_svg().encode()


def _dxf_leader(
    doc: "ezdxf.document.Drawing", msp: object, targets: tuple[Point, ...], landing: Point, content: LeaderContent, style: SymbolStyle
) -> None:
    match content:
        case LeaderContent(tag="note", note=text):
            builder = msp.add_multileader_mtext("Standard")  # Exemption: the mleader builder is the stateful sink; leader lines add in place
            # the fuller `MTextEditor` surface the flat `.append` discards — the ISO 3098 lettering face inline (the
            # height rides the builder `char_height`); a multi-item keynote legend deepens to `.bullet_list` here.
            builder.set_content(str(MTextEditor().font("isocp").append(text)), char_height=style.text_height.mm)
            for target in targets:
                builder.add_leader_line(_side(target, landing), [Vec2(target), Vec2(landing)])
            builder.build(insert=Vec2(landing))
        case LeaderContent(tag="keynote", keynote=(code, bubble, sheet_ref, mask)):
            _dxf_bubble_leader(doc, msp, targets, landing, code, bubble, style, mask=mask)
        case LeaderContent(tag="flag", flag=(number, shape, mask)):
            _dxf_bubble_leader(doc, msp, targets, landing, number, shape, style, mask=mask)
        case _ as unreachable:
            assert_never(unreachable)


def _dxf_bubble_leader(
    doc: "ezdxf.document.Drawing",
    msp: object,
    targets: tuple[Point, ...],
    landing: Point,
    label: str,
    shape: BubbleShape,
    style: SymbolStyle,
    *,
    mask: bool,
) -> None:
    block = _bubble_block(doc, shape, style)
    builder = msp.add_multileader_block("Standard")  # Exemption: the block mleader builder is the stateful sink; leader lines add in place
    builder.set_content(block, scale=style.text_height.mm)
    builder.set_attribute("LABEL", label)
    for target in targets:
        builder.add_leader_line(_side(target, landing), [Vec2(target), Vec2(landing)])
    builder.build(insert=Vec2(landing))
    if mask:  # the wipeout backing so the code reads over drawing geometry
        msp.add_wipeout(_polygon_at(shape, landing, _BUBBLE * style.text_height.mm))


def _dxf_note(msp: object, insert: Point, body: NoteBody, style: SymbolStyle) -> None:
    attribs = GfxAttribs(layer=style.layer.compose(), lineweight=_lw(style.weight)).asdict()
    match body:  # Exemption: ezdxf MText is the stateful entity; the location/width set in place
        case NoteBody(tag="prose", prose=(source, _run, broken)):
            content = str(MTextEditor().font("isocp").height(style.text_height.mm).append(source))  # ISO 3098 face + height the raw source lacks
            note = msp.add_mtext(content, dxfattribs=attribs)
            note.dxf.width = max((line.advance for line in broken.lines), default=style.text_height.mm)
            note.set_location(Vec2(insert), attachment_point=MTextEntityAlignment.TOP_LEFT)
        case NoteBody(tag="math", math=source):
            msp.add_mtext(source, dxfattribs=attribs).set_location(Vec2(insert), attachment_point=MTextEntityAlignment.TOP_LEFT)
        case _ as unreachable:
            assert_never(unreachable)


def _dxf_revcloud(msp: object, region: tuple[Point, ...], radius: float, style: SymbolStyle) -> None:
    # the scalloped enclosure as one bulged LWPolyline — each convex bump a `(x, y, 0, 0, 1.0)` semicircle-bulge
    # vertex, never a hand-emitted arc series; the `xyseb` format carries the per-vertex bulge.
    points = tuple(
        (a[0] + (b[0] - a[0]) * i / bumps, a[1] + (b[1] - a[1]) * i / bumps, 0.0, 0.0, 1.0)
        for a, b in zip(region, (*region[1:], region[0]), strict=True)
        for bumps in (max(1, round(math.dist(a, b) / (2.0 * radius))),)
        for i in range(1, bumps + 1)
    )
    msp.add_lwpolyline(points, format="xyseb", close=True, dxfattribs=GfxAttribs(layer=style.layer.compose(), lineweight=_lw(style.weight)).asdict())


def _dxf_mark(doc: "ezdxf.document.Drawing", msp: object, mark: AnnotateOp, landing: Point | None) -> None:
    match mark:
        case AnnotateOp(tag="leader", leader=(targets, home, _path, content, style)):
            _dxf_leader(doc, msp, targets, landing or home, content, style)
        case AnnotateOp(tag="textnote", textnote=(insert, body, style)):
            _dxf_note(msp, insert, body, style)
        case AnnotateOp(tag="revcloud", revcloud=(region, radius, _mark_no, style)):
            _dxf_revcloud(msp, region, radius, style)
        case _ as unreachable:
            assert_never(unreachable)


def _bubble_block(doc: "ezdxf.document.Drawing", shape: BubbleShape, style: SymbolStyle) -> str:
    # one reusable keynote/flag block placed by N `add_blockref`, never a per-placement geometry copy
    name = f"ANNOT_{shape.value}"
    if name not in doc.blocks:
        block = doc.blocks.new(name)  # Exemption: ezdxf BlockLayout is the GraphicsFactory sink; the outline + attdef add in place
        block.add_lwpolyline(_polygon_at(shape, (0.0, 0.0), _BUBBLE * style.text_height.mm), close=True)
        block.add_attdef("LABEL", (0.0, 0.0))
    return name


def _side(target: Point, landing: Point, /) -> "ConnectionSide":
    return ConnectionSide.right if target[0] > landing[0] else ConnectionSide.left


def _polygon_at(shape: BubbleShape, at: Point, radius: float, /) -> tuple[tuple[float, float], ...]:
    match shape:  # the closed vertex ring an `add_lwpolyline`/`add_wipeout` takes
        case BubbleShape.CIRCLE:
            return tuple((at[0] + radius * math.cos(math.tau * i / 16), at[1] + radius * math.sin(math.tau * i / 16)) for i in range(16))
        case BubbleShape.RECTANGLE:
            return (
                (at[0] - radius, at[1] - radius),
                (at[0] + radius, at[1] - radius),
                (at[0] + radius, at[1] + radius),
                (at[0] - radius, at[1] + radius),
            )
        case BubbleShape.HEXAGON | BubbleShape.TRIANGLE | BubbleShape.DIAMOND:
            flat = _polygon(_SIDES[shape], radius)
            return tuple((at[0] + flat[2 * i], at[1] + flat[2 * i + 1]) for i in range(_SIDES[shape]))
        case _ as unreachable:
            assert_never(unreachable)


# --- [TABLES] ---------------------------------------------------------------------------
_SIDES: frozendict[BubbleShape, int] = frozendict({BubbleShape.TRIANGLE: 3, BubbleShape.DIAMOND: 4, BubbleShape.HEXAGON: 6})


def _svg_engine(annotate: Annotate) -> tuple[tuple[LayerNode, ...], ArtifactReceipt]:
    ziafont.config.precision = ziamath.config.precision = (
        _PRECISION  # set once inside the serialized offload lane — same d-float bytes -> same content key
    )
    ramp = hex_ramp(annotate.palette)
    routed = _route(Block.of_seq(annotate.marks))
    box = _bbox(annotate.marks)
    groups: dict[str, list["drawsvg.Group"]] = {}
    for index, mark in enumerate(
        annotate.marks
    ):  # Exemption: the drawsvg named-layer tree buckets marks by `SymbolStyle.layer` through a mutable dict of group lists
        groups.setdefault(_style(mark).layer.compose(), []).append(_svg_mark(mark, ramp, routed.get(index)))
    layers = tuple(_row(name=name, source=_layer_svg(name, tuple(items), box), bbox=box) for name, items in sorted(groups.items()))
    key = annotate._key
    return layers, ArtifactReceipt.Drawing(
        key,
        "drawing-annotate",
        len(annotate.marks),
        _LEADER_STYLE,
        int(box[2] - box[0]),
        int(box[3] - box[1]),
        sum(len(layer.source) for layer in layers),
    )


def _dxf_extent(msp: object, fallback: Box, /) -> tuple[int, int]:
    # the TRUE rendered extent over the authored multileader/mtext/wipeout/lwpolyline entities — the finalized
    # `drawing/dimension#DIMENSION` reads `ezdxf.bbox.extents`, never a point-hull the placed blocks and masks miss.
    box = bbox.extents(msp, fast=True)
    return (round(box.size.x), round(box.size.y)) if box.has_data else (int(fallback[2] - fallback[0]), int(fallback[3] - fallback[1]))


def _dxf_engine(annotate: Annotate) -> tuple[tuple[LayerNode, ...], ArtifactReceipt]:
    doc = ezdxf.new("R2018", setup=True)
    msp = doc.modelspace()
    routed = _route(Block.of_seq(annotate.marks))
    box = _bbox(annotate.marks)
    for index, mark in enumerate(annotate.marks):  # Exemption: ezdxf Modelspace is the GraphicsFactory sink; add_* mutate the layout in place
        _dxf_mark(doc, msp, mark, routed.get(index))
    width, height = _dxf_extent(msp, box)
    stream = io.StringIO()
    doc.write(stream)
    data = stream.getvalue().encode()
    key = annotate._key
    return (_row(name="dxf", source=data, bbox=box),), ArtifactReceipt.Drawing(
        key, "drawing-annotate", len(annotate.marks), _LEADER_STYLE, width, height, len(data)
    )


_ENGINES: frozendict[SymbolTarget, AnnotateEngine] = frozendict({
    SymbolTarget.SVG: AnnotateEngine(arm=_svg_engine, engine="drawsvg"),
    SymbolTarget.DXF: AnnotateEngine(arm=_dxf_engine, engine="ezdxf"),
})


# --- [EXPORTS] --------------------------------------------------------------------------
__all__ = ["Annotate", "AnnotateOp", "BubbleShape", "LeaderContent", "LeaderPath", "NoteBody"]
```

`Annotate` is the one ISO 128-2 annotation grammar every leader, keynote, flag-note, general-note, and revision-cloud mark is built from: the closed `AnnotateOp` union (`Leader`/`TextNote`/`RevisionCloud`) with its nested `LeaderContent` (`Note`/`Keynote`/`Flag`) and `NoteBody` (`Prose`/`Math`) sub-families collapses the shared leader geometry and the shared note-block position into one owner per concern, and the `drawing/symbol#SYMBOL` `SymbolTarget` policy value selects the dual lowering — a `drawsvg` named-layer `Group` whose leader `Path` carries an auto-oriented `Marker(orient='auto')` terminator keyed by `style.terminator`, whose content is `ziafont`-outlined and `ziamath`-typeset, and whose revision cloud is a `drawsvg.Path` scallop chain, or an `ezdxf` layout of `add_multileader_mtext`/`add_multileader_block` leaders with `MTextEditor` content, reusable `add_attdef` keynote blocks placed by `add_blockref`, `add_wipeout` masks, `add_mtext` notes, and a bulged `add_lwpolyline` cloud. Every mark composes the drawing plane's one `SymbolStyle` mark-style owner and its `drawing/regime#REGIME`-owned `Terminator`/`LineWeight`/`TextHeight`/`LayerName` ISO vocabulary rather than declaring parallels, palette-indexes to one `visualization/chart/spec#CHART` ramp through `hex_ramp`, and consumes `typography/layout#LAYOUT`'s `LineBrokenRun` and `typography/shape#SHAPE`'s `PositionedGlyphRun` for the Knuth-Plass general note rather than re-breaking or re-shaping. A keynote/flag column distributes collinear and equally spaced through one `kiwisolver.Solver` at graded `strength`, a revision-cloud filled-band variant composes the landed `graphic/vector/region#REGION` `skia-pathops` `outline`/`boolean` over the self-contained `drawsvg`/`ezdxf` stroked scallop base (the correct default), the synchronous render offloads onto the runtime thread lane off the event loop, and every mark's named-layer bytes feed `export/layered#LAYERED` and place onto `composition/sheet#SHEET` directly. The owner contributes one `ArtifactReceipt.Drawing` (mark/entity/byte facts) on the shared receipt family and one `core/plan#PLAN` `ArtifactWork` node keyed by the content identity, composites nothing, and re-renders nothing.
