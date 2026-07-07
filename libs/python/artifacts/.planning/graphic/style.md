# [PY_ARTIFACTS_GRAPHIC_STYLE]

The theme-as-DATA owner — the s1 SELECT tier where an office's visual identity lives as one row set, so switching styles is switching one `Theme` and library-default output is a defect by construction. A theme selects and composes what the substrates own: its color system is derive `Palette` seeds in LCh (values resolve through `graphic/color/derive#DERIVE`, never a literal hex), its stroke hierarchy SELECTS `drawing/regime#REGIME` pen and weight rows by key, its TYPE SYSTEM rows carry the scale ratio, per-role leading/tracking, per-role variable-axis coordinates (pinned through `typography/font#FONT` `INSTANCE` and frozen through `FREEZE`/`RemapByOTL` for non-OpenType consumers), and per-role OpenType feature presets over `typography/shape#SHAPE` `FeatureSpec` (tabular figures for schedules, small caps for running heads), its SHEET FAMILY rows carry title-block grid variants and page masters (margins, columns, baseline grid), its figure-ground rows fix transparency and blend conventions, and its ENTOURAGE rows carry the art-directed silhouette assets (scale figures, trees, vehicles) as vector-path data placed by the diagram and sheet planes.

Every visual plane composes the binding seam downward: `composition/sheet#SHEET` reads the sheet-family variant, `document/emit#EMIT` the page masters and running-head type roles, `visualization/table#TABLE` the table rows, `visualization/chart/spec#SPEC` the chart rows (lowered onto typed `ThemeConfig` via `@theme.register`, never a hand-merged dict), `visualization/diagram/draw#DRAW` and `schematic` the diagram aesthetics and entourage rows. This page mints no receipt and runs no async — a theme is a frozen value, selection is a `Map` read.

## [01]-[INDEX]

- [01]-[STYLE]: the `Theme` row set and its axes — `TypeSystem`/`TypeRow` the modular type scale with per-role axes/features/leading, `StrokeHierarchy` the regime-keyed emphasis rows, `PaletteSpec` the derive-seeded color system, `GroundRow` the figure-ground and transparency conventions, `Entourage`/`EntourageKind` the silhouette asset rows, `PageMaster`/`SheetFamily` the grid/margin/zone/title-block rows, the per-plane `ChartRows`/`DiagramRows`/`TableRows` bindings, and `Theme.role()`/`Theme.pen()`/`Theme.size()` the SELECT projections every consumer keys.

## [02]-[STYLE]

- Owner: `Theme` is the one office-identity carrier — a frozen struct of row groups, each row referencing an owning substrate by key or by value spec, never re-declaring substrate vocabulary. A consumer holds one `Theme` and keys it (`theme.role(ThemeRole.SCHEDULE)`, `theme.pen(Emphasis.PRIMARY)`, `theme.sheet.master`); the theme never renders, converts, or shapes — it selects.
- Type: `TypeSystem` fixes `base_pt` and the modular `ratio`; `TypeRow(step, leading, tracking, axes, features)` places each `ThemeRole` on the scale — `size(role) = base_pt · ratio^step` derives every size from two numbers (no per-role size literals); `axes` are the variable-font coordinates the font owner pins via `INSTANCE` (an `AxisLimit` pin per axis) and freezes via `FREEZE` for a consumer that cannot carry OpenType Layout; `features` is the per-role `FeatureSpec` preset the shape engine applies (`tnum`/`lnum` on `SCHEDULE`, `smcp` on `RUNNING_HEAD`, `onum`/`liga` on `BODY`); `leading`/`tracking` feed `typography/layout#LAYOUT`'s `LayoutParams` so break correctness composes the theme instead of overriding it.
- Rows: `StrokeHierarchy` maps the closed `Emphasis` axis onto regime `LineWeight` keys (primary/secondary/tertiary/hairline — the drafting two-width group law stays regime's; the theme picks WHICH rows express emphasis); `PaletteSpec` seeds derive's `Palette` factory (seed LCh strings, count, spacing, ramp) so a chart's categorical set, a diagram's ink, and a swatch board all derive from one seed row; `GroundRow` fixes figure-ground opacity, screen values, and the sanctioned `BlendMode` conventions per intent (blended transparency as theme DATA over `graphic/layer#LAYER`'s vocabulary); `Entourage` rows carry `(kind, path_d, height_m)` silhouettes — real-world-height-normalized vector paths placed at drawing scale by the consuming plane, per office art direction.
- Sheet: `PageMaster(margins_mm, columns, gutter_mm, baseline_mm)` is the one page-grid row `document/emit#EMIT`'s `@page` lowering and `composition/sheet#SHEET`'s zone system read; `SheetFamily(variant, master, title_block)` names the office title-block grid variant per sheet size class — the ISO 7200 cell grid geometry itself is sheet's; the theme selects the variant.
- Growth: a new type role is one `ThemeRole` member plus one `TypeRow`; a new emphasis level is one `Emphasis` member plus one hierarchy row; a new entourage asset is one row; a new per-plane binding is one row group; a new office style is one `Theme` value — zero new surface anywhere else.
- Boundary: no color math (`graphic/color/derive#DERIVE` resolves seeds); no pen truth (`drawing/regime#REGIME` owns the rows; the theme holds keys); no shaping/layout/font transforms (typography owns them; the theme holds presets and coordinates); no chart/table/diagram rendering (each plane lowers its rows through its own typed config); no sheet geometry (`composition/sheet#SHEET`); no receipt, no identity, no rail.

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
class ThemeRole(StrEnum):  # the closed typographic role axis every document/plane consumer keys
    DISPLAY = "display"
    HEADING = "heading"
    SUBHEADING = "subheading"
    BODY = "body"
    CAPTION = "caption"
    LABEL = "label"
    SCHEDULE = "schedule"
    RUNNING_HEAD = "running_head"
    FOLIO = "folio"


class Emphasis(StrEnum):  # stroke-hierarchy axis — the theme maps it onto regime LineWeight rows
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
    # derive Palette factory arguments — seeds as LCh strings; derive resolves every target model by value.
    seeds: tuple[str, ...]
    count: int
    spacing: str = "perceptual"
    ramp: str = "linear"


class GroundRow(Struct, frozen=True):
    # figure-ground convention per layer intent: screen opacity + sanctioned blend — theme DATA over layer vocabulary.
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
    key: str  # the office-style identity — switching styles is switching this one value
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
# The house default row set — one Theme VALUE, replaced whole per office style; never a scattered override.
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

A theme row references its owning substrate and the consumer lowers it through that substrate's typed surface: `PaletteSpec` feeds derive's `Palette` factory and the resolved swatches thread into altair scales, lets-plot `scale_color_manual`, table styles, and diagram ink; `TypeRow.axes` pins land as font `INSTANCE` jobs and freeze through `RemapByOTL` where a target cannot carry OpenType Layout; `TypeRow.features` presets ride the shape engine's `FeatureSpec` unchanged; `strokes` keys resolve regime weight rows so a chart rule, a diagram edge, and a drawing pen share one emphasis law; `GroundRow` and `Entourage` make blended transparency and art-directed scale furniture theme DATA over the layer vocabulary and V15 geometry. `HOUSE` is the standing default row set — a second office style is a second `Theme` value selected at issue time, and any consumer emitting a library default where a theme row exists is defective.
