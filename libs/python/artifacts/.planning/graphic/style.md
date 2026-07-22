# [PY_ARTIFACTS_GRAPHIC_STYLE]

Theme-as-data owns the `SELECT` tier where an office's visual identity lives as one `Theme` value. Theme replacement switches style, and an emitter-level library default is a defect. Theme rows compose substrate-owned values: color seeds and policies resolve through `graphic/color/derive#DERIVE`, strokes through `drawing/regime#REGIME`, type roles through `typography/font#FONT` and `typography/shape#SHAPE`, layer intent through `graphic/layer#LAYER`, and sheet grids through `composition/sheet#SHEET`. `ThemeMode` selects light, dark, or high-contrast `ColorScheme` data over a role-stable `ColorRole` map, while `FamilyStack` supplies one ordered, unique multi-script fallback chain.

Every visual plane composes a typed per-plane binding: `composition/sheet#SHEET` reads `SheetFamily`, `document/emit#DOCUMENT` reads `PageMaster` and running-head roles, table and chart owners read `TableBinding` and `ChartBinding`, and diagram owners read `DiagramBinding`. `DiagramStyle` closes the named ink variants a schematic row can request, and `DiagramBinding.ink_roles` maps every variant to a semantic `ColorRole` that `Theme.diagram_ink` resolves under the selected `ThemeMode`. Frozen dataclasses own domain equality; `Option[FeatureSpec]` owns absent OpenType presets; refined types prove names, family stacks, scales, coordinates, dimensions, and palette inputs; complete-map refinements prove every closed key before selection.

## [01]-[INDEX]

- [01]-[STYLE]: the `Theme` row set — the `TypeSystem` modular scale with a multi-script fallback stack, regime-keyed `StrokeHierarchy`, the `ThemeMode`-keyed `ColorScheme` carrying `ColorRole` seed tokens plus the derive-consumable `PaletteSpec`, figure-ground `GroundRow`, `Entourage` silhouettes, `PageMaster`/`SheetFamily` grid rows, the typed `ChartBinding`/`DiagramBinding`/`TableBinding` per-plane rows, and the `role`/`pen`/`seed`/`size` SELECT projections every consumer keys.

## [02]-[STYLE]

- Owner: `Theme` is a frozen dataclass of complete row groups, each referencing its owning substrate by key or value spec. A consumer holds one `Theme`, selects a `ThemeMode`, and keys it; the theme never renders, converts, or shapes.
- Type: `size(role)` derives every size from `base_pt` and `ratio`, no per-role literals; `families` is the primary face plus its multi-script fallback stack the shape engine walks; `axes` pin via `INSTANCE` and freeze via `FREEZE` for a non-OTL consumer; `leading`/`tracking` feed `typography/layout#LAYOUT` so break correctness composes the theme instead of overriding it.
- Color: `ColorScheme` carries the `ColorRole` semantic token map (primary/surface/on-surface/accent/outline) as LCh seed strings derive resolves by value, plus the `PaletteSpec` whose fields ARE the derive `ColorOp.Palette` arguments — `seed`/`stop`/`count`/`spacing`/`space`/`ramp`/`anchors` in the factory's own types, never a re-parsed string — so a chart, diagram, and swatch all derive from one seed row; a `ThemeMode` selects the light/dark/high-contrast scheme so role tokens stay stable across posture.
- Rows: `StrokeHierarchy` maps the closed `Emphasis` axis onto regime `LineWeight` keys — the drafting two-width group law stays regime's, the theme picks which rows express emphasis; `GroundRow` makes blended transparency theme data over `graphic/layer#LAYER`'s `LayerIntent` and derive's `BlendMode`; `Entourage` silhouettes are real-world-height-normalized paths placed at drawing scale by the consuming plane.
- Sheet: `PageMaster` is the one page-grid row `document/emit#DOCUMENT`'s `@page` lowering and `composition/sheet#SHEET`'s zones read; `SheetFamily` names the office title-block variant per size class — the ISO 7200 cell grid is sheet's, the theme selects the variant.
- Growth: a new type role is one `ThemeRole` member plus one `TypeRow`; a new emphasis level one `Emphasis` member plus one hierarchy row; a new color token one `ColorRole` member plus one scheme seed; a new rendering posture one `ThemeMode` member plus one `ColorScheme`; a new per-plane axis one field on the owning `ChartBinding`/`DiagramBinding`/`TableBinding`; a new entourage asset one row; a new office style one `Theme` value — zero new surface anywhere else.
- Boundary: no color math, compositing vocabulary, pen truth, shaping, layout, font transformation, chart/table/diagram rendering, or sheet geometry. Theme rows carry canonical substrate values and typed per-plane bindings; no `Map[str, str]`, nullable feature preset, receipt, identity, or rail survives inward.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
import hashlib
from dataclasses import asdict, dataclass
from enum import Enum, StrEnum
from math import isfinite
from typing import Annotated, Final

import msgspec
from beartype import beartype
from beartype.vale import Is
from builtins import frozendict
from expression import Nothing, Option, Some
from expression.collections import Map

from rasm.artifacts.drawing.regime import LineWeight, TextHeight
from rasm.artifacts.graphic.color.derive import Amount, BlendMode, ColorModel, ColorText, PaletteCount, Ramp, Spacing
from rasm.artifacts.graphic.layer import LayerIntent
from rasm.artifacts.typography.font import AxisLimit
from rasm.artifacts.typography.shape import FeatureSpec

# --- [TYPES] ----------------------------------------------------------------------------
type NonEmpty = Annotated[str, Is[lambda value: len(value.strip()) > 0]]
type Positive = Annotated[float, Is[lambda value: value > 0.0 and isfinite(value)]]
type ScaleRatio = Annotated[float, Is[lambda value: value > 1.0 and isfinite(value)]]
type AxisTag = Annotated[str, Is[lambda value: len(value) == 4 and value.isascii()]]
type AxisCoordinate = Annotated[float, Is[isfinite]]
type FamilyStack = Annotated[tuple[NonEmpty, ...], Is[lambda values: len(values) > 0 and len(values) == len(set(values))]]
type Columns = Annotated[int, Is[lambda value: value >= 1]]


class ThemeRole(StrEnum):
    DISPLAY = "display"
    HEADING = "heading"
    SUBHEADING = "subheading"
    BODY = "body"
    CAPTION = "caption"
    LABEL = "label"
    SCHEDULE = "schedule"
    RUNNING_HEAD = "running_head"
    FOLIO = "folio"


class ThemeMode(StrEnum):
    LIGHT = "light"
    DARK = "dark"
    HIGH_CONTRAST = "high_contrast"


class ColorRole(StrEnum):
    PRIMARY = "primary"
    ON_PRIMARY = "on_primary"
    PRIMARY_CONTAINER = "primary_container"
    ON_PRIMARY_CONTAINER = "on_primary_container"
    SECONDARY = "secondary"
    ON_SECONDARY = "on_secondary"
    SECONDARY_CONTAINER = "secondary_container"
    ON_SECONDARY_CONTAINER = "on_secondary_container"
    TERTIARY = "tertiary"
    ON_TERTIARY = "on_tertiary"
    TERTIARY_CONTAINER = "tertiary_container"
    ON_TERTIARY_CONTAINER = "on_tertiary_container"
    ACCENT = "accent"
    SURFACE = "surface"
    ON_SURFACE = "on_surface"
    SURFACE_VARIANT = "surface_variant"
    ON_SURFACE_VARIANT = "on_surface_variant"
    BACKGROUND = "background"
    ON_BACKGROUND = "on_background"
    OUTLINE = "outline"
    OUTLINE_VARIANT = "outline_variant"
    ERROR = "error"
    ON_ERROR = "on_error"
    SUCCESS = "success"
    ON_SUCCESS = "on_success"
    WARNING = "warning"
    ON_WARNING = "on_warning"
    INFO = "info"
    ON_INFO = "on_info"
    INVERSE_SURFACE = "inverse_surface"
    INVERSE_ON_SURFACE = "inverse_on_surface"
    SHADOW = "shadow"
    SCRIM = "scrim"


class Emphasis(StrEnum):
    PRIMARY = "primary"
    SECONDARY = "secondary"
    TERTIARY = "tertiary"
    HAIRLINE = "hairline"


class DiagramStyle(StrEnum):
    PRIMARY = "primary"
    SECONDARY = "secondary"
    ACCENT = "accent"
    MUTED = "muted"
    ERROR = "error"
    SUCCESS = "success"
    WARNING = "warning"
    INFO = "info"


class EntourageKind(StrEnum):
    FIGURE_STANDING = "figure_standing"
    FIGURE_WALKING = "figure_walking"
    FIGURE_SEATED = "figure_seated"
    TREE_DECIDUOUS = "tree_deciduous"
    TREE_CONIFER = "tree_conifer"
    SHRUB = "shrub"
    VEHICLE_CAR = "vehicle_car"
    VEHICLE_BICYCLE = "vehicle_bicycle"


class SheetVariant(StrEnum):
    FULL = "full"
    COMPACT = "compact"
    MINIMAL = "minimal"


# --- [MODELS] ---------------------------------------------------------------------------
@beartype
@dataclass(frozen=True, slots=True)
class TypeRow:
    step: int
    leading: Positive
    tracking: AxisCoordinate = 0.0
    axes: Map[AxisTag, AxisCoordinate] = Map.empty()
    features: Option[FeatureSpec] = Nothing


type TypeRows = Annotated[Map[ThemeRole, TypeRow], Is[lambda rows: all(rows.contains_key(role) for role in ThemeRole)]]


@beartype
@dataclass(frozen=True, slots=True)
class TypeSystem:
    families: FamilyStack
    base_pt: Positive
    ratio: ScaleRatio
    rows: TypeRows

    @property
    def family(self) -> NonEmpty:
        return self.families[0]

    def size(self, role: ThemeRole, /) -> float:
        return self.base_pt * self.ratio ** self.rows[role].step

    def pins(self, role: ThemeRole, /) -> Map[AxisTag, AxisLimit]:
        return self.rows[role].axes.map(lambda _tag, value: AxisLimit.of(value))


@beartype
@dataclass(frozen=True, slots=True)
class PaletteSpec:
    seed: Annotated[tuple[ColorText, ...], Is[lambda values: len(values) > 0]]
    stop: ColorText
    count: PaletteCount
    spacing: Spacing = 0.0
    space: ColorModel = ColorModel.OKLCH
    ramp: Ramp = Ramp.Smooth()
    anchors: tuple[ColorText, ...] = ()


type RoleMap = Annotated[Map[ColorRole, ColorText], Is[lambda rows: all(rows.contains_key(role) for role in ColorRole)]]


@beartype
@dataclass(frozen=True, slots=True)
class ColorScheme:
    roles: RoleMap
    palette: PaletteSpec


@beartype
@dataclass(frozen=True, slots=True)
class GroundRow:
    intent: LayerIntent
    opacity: Amount
    blend: BlendMode = BlendMode.NORMAL


@beartype
@dataclass(frozen=True, slots=True)
class Entourage:
    kind: EntourageKind
    path_d: NonEmpty
    height_m: Positive


@beartype
@dataclass(frozen=True, slots=True)
class PageMaster:
    margins_mm: tuple[Positive, Positive, Positive, Positive]
    columns: Columns
    gutter_mm: Positive
    baseline_mm: Positive


@beartype
@dataclass(frozen=True, slots=True)
class SheetFamily:
    variant: SheetVariant
    master: PageMaster
    title_block: NonEmpty


type DiagramInkMap = Annotated[Map[DiagramStyle, ColorRole], Is[lambda rows: all(rows.contains_key(style) for style in DiagramStyle)]]


@beartype
@dataclass(frozen=True, slots=True)
class ChartBinding:
    axis_role: ThemeRole = ThemeRole.LABEL
    title_role: ThemeRole = ThemeRole.SUBHEADING
    grid_emphasis: Emphasis = Emphasis.HAIRLINE
    background: ColorRole = ColorRole.BACKGROUND


@beartype
@dataclass(frozen=True, slots=True)
class DiagramBinding:
    ink_roles: DiagramInkMap
    node_role: ColorRole = ColorRole.SURFACE
    label_role: ThemeRole = ThemeRole.LABEL
    edge_emphasis: Emphasis = Emphasis.SECONDARY


@beartype
@dataclass(frozen=True, slots=True)
class TableBinding:
    heading_role: ThemeRole = ThemeRole.SUBHEADING
    body_role: ThemeRole = ThemeRole.SCHEDULE
    stripe: ColorRole = ColorRole.SURFACE
    rule_emphasis: Emphasis = Emphasis.TERTIARY


type StrokeMap = Annotated[Map[Emphasis, LineWeight], Is[lambda rows: all(rows.contains_key(level) for level in Emphasis)]]
type SchemeMap = Annotated[Map[ThemeMode, ColorScheme], Is[lambda rows: all(rows.contains_key(mode) for mode in ThemeMode)]]
type GroundRows = Annotated[tuple[GroundRow, ...], Is[lambda rows: len({row.intent for row in rows}) == len(rows)]]


def _theme_lowered(raw: object) -> object:
    # msgpack enc_hook for the fingerprint: a Map lowers key-sorted, a frozendict as its dict view, a frozenset sorts,
    # a non-str/int enum by value; every other member is a dataclass asdict already lowered, a Struct, or a scalar.
    if isinstance(raw, Map):
        return dict(raw.items())
    if isinstance(raw, frozendict):
        return dict(raw)
    if isinstance(raw, frozenset):
        return sorted(raw)
    if isinstance(raw, Enum):
        return raw.value
    raise NotImplementedError(type(raw).__name__)


_THEME_CANON: Final = msgspec.msgpack.Encoder(order="deterministic", enc_hook=_theme_lowered)


@beartype
@dataclass(frozen=True, slots=True)
class Theme:
    key: NonEmpty
    type_system: TypeSystem
    strokes: StrokeMap
    schemes: SchemeMap
    ground: GroundRows
    entourage: tuple[Entourage, ...]
    sheet: SheetFamily
    chart: ChartBinding
    diagram: DiagramBinding
    table: TableBinding

    @property
    def fingerprint(self) -> str:
        # content-addressed theme identity over every render-affecting field — two themes sharing one display `key`
        # never collide inside a content preimage, and any styling edit re-keys every artifact that composed it;
        # `key` stays the human label and never enters a hash.
        return hashlib.sha256(_THEME_CANON.encode(asdict(self))).hexdigest()

    def role(self, role: ThemeRole, /) -> TypeRow:
        return self.type_system.rows[role]

    def pen(self, emphasis: Emphasis, /) -> LineWeight:
        return self.strokes[emphasis]

    def seed(self, mode: ThemeMode, role: ColorRole, /) -> ColorText:
        return self.schemes[mode].roles[role]

    def palette(self, mode: ThemeMode, /) -> PaletteSpec:
        return self.schemes[mode].palette

    def diagram_ink(self, mode: ThemeMode, style: DiagramStyle, /) -> ColorText:
        return self.seed(mode, self.diagram.ink_roles[style])

    def lettering_height(self, role: ThemeRole, /) -> TextHeight:
        target = self.type_system.size(role) * 25.4 / 72.0
        return min(TextHeight, key=lambda h: abs(h.mm - target))


# --- [TABLES] ---------------------------------------------------------------------------
HOUSE: Final[Theme] = Theme(
    key="house",
    type_system=TypeSystem(
        families=("Inter", "Noto Sans", "Noto Sans Arabic", "Noto Sans CJK SC"),
        base_pt=9.0,
        ratio=1.25,
        rows=Map.of_seq([
            (ThemeRole.DISPLAY, TypeRow(step=4, leading=1.1, axes=Map.of_seq([("wght", 650.0)]))),
            (ThemeRole.HEADING, TypeRow(step=2, leading=1.2, axes=Map.of_seq([("wght", 600.0)]))),
            (ThemeRole.SUBHEADING, TypeRow(step=1, leading=1.25, axes=Map.of_seq([("wght", 550.0)]))),
            (ThemeRole.BODY, TypeRow(step=0, leading=1.4, features=Some(frozendict({"onum": True, "liga": True})))),
            (ThemeRole.CAPTION, TypeRow(step=-1, leading=1.3)),
            (ThemeRole.LABEL, TypeRow(step=-1, leading=1.1, tracking=20.0)),
            (ThemeRole.SCHEDULE, TypeRow(step=-1, leading=1.2, features=Some(frozendict({"tnum": True, "lnum": True})))),
            (ThemeRole.RUNNING_HEAD, TypeRow(step=-1, leading=1.0, tracking=40.0, features=Some(frozendict({"smcp": True})))),
            (ThemeRole.FOLIO, TypeRow(step=-1, leading=1.0, features=Some(frozendict({"tnum": True})))),
        ]),
    ),
    strokes=Map.of_seq([
        (Emphasis.PRIMARY, LineWeight.W050),
        (Emphasis.SECONDARY, LineWeight.W035),
        (Emphasis.TERTIARY, LineWeight.W025),
        (Emphasis.HAIRLINE, LineWeight.W013),
    ]),
    schemes=Map.of_seq([
        (
            ThemeMode.LIGHT,
            ColorScheme(
                roles=Map.of_seq([
                    (ColorRole.PRIMARY, "lch(45% 55 260)"),
                    (ColorRole.ON_PRIMARY, "lch(100% 0 0)"),
                    (ColorRole.PRIMARY_CONTAINER, "lch(90% 20 260)"),
                    (ColorRole.ON_PRIMARY_CONTAINER, "lch(20% 30 260)"),
                    (ColorRole.SECONDARY, "lch(55% 50 140)"),
                    (ColorRole.ON_SECONDARY, "lch(100% 0 0)"),
                    (ColorRole.SECONDARY_CONTAINER, "lch(91% 18 140)"),
                    (ColorRole.ON_SECONDARY_CONTAINER, "lch(22% 24 140)"),
                    (ColorRole.TERTIARY, "lch(52% 45 315)"),
                    (ColorRole.ON_TERTIARY, "lch(100% 0 0)"),
                    (ColorRole.TERTIARY_CONTAINER, "lch(92% 18 315)"),
                    (ColorRole.ON_TERTIARY_CONTAINER, "lch(22% 25 315)"),
                    (ColorRole.ACCENT, "lch(60% 60 45)"),
                    (ColorRole.SURFACE, "lch(98% 2 260)"),
                    (ColorRole.ON_SURFACE, "lch(20% 4 260)"),
                    (ColorRole.SURFACE_VARIANT, "lch(93% 5 260)"),
                    (ColorRole.ON_SURFACE_VARIANT, "lch(36% 6 260)"),
                    (ColorRole.BACKGROUND, "lch(100% 0 0)"),
                    (ColorRole.ON_BACKGROUND, "lch(15% 3 260)"),
                    (ColorRole.OUTLINE, "lch(70% 3 260)"),
                    (ColorRole.OUTLINE_VARIANT, "lch(82% 3 260)"),
                    (ColorRole.ERROR, "lch(48% 68 25)"),
                    (ColorRole.ON_ERROR, "lch(100% 0 0)"),
                    (ColorRole.SUCCESS, "lch(48% 48 145)"),
                    (ColorRole.ON_SUCCESS, "lch(100% 0 0)"),
                    (ColorRole.WARNING, "lch(62% 68 80)"),
                    (ColorRole.ON_WARNING, "lch(15% 10 80)"),
                    (ColorRole.INFO, "lch(52% 52 245)"),
                    (ColorRole.ON_INFO, "lch(100% 0 0)"),
                    (ColorRole.INVERSE_SURFACE, "lch(24% 3 260)"),
                    (ColorRole.INVERSE_ON_SURFACE, "lch(95% 2 260)"),
                    (ColorRole.SHADOW, "lch(0% 0 0 / 0.35)"),
                    (ColorRole.SCRIM, "lch(0% 0 0 / 0.55)"),
                ]),
                palette=PaletteSpec(seed=("lch(45% 55 260)",), stop="lch(60% 60 45)", count=8),
            ),
        ),
        (
            ThemeMode.DARK,
            ColorScheme(
                roles=Map.of_seq([
                    (ColorRole.PRIMARY, "lch(72% 45 260)"),
                    (ColorRole.ON_PRIMARY, "lch(18% 20 260)"),
                    (ColorRole.PRIMARY_CONTAINER, "lch(34% 34 260)"),
                    (ColorRole.ON_PRIMARY_CONTAINER, "lch(91% 18 260)"),
                    (ColorRole.SECONDARY, "lch(70% 42 140)"),
                    (ColorRole.ON_SECONDARY, "lch(18% 20 140)"),
                    (ColorRole.SECONDARY_CONTAINER, "lch(32% 28 140)"),
                    (ColorRole.ON_SECONDARY_CONTAINER, "lch(90% 16 140)"),
                    (ColorRole.TERTIARY, "lch(72% 40 315)"),
                    (ColorRole.ON_TERTIARY, "lch(20% 20 315)"),
                    (ColorRole.TERTIARY_CONTAINER, "lch(34% 28 315)"),
                    (ColorRole.ON_TERTIARY_CONTAINER, "lch(91% 16 315)"),
                    (ColorRole.ACCENT, "lch(75% 55 45)"),
                    (ColorRole.SURFACE, "lch(22% 3 260)"),
                    (ColorRole.ON_SURFACE, "lch(92% 2 260)"),
                    (ColorRole.SURFACE_VARIANT, "lch(30% 5 260)"),
                    (ColorRole.ON_SURFACE_VARIANT, "lch(80% 5 260)"),
                    (ColorRole.BACKGROUND, "lch(14% 2 260)"),
                    (ColorRole.ON_BACKGROUND, "lch(95% 1 0)"),
                    (ColorRole.OUTLINE, "lch(45% 3 260)"),
                    (ColorRole.OUTLINE_VARIANT, "lch(36% 3 260)"),
                    (ColorRole.ERROR, "lch(68% 60 25)"),
                    (ColorRole.ON_ERROR, "lch(20% 20 25)"),
                    (ColorRole.SUCCESS, "lch(70% 42 145)"),
                    (ColorRole.ON_SUCCESS, "lch(18% 18 145)"),
                    (ColorRole.WARNING, "lch(78% 55 80)"),
                    (ColorRole.ON_WARNING, "lch(20% 16 80)"),
                    (ColorRole.INFO, "lch(72% 42 245)"),
                    (ColorRole.ON_INFO, "lch(18% 18 245)"),
                    (ColorRole.INVERSE_SURFACE, "lch(92% 2 260)"),
                    (ColorRole.INVERSE_ON_SURFACE, "lch(20% 3 260)"),
                    (ColorRole.SHADOW, "lch(0% 0 0 / 0.65)"),
                    (ColorRole.SCRIM, "lch(0% 0 0 / 0.75)"),
                ]),
                palette=PaletteSpec(seed=("lch(72% 45 260)",), stop="lch(75% 55 45)", count=8),
            ),
        ),
        (
            ThemeMode.HIGH_CONTRAST,
            ColorScheme(
                roles=Map.of_seq([
                    (ColorRole.PRIMARY, "lch(35% 70 260)"),
                    (ColorRole.ON_PRIMARY, "lch(100% 0 0)"),
                    (ColorRole.PRIMARY_CONTAINER, "lch(100% 0 0)"),
                    (ColorRole.ON_PRIMARY_CONTAINER, "lch(0% 0 0)"),
                    (ColorRole.SECONDARY, "lch(40% 65 140)"),
                    (ColorRole.ON_SECONDARY, "lch(100% 0 0)"),
                    (ColorRole.SECONDARY_CONTAINER, "lch(100% 0 0)"),
                    (ColorRole.ON_SECONDARY_CONTAINER, "lch(0% 0 0)"),
                    (ColorRole.TERTIARY, "lch(35% 72 315)"),
                    (ColorRole.ON_TERTIARY, "lch(100% 0 0)"),
                    (ColorRole.TERTIARY_CONTAINER, "lch(100% 0 0)"),
                    (ColorRole.ON_TERTIARY_CONTAINER, "lch(0% 0 0)"),
                    (ColorRole.ACCENT, "lch(45% 80 45)"),
                    (ColorRole.SURFACE, "lch(100% 0 0)"),
                    (ColorRole.ON_SURFACE, "lch(0% 0 0)"),
                    (ColorRole.SURFACE_VARIANT, "lch(100% 0 0)"),
                    (ColorRole.ON_SURFACE_VARIANT, "lch(0% 0 0)"),
                    (ColorRole.BACKGROUND, "lch(100% 0 0)"),
                    (ColorRole.ON_BACKGROUND, "lch(0% 0 0)"),
                    (ColorRole.OUTLINE, "lch(0% 0 0)"),
                    (ColorRole.OUTLINE_VARIANT, "lch(0% 0 0)"),
                    (ColorRole.ERROR, "lch(40% 85 25)"),
                    (ColorRole.ON_ERROR, "lch(100% 0 0)"),
                    (ColorRole.SUCCESS, "lch(35% 72 145)"),
                    (ColorRole.ON_SUCCESS, "lch(100% 0 0)"),
                    (ColorRole.WARNING, "lch(45% 80 80)"),
                    (ColorRole.ON_WARNING, "lch(0% 0 0)"),
                    (ColorRole.INFO, "lch(35% 72 245)"),
                    (ColorRole.ON_INFO, "lch(100% 0 0)"),
                    (ColorRole.INVERSE_SURFACE, "lch(0% 0 0)"),
                    (ColorRole.INVERSE_ON_SURFACE, "lch(100% 0 0)"),
                    (ColorRole.SHADOW, "lch(0% 0 0 / 0.8)"),
                    (ColorRole.SCRIM, "lch(0% 0 0 / 0.9)"),
                ]),
                palette=PaletteSpec(seed=("lch(35% 70 260)",), stop="lch(45% 80 45)", count=8, ramp=Ramp.Discrete()),
            ),
        ),
    ]),
    ground=(
        GroundRow(LayerIntent.REFERENCE, 0.45),
        GroundRow(LayerIntent.FIGURE, 1.0),
        GroundRow(LayerIntent.OVERLAY, 0.85, BlendMode.MULTIPLY),
    ),
    entourage=(),
    sheet=SheetFamily(
        variant=SheetVariant.FULL,
        master=PageMaster(margins_mm=(20.0, 10.0, 10.0, 20.0), columns=12, gutter_mm=4.0, baseline_mm=3.0),
        title_block="iso7200-strip",
    ),
    chart=ChartBinding(),
    diagram=DiagramBinding(
        ink_roles=Map.of_seq([
            (DiagramStyle.PRIMARY, ColorRole.PRIMARY),
            (DiagramStyle.SECONDARY, ColorRole.SECONDARY),
            (DiagramStyle.ACCENT, ColorRole.ACCENT),
            (DiagramStyle.MUTED, ColorRole.ON_SURFACE_VARIANT),
            (DiagramStyle.ERROR, ColorRole.ERROR),
            (DiagramStyle.SUCCESS, ColorRole.SUCCESS),
            (DiagramStyle.WARNING, ColorRole.WARNING),
            (DiagramStyle.INFO, ColorRole.INFO),
        ])
    ),
    table=TableBinding(),
)

# --- [EXPORTS] --------------------------------------------------------------------------
__all__ = [
    "ChartBinding",
    "ColorRole",
    "ColorScheme",
    "DiagramBinding",
    "DiagramInkMap",
    "DiagramStyle",
    "Emphasis",
    "Entourage",
    "EntourageKind",
    "GroundRow",
    "HOUSE",
    "PageMaster",
    "PaletteSpec",
    "SheetFamily",
    "SheetVariant",
    "TableBinding",
    "Theme",
    "ThemeMode",
    "ThemeRole",
    "TypeRow",
    "TypeSystem",
]
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
