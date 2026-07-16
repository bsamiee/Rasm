# [PY_ARTIFACTS_MATH]

`Formula` is the mathematical-typesetting owner of the corpus — every formula, tolerance stack, engineering annotation, and document equation renders through this one page, so no consumer hand-builds math from glyph primitives. `ziamath` owns the layout kernel: `Math` renders presentation MathML, `Latex` renders a LaTeX expression, `Text` renders a mixed text-plus-`$math$` paragraph, all against an OpenType MATH face (the bundled STIX Two Math or a caller face) emitting a standalone SVG string. `ziafont` is the glyph substrate ziamath extends; plain-text outlining stays `typography/shape#SHAPE`, so this owner adds only what is mathematical.

`render` is the substrate fold `drawing/dimension#DIMENSION`, `drawing/annotate#ANNOTATE`, and `visualization/diagram/draw#DRAW` compose inline into their own renders; `emit()` is the producer entry for a standalone formula artifact — the `document/model#MODEL` `FormulaNode` terminal — keyed pre-run over the canonical spec bytes. `MathConstants` projects the uharfbuzz MATH tier once per face (the `has_math_data` gate, the `get_math_constant` axis/gap/scale reads, the stretchy-variant surfaces) as the typed value consumers seat and scale equations with, never a re-derived spacing guess. A GIL-bound XML-parse-and-font-walk kernel crosses the runtime offload seam; SVG rasterization routes to the corpus rasterizers over the emitted string, never here.

## [01]-[INDEX]

- [01]-[MATH]: the ziamath layout owner over the closed `FormulaSpec` source union — `mathml`/`latex`/`mixed` folded by one `render` substrate fold and lowered to the standalone `Document` node by `emit()`, with the uharfbuzz `MathConstants` MATH-tier value consumers seat equations against.

## [02]-[MATH]

- Owner: `Formula` is the one math owner — `(spec, style)` in, laid-out SVG out, one kernel for every consumer. `MathConstants.of(face)` projects the uharfbuzz MATH tier once per face (`has_math_data` falls a MATH-less face to the ziamath bundled default), the typed value a dimension tolerance stack reads to place stacked limits at the correct axis height at drawing scale.
- Cases: `FormulaSpec` closes the input grammar — `mathml` feeds `Math`, `latex` feeds `Latex`, `mixed` feeds `Text` (multi-line, `halign`/`valign`, rotation); `MathStyle` carries `size`, `display`, `variant`, and `color`.
- Entry: `emit()` returns the one `ArtifactWork` node — key minted pre-run over the canonical spec bytes under `CANONICAL_POLICY`, `work=self._emit`, `admission=Admission(keyed=None)`; `_emit` renders once off-loop and mints `ArtifactReceipt.Document(key, len(svg))` threading the same key. Consumers composing `render` inline mint no node, their own producer receipt carrying the composed figure.
- Auto: `render` offloads the GIL-bound ziamath kernel off-loop; provider raises map into `MathFault` at the arm — `parse` a grammar failure, `font` a face without usable tables, `render` a layout failure — recovery keying on the case.
- Output: `Fragment` carries the SVG string plus the seat geometry — `getsize()` the laid-out extent, `getyofst()` the baseline seat — a consumer positions with `seat()` and never re-measures; `MathConstants` carries axis height, fraction-rule thickness, and script scale-downs read off `OTMathConstant`.
- Packages: `ziamath` (`Math`/`Latex`/`Text`, `svg`/`getsize`/`getyofst`), `ziafont` (the glyph substrate), `uharfbuzz` (`has_math_data`, `get_math_constant`, the variant/assembly surfaces), `core/receipt#RECEIPT` (`ArtifactReceipt.Document`, composed never re-declared).
- Growth: a new source grammar is one `FormulaSpec` case plus one dispatch arm; a new style knob is one `MathStyle` field; a new MATH constant read is one `MathConstants` field; a new consumer composes `render` — zero new surface.
- Boundary: no plain-text shaping or outlining (`typography/shape#SHAPE`), no font engineering (`typography/font#FONT` — faces arrive engineered), no rasterization (resvg/vl-convert over the SVG at the consuming plane), no equation semantics (formulas arrive authored; CAS is the compute track), no bidi (math layout is its own directional law). A consumer importing `ziamath` directly, a per-consumer math renderer, and a hand-measured baseline offset are rejected against the one kernel and `seat()`.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from enum import StrEnum
from typing import Literal, Self

from expression import Result, case, tag, tagged_union
from msgspec import Struct

from rasm.artifacts.core.plan import Admission, ArtifactWork
from rasm.artifacts.core.receipt import ArtifactReceipt
from rasm.runtime.faults import RuntimeRail
from rasm.runtime.identity import CANONICAL_POLICY, ContentIdentity, ContentKey
from rasm.runtime.lanes import LanePolicy, Modality

lazy import uharfbuzz as hb
lazy import ziamath

# --- [TYPES] ----------------------------------------------------------------------------
class MathVariant(StrEnum):  # unicode math-variant styling, applied per token run
    NORMAL = "normal"
    BOLD = "bold"
    ITALIC = "italic"
    SCRIPT = "script"
    FRAKTUR = "fraktur"
    DOUBLE_STRUCK = "double-struck"
    SANS = "sans"
    MONO = "mono"


@tagged_union(frozen=True)
class FormulaSpec:
    tag: Literal["mathml", "latex", "mixed"] = tag()
    mathml: str = case()  # presentation MathML -> ziamath.Math
    latex: str = case()  # one LaTeX expression -> ziamath.Latex
    mixed: str = case()  # text + $inline$/$$display$$ paragraph -> ziamath.Text


# --- [MODELS] ---------------------------------------------------------------------------
class MathStyle(Struct, frozen=True):
    size: float = 12.0
    display: bool = True  # block layout; False = inline math axis
    variant: MathVariant = MathVariant.NORMAL
    color: str | None = None  # resolved color value — derive resolves upstream
    linespacing: float = 1.2
    halign: Literal["left", "center", "right"] = "left"
    valign: Literal["top", "center", "base", "axis", "bottom"] = "base"
    rotation: float = 0.0


class MathConstants(Struct, frozen=True):
    # read once per face.
    axis_height: float
    fraction_rule: float
    script_scale: float  # SCRIPT_PERCENT_SCALE_DOWN / 100
    script_script_scale: float
    min_connector_overlap: float

    @classmethod
    def of(cls, face: "hb.Face", size: float, /) -> "Result[Self, MathFault]":
        # Face.has_math_data gates the tier; a MATH-less face is a typed fault the caller falls from.
        if not face.has_math_data:
            return Result.Error(MathFault(font="no MATH table"))
        font = hb.Font(face)
        upem = face.upem or 1000
        read = lambda tag_: font.get_math_constant(hb.OTMathConstant[tag_]) * size / upem
        return Result.Ok(cls(
            axis_height=read("AXIS_HEIGHT"),
            fraction_rule=read("FRACTION_RULE_THICKNESS"),
            script_scale=font.get_math_constant(hb.OTMathConstant.SCRIPT_PERCENT_SCALE_DOWN) / 100.0,
            script_script_scale=font.get_math_constant(hb.OTMathConstant.SCRIPT_SCRIPT_PERCENT_SCALE_DOWN) / 100.0,
            min_connector_overlap=read("MIN_CONNECTOR_OVERLAP"),
        ))


class Fragment(Struct, frozen=True):
    svg: str
    width: float
    height: float
    baseline: float  # y-offset from bbox bottom to the seat line (getyofst)


# --- [ERRORS] ---------------------------------------------------------------------------
@tagged_union(frozen=True)
class MathFault:
    tag: Literal["parse", "font", "render"] = tag()
    parse: str = case()
    font: str = case()
    render: str = case()


# --- [OPERATIONS] -----------------------------------------------------------------------
class Formula(Struct, frozen=True):
    spec: FormulaSpec
    style: MathStyle = MathStyle()
    font: str | None = None  # engineered MATH face path; None = the ziamath bundled STIX Two Math

    async def render(self) -> RuntimeRail[Result[Fragment, MathFault]]:
        # offloaded ziamath layout; provider raises mapped to MathFault at the arm.
        return await LanePolicy.offload(self._laid, modality=Modality.INTERPRETER)

    def _laid(self) -> Result[Fragment, MathFault]:
        match self.spec:
            case FormulaSpec(tag="latex", latex=src):
                r = ziamath.Latex(src, size=self.style.size, font=self.font, inline=not self.style.display, color=self.style.color)
            case FormulaSpec(tag="mathml", mathml=src):
                r = ziamath.Math(src, size=self.style.size, font=self.font)
            case FormulaSpec(tag="mixed", mixed=src):
                r = ziamath.Text(
                    src,
                    size=self.style.size,
                    mathfont=self.font,
                    linespacing=self.style.linespacing,
                    halign=self.style.halign,
                    valign=self.style.valign,
                    rotation=self.style.rotation,
                    color=self.style.color,
                )
        w, h = r.getsize()
        return Result.Ok(Fragment(svg=r.svg(), width=w, height=h, baseline=r.getyofst()))

    def emit(self, /) -> ArtifactWork:
        return ArtifactWork(key=self._key, work=self._emit, parents=(), admission=Admission(keyed=None), cost=0.5)

    @property
    def _key(self) -> ContentKey:
        # key over the spec+style input, computable before layout runs.
        return ContentIdentity.of("formula", (self.spec, self.style, self.font), policy=CANONICAL_POLICY)

    async def _emit(self) -> RuntimeRail[ArtifactReceipt]:
        laid = await self.render()
        return laid.map(lambda frag: ArtifactReceipt.Document(self._key, len(frag.svg.encode())))


def seat(fragment: Fragment, x: float, y: float, /) -> tuple[float, float]:
    # baseline seating: the insertion point that places the fragment's seat line on (x, y).
    return (x, y - fragment.height + fragment.baseline)


# --- [EXPORTS] --------------------------------------------------------------------------
__all__ = [
    "Formula",
    "FormulaSpec",
    "Fragment",
    "MathConstants",
    "MathFault",
    "MathStyle",
    "MathVariant",
    "seat",
]
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
