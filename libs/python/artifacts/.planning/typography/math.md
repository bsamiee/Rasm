# [PY_ARTIFACTS_TYPOGRAPHY_MATH]

THE mathematical-typesetting owner of the corpus — every formula, tolerance stack, engineering annotation, and document equation renders through this one page, so no consumer hand-builds math from glyph primitives. `ziamath` owns the layout kernel: `Math` renders presentation MathML, `Latex` (IS-A `Math`) renders a LaTeX expression through `latex2mathml`, and `Text` renders a mixed text-plus-`$math$` paragraph — all against an OpenType MATH-table face (the bundled STIX Two Math or a caller face), emitting a standalone SVG string or drawing in place onto a caller `ET.Element` via `drawon` with the `axis`/`base` vertical anchors an AEC label needs to seat against a dimension line. `ziafont` is the glyph substrate ziamath extends; the plain-text outline lane stays `typography/shape#SHAPE`'s `to_svg_path` — this owner adds only what is mathematical. The uharfbuzz MATH tier backs the layout-quality surface: `Face.has_math_data` gates the tier, `Font.get_math_constant(OTMathConstant)` reads axis height, fraction gaps, and script scale-downs, and `get_math_glyph_variants`/`get_math_glyph_assembly` expose the stretchy-delimiter machinery — the `MathConstants` value consumers read to seat and scale equations, never a re-derived spacing guess.

`FormulaSpec` is the closed source union (`mathml`/`latex`/`mixed`) with its style band (size, display/inline, math-variant, resolved color value); `render` is the substrate fold `drawing/dimension#DIMENSION`, `drawing/annotate#ANNOTATE`, and `visualization/diagram/draw#DRAW` compose inline into their own renders; `emit()` is the producer entry for a standalone formula artifact (the `document/model#MODEL` `FormulaNode` terminal), landing the one node contract with its pre-run input key. The GIL-bound XML-parse-and-font-walk kernel crosses the runtime offload seam; SVG rasterization routes to the corpus rasterizers over the emitted string, never here.

## [01]-[INDEX]

- [01]-[MATH]: the `FormulaSpec` source union and `MathStyle` band, the `Formula` owner whose `render` substrate fold every drawing/diagram consumer composes and whose `emit()`/`_emit` pair lands the standalone `Document` producer node, the `MathConstants` uharfbuzz MATH-tier projection (`has_math_data` gate, `get_math_constant` reads, variant/assembly surfaces), the `seat()` baseline-seating fold over `getyofst`/`axis`, and the closed `MathFault` rail — imported by dimension, annotate, draw, and the document model; importing shape and font downward.

## [02]-[MATH]

- Owner: `Formula` is the one math owner — `(spec, style)` in, laid-out SVG out, one kernel for every consumer. The source union closes the input grammar: `mathml` feeds `Math` directly, `latex` feeds `Latex`, `mixed` feeds `Text` (multi-line, `halign`/`valign`, rotation). One egress family serves all three: the standalone SVG string (the durable artifact and the consumer fragment), `getsize()` the laid-out extent, `getyofst()` the baseline seat — a consumer positions the fragment with `seat()` and never re-measures.
- Constants: `MathConstants.of(face)` projects the uharfbuzz MATH tier once per face — `Face.has_math_data` gates (a MATH-less face falls to the ziamath bundled default), `Font.get_math_constant(OTMathConstant)` reads `AXIS_HEIGHT`, `FRACTION_RULE_THICKNESS`, `SCRIPT_PERCENT_SCALE_DOWN`, `MIN_CONNECTOR_OVERLAP` and kin, `get_math_glyph_variants`/`get_math_glyph_assembly` expose vertical/horizontal variant selection — the typed value a dimension tolerance stack reads to place stacked limits at the correct axis height at drawing scale.
- Cases: `MathStyle` carries `size` (pt), `display` (block vs inline layout), `variant` (the unicode math-variant styling — bold/italic/script/fraktur/double-struck/sans/mono), and `color` (a RESOLVED value string — derive resolves upstream, never a literal here); `mixed` adds `linespacing`/`halign`/`valign`/`rotation` through the `Text` constructor's own band.
- Entry: `emit()` returns the ONE `ArtifactWork` node — `key` minted PRE-RUN over the canonical spec bytes (`ContentIdentity.of` under `CANONICAL_POLICY`), `work=self._emit`, `admission=Admission(keyed=None)`; `_emit` renders once off-loop and mints `ArtifactReceipt.Document(key, len(svg))` threading the SAME key so `receipt.slot == node.key`. Consumers composing `render` inline mint no node — their own producer receipt carries the composed figure.
- Auto: `render` offloads the ziamath kernel through `LanePolicy.offload(..., modality=Modality.INTERPRETER)` — the `xml.etree` parse, the MATH-table walk, and the layout fold are GIL-bound shared-address work; provider raises map into `MathFault` at the arm (`parse` a MathML/LaTeX grammar failure, `font` a face without usable tables, `render` a layout failure) — recovery keys on the case.
- Growth: a new source grammar is one `FormulaSpec` case plus one dispatch arm; a new style knob is one `MathStyle` field; a new MATH constant read is one `MathConstants` field; a new consumer composes `render` — zero new surface.
- Boundary: no plain-text shaping or outlining (`typography/shape#SHAPE`); no font engineering (`typography/font#FONT` — instancing/subsetting/freezing arrive as engineered faces); no rasterization (resvg/vl-convert over the SVG string at the consuming plane); no equation SEMANTICS (formulas arrive authored; CAS work is the compute track); no bidi (math layout is its own directional law). A consumer importing `ziamath` directly, a per-consumer math renderer, and a hand-measured baseline offset are the deleted forms.

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
class MathVariant(StrEnum):  # unicode math-variant styling ziamath applies per token run
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
    color: str | None = None  # RESOLVED color value — derive resolves upstream
    linespacing: float = 1.2  # mixed-paragraph band
    halign: Literal["left", "center", "right"] = "left"
    valign: Literal["top", "center", "base", "axis", "bottom"] = "base"
    rotation: float = 0.0


class MathConstants(Struct, frozen=True):
    # the uharfbuzz MATH-tier projection consumers seat equations with — read once per face.
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
    # one laid-out formula: the SVG string plus the seat geometry a consumer positions with.
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
        # the substrate fold dimension/annotate/draw compose inline — one offloaded ziamath layout,
        # provider raises mapped to MathFault at the arm.
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
        # key-over-input: the canonical spec+style bytes, computable BEFORE layout runs.
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

One kernel replaces seven hand-builds: a dimension tolerance stack renders `FormulaSpec(latex=...)` at its ISO 3098-derived size and seats the fragment with `seat()` against the dimension line's axis; an annotation note interleaves prose and math through the `mixed` case; a diagram label routes its formula here and its plain text through the shape engine; the document model's `FormulaNode` lowers to the standalone `emit()` node whose receipt is content-addressed over the INPUT spec, so a re-issued document re-renders only changed formulas. `MathConstants` makes the OpenType MATH table a typed read — axis height and script scaling are facts of the face, not magic numbers in a consumer — and every face arrives engineered through the font owner, keeping this page a pure layout seam between typography's substrates and the planes that draw.
