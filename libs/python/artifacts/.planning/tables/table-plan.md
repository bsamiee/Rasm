# [PY_ARTIFACTS_TABLE_PLAN]

The publication-quality tabular-artifact owner. `TablePlan` is ONE owner over great-tables (Bring-Your-Own-DataFrame: polars first-class, pandas optional) producing styled tables — spanners, value formatting, data-driven coloring, inline sparklines — exported to HTML, LaTeX, and PDF. It is the missing third artifact pillar beside documents and charts, so numeric study results render as formatted tables rather than raw dumps, feeding `reporting/report-plan#REPORT` and `documents/document-plan#DOCUMENT`. great-tables is admitted in the manifest (cp315-clean, pure-Python); this page closes the `PUBLICATION_TABLE_OWNER` idea.

## [1]-[INDEX]

[CLUSTERS]: one cluster — `[2]-[TABLE]`, the great-tables styled-table export owner over HTML/LaTeX/PDF.

## [2]-[TABLE]

- Owner: `TablePlan` the one table owner discriminating export format; `TableFormat` the closed `StrEnum` over `HTML`/`LATEX`/`PDF`; the great-tables `GT` builder is the styling surface, the dataframe the BYODF input.
- Cases: `TableFormat` rows `HTML` (`GT.as_raw_html(inline_css=True)`, host-free, no driver) · `LATEX` (`GT.as_latex`, host-free) · `PDF` (`GT.save` to a vector target through a headless Chrome WebDriver) — HTML and LaTeX are the host-free default set; `save` (PNG/PDF/SVG) rides a Chrome driver and is gated optional.
- Entry: `TablePlan.render` builds the `GT` table from the dataframe, applies the formatting and styling spec (column spanners, value formats, data-driven fill), dispatches the format, and returns a `RuntimeRail[ContentKey]`; the styling is a data-driven spec, never a per-column imperative loop.
- Auto: value formatting folds the column-to-format map through `GT.fmt_number`/`fmt_currency`/`fmt_percent`/`fmt_scientific`/`fmt_date`/`fmt_datetime` and the inline-sparkline arm through `GT.fmt_nanoplot`; spanners fold the column-group map through `GT.tab_spanner`; data-driven coloring folds the domain-to-palette map (from `color-management/colorimetry#COLOR`) through `GT.data_color`.
- Receipt: each render contributes `receipt/artifact-receipt#RECEIPT` `ArtifactReceipt.Table` carrying the content key and the format.
- Packages: `great-tables` (`GT`/`fmt_number`/`fmt_currency`/`fmt_percent`/`fmt_scientific`/`fmt_date`/`fmt_datetime`/`fmt_nanoplot`/`tab_spanner`/`data_color`/`as_raw_html`/`as_latex`/`save`), runtime (`content_identity.ContentIdentity`, `faults.RuntimeRail`/`boundary`).
- Growth: a new export format is one `TableFormat` row; a new styling primitive is one builder fold; zero new surface.
- Boundary: no raw data interchange (that stays at `data`); the dataframe arrives as a settled polars/pandas input; `as_raw_html(inline_css=True)` is the no-driver host-free path and the PDF `save` path rides a host-coupled Chrome WebDriver, gated optional and never the default.

```python signature
from collections.abc import Callable
from enum import StrEnum
from functools import reduce
from io import BytesIO
from typing import Final, assert_never

from great_tables import GT
from msgspec import Struct

from rasm.runtime.content_identity import ContentIdentity, ContentKey
from rasm.runtime.faults import RuntimeRail, boundary


class TableFormat(StrEnum):
    HTML = "html"
    LATEX = "latex"
    PDF = "pdf"


type Formatter = Callable[[GT, str], GT]

FORMATTERS: Final[dict[str, Formatter]] = {
    "number": lambda gt, col: gt.fmt_number(columns=col),
    "currency": lambda gt, col: gt.fmt_currency(columns=col),
    "percent": lambda gt, col: gt.fmt_percent(columns=col),
    "scientific": lambda gt, col: gt.fmt_scientific(columns=col),
    "date": lambda gt, col: gt.fmt_date(columns=col),
    "datetime": lambda gt, col: gt.fmt_datetime(columns=col),
    "nanoplot": lambda gt, col: gt.fmt_nanoplot(columns=col),
}


class TablePlan(Struct, frozen=True):
    frame: object
    spanners: dict[str, tuple[str, ...]]
    formats: dict[str, str]
    fills: dict[str, object]
    fmt: TableFormat

    def render(self) -> RuntimeRail[ContentKey]:
        return boundary(f"table.{self.fmt}", self._emit)

    def _emit(self) -> ContentKey:
        return ContentIdentity.of(f"table-{self.fmt}", self._build())

    def _build(self) -> bytes:
        spanned = reduce(
            lambda gt, item: gt.tab_spanner(label=item[0], columns=list(item[1])),
            self.spanners.items(),
            GT(self.frame),
        )
        formatted = reduce(
            lambda gt, item: FORMATTERS[item[1]](gt, item[0]),
            self.formats.items(),
            spanned,
        )
        colored = reduce(
            lambda gt, item: gt.data_color(columns=item[0], palette=item[1]),
            self.fills.items(),
            formatted,
        )
        match self.fmt:
            case TableFormat.HTML:
                return colored.as_raw_html(inline_css=True).encode()
            case TableFormat.LATEX:
                return colored.as_latex().encode()
            case TableFormat.PDF:
                sink = BytesIO()
                colored.save(sink, scale=2.0)
                return sink.getvalue()
            case _:
                assert_never(self.fmt)
```

## [3]-[RESEARCH]

No open items. The `GT`/`tab_spanner(label, columns)`/`fmt_number`/`fmt_currency`/`fmt_percent`/`fmt_scientific`/`fmt_date`/`fmt_datetime`/`fmt_nanoplot(columns)`/`data_color(columns, palette)`/`as_raw_html(inline_css)`/`as_latex`/`save(file, scale)` member spellings verify against the folder `.api` catalogue for `great-tables`; HTML (`as_raw_html`) and LaTeX (`as_latex`) are the host-free no-driver paths and PDF (`save`) rides a host-coupled Chrome WebDriver, gated optional. The `FORMATTERS` row catalogue collapses the per-format method family — including the inline-sparkline `fmt_nanoplot` arm the page charter promises — into one data-driven fold.
