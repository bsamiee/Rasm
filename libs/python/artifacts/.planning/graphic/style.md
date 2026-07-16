# [PY_ARTIFACTS_GRAPHIC_STYLE]

The theme-as-data owner — the s1 SELECT tier where an office's visual identity lives as one `Theme` row set, so switching styles is switching one value and a library-default emit is a defect by construction. A theme selects and composes what the substrates own, holding keys and value specs but never the substrate vocabulary itself: color seeds resolve through `graphic/color/derive#DERIVE`, stroke rows through `drawing/regime#REGIME`, type roles through `typography/font#FONT` and `typography/shape#SHAPE`, sheet grids through `composition/sheet#SHEET`.

Every visual plane composes this seam downward: `composition/sheet#SHEET` reads the sheet-family variant, `document/emit#EMIT` the page masters and running-head roles, `visualization/table#TABLE`/`visualization/chart/spec#SPEC` the table and chart rows (lowered onto a typed `ThemeConfig` via `@theme.register`, never a hand-merged dict), `visualization/diagram/draw#DRAW` the diagram aesthetics and entourage. No receipt, no async — a theme is a frozen value, selection a `Map` read.

## [01]-[INDEX]

- [01]-[STYLE]: the `Theme` row set — the `TypeSystem` modular scale, regime-keyed `StrokeHierarchy`, derive-seeded `PaletteSpec`, figure-ground `GroundRow`, `Entourage` silhouettes, `PageMaster`/`SheetFamily` grid rows, per-plane chart/diagram/table bindings, and the `role`/`pen`/`size` SELECT projections every consumer keys.

## [02]-[STYLE]

- Owner: `Theme` is a frozen struct of row groups, each referencing its owning substrate by key or value spec. A consumer holds one `Theme` and keys it; the theme never renders, converts, or shapes — it selects.
- Type: `size(role)` derives every size from `base_pt` and `ratio`, no per-role literals; `axes` pin via `INSTANCE` and freeze via `FREEZE` for a non-OTL consumer; `leading`/`tracking` feed `typography/layout#LAYOUT` so break correctness composes the theme instead of overriding it.
- Rows: `StrokeHierarchy` maps the closed `Emphasis` axis onto regime `LineWeight` keys — the drafting two-width group law stays regime's, the theme picks which rows express emphasis; `PaletteSpec` seeds derive's `Palette` factory so chart, diagram, and swatch all derive from one seed row; `GroundRow` makes blended transparency theme data over `graphic/layer#LAYER`'s vocabulary; `Entourage` silhouettes are real-world-height-normalized paths placed at drawing scale by the consuming plane.
- Sheet: `PageMaster` is the one page-grid row `document/emit#EMIT`'s `@page` lowering and `composition/sheet#SHEET`'s zones read; `SheetFamily` names the office title-block variant per size class — the ISO 7200 cell grid is sheet's, the theme selects the variant.
- Growth: a new type role is one `ThemeRole` member plus one `TypeRow`; a new emphasis level one `Emphasis` member plus one hierarchy row; a new entourage asset one row; a new per-plane binding one row group; a new office style one `Theme` value — zero new surface anywhere else.
- Boundary: no color math (`graphic/color/derive#DERIVE` resolves seeds); no pen truth (`drawing/regime#REGIME` owns the rows, the theme holds keys); no shaping/layout/font transforms (typography's, the theme holds presets and coordinates); no chart/table/diagram rendering (each plane lowers its own typed config); no sheet geometry (`composition/sheet#SHEET`); no receipt, identity, or rail.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from enum import StrEnum
from typing import TYPE_CHECKING, Final

from expression.collections import Map
from msgspec import Struct

from rasm.artifacts.drawing.regime import LineWeight, TextHeight
from rasm.artifacts.graphic.layer import BlendMode, LayerIntent
from rasm.artifacts.typography.font import AxisLimit

if TYPE_CHECKING:
    from rasm.artifacts.typography.shape import FeatureSpec

# --- [TYPES] ----------------------------------------------------------------------------
class ThemeRole(StrEnum):  # closed typographic role axis every consumer keys
    DISPLAY = "display"
    HEADING = "heading"
    SUBHEADING = "subheading"
    BODY = "body"
    CAPTION = "caption"
    LABEL = "label"
    SCHEDULE = "schedule"
    RUNNING_HEAD = "running_head"
    FOLIO = "folio"


class Emphasis(StrEnum):  # stroke-hierarchy axis mapped onto regime LineWeight rows
    PRIMARY = "primary"
    SECONDARY = "secondary"
    TERTIARY = "tertiary"
    HAIRLINE = "hairline"


class EntourageKind(StrEnum):  # art-directed silhouette classes; assets are per-theme path rows
    FIGURE_STANDING = "figure_standing"
    FIGURE_WALKING = "figure_walking"
    FIGURE_SEATED = "figure_seated"
    TREE_DECIDUOUS = "tree_deciduous"
    TREE_CONIFER = "tree_conifer"
    SHRUB = "shrub"
    VEHICLE_CAR = "vehicle_car"
    VEHICLE_BICYCLE = "vehicle_bicycle"


class SheetVariant(StrEnum):  # office title-block grid variants per sheet size class
    FULL = "full"  # A0/A1 — full strip title block
    COMPACT = "compact"  # A2/A3 — corner block
    MINIMAL = "minimal"  # A4 — footer band


# --- [MODELS] ---------------------------------------------------------------------------
class TypeRow(Struct, frozen=True):
    step: int  # modular-scale exponent — size derives, never a literal
    leading: float  # line-height multiple, consumed by layout LayoutParams
    tracking: float = 0.0  # em/1000 letterspace
    axes: Map[str, float] = Map.empty()  # variable coordinates -> font INSTANCE AxisLimit pins per axis
    features: "FeatureSpec | None" = None  # per-role OpenType preset the shape engine applies


class TypeSystem(Struct, frozen=True):
    family: str  # the theme face — resolved through the font owner's face selection
    base_pt: float
    ratio: float  # the modular scale (1.2 minor third, 1.25 major third, ...)
    rows: Map[ThemeRole, TypeRow]

    def size(self, role: ThemeRole, /) -> float:
        return self.base_pt * self.ratio ** self.rows[role].step

    def pins(self, role: ThemeRole, /) -> Map[str, AxisLimit]:
        # the font INSTANCE pin map for one role — FREEZE lowers the same row for non-OTL consumers.
        return self.rows[role].axes.map(lambda _tag, value: AxisLimit.of(value))


class PaletteSpec(Struct, frozen=True):
    # derive Palette factory args — seeds as LCh strings; derive resolves every target by value.
    seeds: tuple[str, ...]
    count: int
    spacing: str = "perceptual"
    ramp: str = "linear"


class GroundRow(Struct, frozen=True):
    # figure-ground per layer intent: screen opacity + sanctioned blend.
    intent: LayerIntent
    opacity: float
    blend: BlendMode = BlendMode.NORMAL


class Entourage(Struct, frozen=True):
    kind: EntourageKind
    path_d: str  # the silhouette outline, normalized to height_m real-world metres
    height_m: float


class PageMaster(Struct, frozen=True):
    margins_mm: tuple[float, float, float, float]  # top, right, bottom, left
    columns: int
    gutter_mm: float
    baseline_mm: float  # the baseline grid emit's @page lowering and sheet's zones snap to


class SheetFamily(Struct, frozen=True):
    variant: SheetVariant
    master: PageMaster
    title_block: str  # the office title-block grid row key sheet's ISO 7200 cell grid resolves


class Theme(Struct, frozen=True):
    key: str  # office-style identity
    type_system: TypeSystem
    strokes: Map[Emphasis, LineWeight]
    palette: PaletteSpec
    ground: tuple[GroundRow, ...]
    entourage: tuple[Entourage, ...]
    sheet: SheetFamily
    chart: Map[str, str] = Map.empty()  # chart-plane rows -> altair ThemeConfig / lets-plot theme lowering keys
    diagram: Map[str, str] = Map.empty()  # diagram aesthetics rows draw/schematic lower onto GlyphStyle
    table: Map[str, str] = Map.empty()  # table rows table's tab_options lowering consumes

    def role(self, role: ThemeRole, /) -> TypeRow:
        return self.type_system.rows[role]

    def pen(self, emphasis: Emphasis, /) -> LineWeight:
        return self.strokes[emphasis]

    def lettering_height(self, role: ThemeRole, /) -> TextHeight:
        # drafting consumers snap the derived size to the nearest ISO 3098 nominal — the cascade is regime's.
        target = self.type_system.size(role) * 25.4 / 72.0
        return min(TextHeight, key=lambda h: abs(h.mm - target))


# --- [TABLES] ---------------------------------------------------------------------------
# house default row set — one Theme value, replaced whole per office style.
HOUSE: Final[Theme] = Theme(
    key="house",
    type_system=TypeSystem(
        family="Inter",
        base_pt=9.0,
        ratio=1.25,
        rows=Map.of_seq([
            (ThemeRole.DISPLAY, TypeRow(step=4, leading=1.1, axes=Map.of_seq([("wght", 650.0)]))),
            (ThemeRole.HEADING, TypeRow(step=2, leading=1.2, axes=Map.of_seq([("wght", 600.0)]))),
            (ThemeRole.SUBHEADING, TypeRow(step=1, leading=1.25, axes=Map.of_seq([("wght", 550.0)]))),
            (ThemeRole.BODY, TypeRow(step=0, leading=1.4, features={"onum": True, "liga": True})),
            (ThemeRole.CAPTION, TypeRow(step=-1, leading=1.3)),
            (ThemeRole.LABEL, TypeRow(step=-1, leading=1.1, tracking=20.0)),
            (ThemeRole.SCHEDULE, TypeRow(step=-1, leading=1.2, features={"tnum": True, "lnum": True})),
            (ThemeRole.RUNNING_HEAD, TypeRow(step=-1, leading=1.0, tracking=40.0, features={"smcp": True})),
            (ThemeRole.FOLIO, TypeRow(step=-1, leading=1.0, features={"tnum": True})),
        ]),
    ),
    strokes=Map.of_seq([
        (Emphasis.PRIMARY, LineWeight.W050),
        (Emphasis.SECONDARY, LineWeight.W035),
        (Emphasis.TERTIARY, LineWeight.W025),
        (Emphasis.HAIRLINE, LineWeight.W013),
    ]),
    palette=PaletteSpec(seeds=("lch(45% 55 260)", "lch(60% 60 45)", "lch(55% 50 140)"), count=8),
    ground=(
        GroundRow(LayerIntent.REFERENCE, 0.45),
        GroundRow(LayerIntent.FIGURE, 1.0),
        GroundRow(LayerIntent.OVERLAY, 0.85, BlendMode.MULTIPLY),
    ),
    entourage=(),  # office asset rows land per style; the vocabulary admits them as data
    sheet=SheetFamily(
        variant=SheetVariant.FULL,
        master=PageMaster(margins_mm=(20.0, 10.0, 10.0, 20.0), columns=12, gutter_mm=4.0, baseline_mm=3.0),
        title_block="iso7200-strip",
    ),
)

# --- [EXPORTS] --------------------------------------------------------------------------
__all__ = [
    "Emphasis",
    "Entourage",
    "EntourageKind",
    "GroundRow",
    "HOUSE",
    "PageMaster",
    "PaletteSpec",
    "SheetFamily",
    "SheetVariant",
    "Theme",
    "ThemeRole",
    "TypeRow",
    "TypeSystem",
]
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
