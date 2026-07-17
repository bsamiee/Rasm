# [PY_ARTIFACTS_MATH]

`Formula` owns mathematical typesetting through one closed `FormulaSpec` family. `MathMLSpec`, `LatexSpec`, and `MixedSpec` carry only the source and style axes their ziamath renderer consumes, so grammar-inapplicable combinations are absent by construction. `ziamath.Math`, `Latex`, and `Text` render against an OpenType MATH face and emit standalone SVG; explicit equation labels, upright operators, math variants, mixed alignment, rotation, line spacing, and the SVG profile stay on their owning case.

`laid()` is the synchronous composition fold, `render()` its offloaded form, and `emit()` the standalone artifact entry. One result carrier survives from provider capture through receipt projection. A reentrant lock brackets ziamath's process-global operator registry and `config.svg2`, restores the SVG profile on every exit, and makes direct `laid()` composition obey the same global-state law as the offloaded form. `MathConstants` derives every `hb.OTMathConstant` member into one frozen map and exposes common consumer names as projections; connector overlap derives for every writing direction.

## [01]-[INDEX]

- [01]-[MATH]: the ziamath layout owner over the closed `FormulaSpec` source union — `mathml`/`latex`/`mixed` folded by one `_laid` kernel, composed inline through `laid()`, offloaded through `render()`, and lowered to the standalone `Document` node by `emit()`, with the uharfbuzz `MathConstants` MATH-tier value consumers seat equations against.

## [02]-[MATH]

- Owner: `Formula` folds `(spec, font, operators)` into one laid-out `Fragment`. `MathConstants.of(face, size)` projects the complete uharfbuzz MATH constant family once per face; `has_math_data` maps a MATH-less face to the typed `font` fault.
- Cases: `FormulaSpec` carries `MathMLSpec`, `LatexSpec`, or `MixedSpec`; each payload contains its renderer's complete source and style policy. `Fragment.math` carries the MathML intermediate, while `Fragment.mixed` makes that inapplicable field unrepresentable.
- Entry: `emit()` mints one key over `(spec ⊕ font ⊕ operators)`, captures it in `partial(self._emit, key)`, and threads it into `ArtifactReceipt.Document`; `laid()` composes without minting a node.
- Auto: `render()` crosses the ziamath kernel as a `KernelTrait.RELEASING` thread kernel — ziamath refuses the subinterpreter import, so the thread arm is its one placement; `_CONFIG_LOCK` serializes global operator and SVG-profile mutation for both sync and async entrypoints, and `finally` restores the prior profile. `ParseError`, `OSError`, and `ValueError` map to distinct `MathFault` cases; an unclassified raise crosses as the runtime boundary fault.
- Output: `Fragment` is a closed `math`/`mixed` family over shared SVG, extent, and baseline values; only `math` carries the MathML intermediate. `MathConstants.values` covers the complete provider enum, normalizes percentage rows, scales dimensional rows, and derives named projections without a parallel value roster.
- Packages: `ziamath` (`Math`/`Latex`/`Text`, `svg`/`getsize`/`getyofst`/`mathmlstr`, `declareoperator`, `config.svg2`), `ziafont` (the glyph substrate), `uharfbuzz` (`has_math_data`, `get_math_constant`, `get_math_min_connector_overlap`), `core/receipt#RECEIPT` (`ArtifactReceipt.Document`, composed never re-declared).
- Growth: a new source grammar is one `FormulaSpec` case and one dispatch arm; a style axis lands on its existing case payload; a new MATH constant requires no owner edit because the enum fold absorbs it; operator vocabulary grows inside `Formula.operators`.
- Exemption: `_laid` is the provider boundary for process-global config, operator registration, parsing, and layout; its lock, `try`/`finally`, and exact exception arms own that forced mutable kernel.
- Boundary: no plain-text shaping or outlining (`typography/shape#SHAPE`), no font engineering (`typography/font#FONT` — faces arrive engineered), no rasterization (resvg/vl-convert over the SVG at the consuming plane), no equation semantics (formulas arrive authored; CAS is the compute track), no bidi (math layout is its own directional law). A consumer importing `ziamath` directly, a per-consumer math renderer, and a hand-measured baseline offset are rejected against the one kernel and `seat()`; a nested `RuntimeRail[Result[...]]` return and a raised provider exception crossing the async edge are rejected against the one-carrier boundary migration.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from builtins import frozendict
from enum import StrEnum
from functools import partial
from threading import RLock
from typing import Final, Literal, assert_never
from xml.etree.ElementTree import ParseError

from expression import Error, Ok, Result, case, tag, tagged_union
from msgspec import Struct, msgpack

from rasm.artifacts.core.plan import Admission, ArtifactWork
from rasm.artifacts.core.receipt import ArtifactReceipt
from rasm.runtime.faults import BoundaryFault, RuntimeRail
from rasm.runtime.identity import ContentIdentity, ContentKey
from rasm.runtime.lanes import LanePolicy
from rasm.runtime.workers import Kernel, KernelTrait

lazy import latex2mathml.commands  # the operator registry ziamath.declareoperator appends to; snapshot/restore rides _laid
lazy import uharfbuzz as hb
lazy import ziamath

# --- [TYPES] ----------------------------------------------------------------------------
class MathVariant(StrEnum):
    NORMAL = "normal"
    BOLD = "bold"
    ITALIC = "italic"
    SCRIPT = "script"
    FRAKTUR = "fraktur"
    DOUBLE_STRUCK = "double"
    SANS = "sans"
    MONO = "mono"


class MathMLSpec(Struct, frozen=True, kw_only=True):
    source: str
    size: float = 12.0
    number: str | None = None
    svg2: bool = True


class LatexSpec(Struct, frozen=True, kw_only=True):
    source: str
    size: float = 12.0
    display: bool = True
    variant: MathVariant = MathVariant.NORMAL
    color: str | None = None
    number: str | None = None
    svg2: bool = True


class MixedSpec(Struct, frozen=True, kw_only=True):
    source: str
    size: float = 12.0
    variant: MathVariant = MathVariant.NORMAL
    color: str | None = None
    linespacing: float = 1.2
    halign: Literal["left", "center", "right"] = "left"
    valign: Literal["top", "center", "base", "axis", "bottom"] = "base"
    rotation: float = 0.0
    svg2: bool = True


@tagged_union(frozen=True)
class FormulaSpec:
    tag: Literal["mathml", "latex", "mixed"] = tag()
    mathml: MathMLSpec = case()
    latex: LatexSpec = case()
    mixed: MixedSpec = case()


# --- [CONSTANTS] ------------------------------------------------------------------------
_CANON: Final = msgpack.Encoder(order="deterministic")  # the stable preimage encoding the bare `ContentIdentity.key` mint addresses
_CONFIG_LOCK: Final = RLock()

# --- [MODELS] ---------------------------------------------------------------------------
class MathConstants(Struct, frozen=True):
    values: frozendict[str, float]
    connector_overlap: frozendict[str, float]

    def value(self, constant: "hb.OTMathConstant", /) -> float:
        return self.values[constant.name]

    @property
    def axis_height(self) -> float:
        return self.values["AXIS_HEIGHT"]

    @property
    def fraction_rule(self) -> float:
        return self.values["FRACTION_RULE_THICKNESS"]

    @property
    def script_scale(self) -> float:
        return self.values["SCRIPT_PERCENT_SCALE_DOWN"]

    @property
    def script_script_scale(self) -> float:
        return self.values["SCRIPT_SCRIPT_PERCENT_SCALE_DOWN"]

    @property
    def stack_top_shift(self) -> float:
        return self.values["STACK_TOP_SHIFT_UP"]

    @property
    def stack_bottom_shift(self) -> float:
        return self.values["STACK_BOTTOM_SHIFT_DOWN"]

    @property
    def stack_gap_min(self) -> float:
        return self.values["STACK_GAP_MIN"]

    @property
    def min_connector_overlap(self) -> float:
        return self.connector_overlap["ltr"]

    @classmethod
    def of(cls, face: "hb.Face", size: float, /) -> Result["MathConstants", "MathFault"]:
        if not face.has_math_data:
            return Error(MathFault(font="no MATH table"))
        font = hb.Font.create(face)
        scale = size / (face.upem or 1000)
        return Ok(
            cls(
                values=frozendict(
                    (
                        constant.name,
                        font.get_math_constant(constant) / 100.0
                        if "PERCENT" in constant.name
                        else font.get_math_constant(constant) * scale,
                    )
                    for constant in hb.OTMathConstant
                ),
                connector_overlap=frozendict(
                    (direction, font.get_math_min_connector_overlap(direction) * scale) for direction in ("ltr", "rtl", "ttb", "btt")
                ),
            )
        )


@tagged_union(frozen=True)
class Fragment:
    tag: Literal["math", "mixed"] = tag()
    math: tuple[str, float, float, float, str] = case()
    mixed: tuple[str, float, float, float] = case()

    @property
    def metrics(self) -> tuple[str, float, float, float]:
        match self:
            case Fragment(tag="math", math=(svg, width, height, baseline, _)) | Fragment(
                tag="mixed", mixed=(svg, width, height, baseline)
            ):
                return (svg, width, height, baseline)
            case _ as unreachable:
                assert_never(unreachable)

    @property
    def svg(self) -> str:
        return self.metrics[0]

    @property
    def width(self) -> float:
        return self.metrics[1]

    @property
    def height(self) -> float:
        return self.metrics[2]

    @property
    def baseline(self) -> float:
        return self.metrics[3]


# --- [ERRORS] ---------------------------------------------------------------------------
@tagged_union(frozen=True)
class MathFault:
    tag: Literal["parse", "font", "render"] = tag()
    parse: str = case()
    font: str = case()
    render: str = case()


# --- [OPERATIONS] -----------------------------------------------------------------------
class Formula(Struct, frozen=True):
    # `lane` arrives projected via LanePolicy.of(context) at the composition root — a capacity literal has no owner.
    spec: FormulaSpec
    lane: LanePolicy
    font: str | None = None
    operators: frozenset[str] = frozenset()

    def laid(self) -> Result[Fragment, MathFault]:
        return _laid(self.spec, self.font, self.operators)

    async def render(self) -> Result[Fragment, MathFault]:
        crossed = await self.lane.offload(Kernel.of(_laid, KernelTrait.RELEASING), self.spec, self.font, self.operators)
        return crossed.map_error(lambda fault: MathFault(render=fault.tag)).bind(lambda outcome: outcome)

    def emit(self, /) -> ArtifactWork:
        # `ContentIdentity.key` is the bare mint (`of` returns the railed `RuntimeRail[ContentKey]`).
        key = ContentIdentity.key("formula", _CANON.encode((self.spec, self.font, tuple(sorted(self.operators)))))
        return ArtifactWork(key=key, work=partial(self._emit, key), parents=(), admission=Admission(keyed=None), cost=0.5)

    async def _emit(self, key: ContentKey, /) -> RuntimeRail[ArtifactReceipt]:
        laid = await self.render()
        return laid.map(lambda frag: ArtifactReceipt.Document(key, len(frag.svg.encode()))).map_error(
            lambda fault: BoundaryFault(boundary=(f"math.{self.spec.tag}", fault.tag))
        )


def _laid(spec: FormulaSpec, font: str | None, operators: frozenset[str], /) -> Result[Fragment, MathFault]:
    with _CONFIG_LOCK:
        previous_svg2 = ziamath.config.svg2
        previous_operators = latex2mathml.commands.FUNCTIONS
        try:
            # per-render operator registry: `declareoperator` APPENDS to `latex2mathml.commands.FUNCTIONS`, a
            # process-global tuple, so each render declares over the pristine baseline and the finally restores it —
            # one formula's vocabulary never leaks into a sibling's parse (a leaked declare would typeset an
            # undeclared token as an operator and poison the content-addressed cache), and repeat renders never
            # grow the tuple unboundedly.
            for name in sorted(operators):
                ziamath.declareoperator(name)
            match spec:
                case FormulaSpec(tag="latex", latex=style):
                    ziamath.config.svg2 = style.svg2
                    mathstyle = None if style.variant is MathVariant.NORMAL else style.variant.value
                    laid = ziamath.Latex(
                        style.source,
                        size=style.size,
                        mathstyle=mathstyle,
                        font=font,
                        color=style.color,
                        inline=not style.display,
                        number=style.number,
                    )
                case FormulaSpec(tag="mathml", mathml=style):
                    ziamath.config.svg2 = style.svg2
                    laid = ziamath.Math(style.source, size=style.size, font=font, number=style.number)
                case FormulaSpec(tag="mixed", mixed=style):
                    ziamath.config.svg2 = style.svg2
                    mathstyle = None if style.variant is MathVariant.NORMAL else style.variant.value
                    laid = ziamath.Text(
                        style.source,
                        size=style.size,
                        mathfont=font,
                        mathstyle=mathstyle,
                        linespacing=style.linespacing,
                        halign=style.halign,
                        valign=style.valign,
                        rotation=style.rotation,
                        color=style.color,
                    )
                case _ as unreachable:
                    assert_never(unreachable)
            width, height = laid.getsize()
            metrics = (laid.svg(), width, height, laid.getyofst())
            return Ok(Fragment(mixed=metrics) if spec.tag == "mixed" else Fragment(math=(*metrics, laid.mathmlstr())))
        except ParseError as bad:
            return Error(MathFault(parse=str(bad)))
        except OSError as missing:
            return Error(MathFault(font=str(missing)))
        except ValueError as bad:
            return Error(MathFault(render=str(bad)))
        finally:
            ziamath.config.svg2 = previous_svg2
            latex2mathml.commands.FUNCTIONS = previous_operators


def seat(fragment: Fragment, x: float, y: float, /) -> tuple[float, float]:
    return (x, y - fragment.height + fragment.baseline)


# --- [EXPORTS] --------------------------------------------------------------------------
__all__ = [
    "Formula",
    "FormulaSpec",
    "Fragment",
    "MathConstants",
    "MathFault",
    "MathMLSpec",
    "LatexSpec",
    "MixedSpec",
    "MathVariant",
    "seat",
]
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
