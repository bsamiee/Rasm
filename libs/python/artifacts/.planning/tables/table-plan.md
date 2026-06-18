# [PY_ARTIFACTS_TABLE_PLAN]

The publication-quality tabular-artifact owner. `TablePlan` is ONE owner over great-tables (Bring-Your-Own-DataFrame: polars first-class, pandas optional) producing styled tables — spanners, value formatting, data-driven coloring, inline sparklines — exported to HTML, LaTeX, and PDF. It is the missing third artifact pillar beside documents and charts, so numeric study results render as formatted tables rather than raw dumps, feeding `reporting/report-plan#REPORT` and `documents/document-plan#DOCUMENT`. great-tables is not yet admitted in the manifest; this page surfaces the gap the `PUBLICATION_TABLE_OWNER` idea closes.

## [1]-[INDEX]

[CLUSTERS]: one cluster — `[2]-[TABLE]`, the great-tables styled-table export owner over HTML/LaTeX/PDF.

## [2]-[TABLE]

- Owner: `TablePlan` the one table owner discriminating export format; `TableFormat` the closed `StrEnum` over `HTML`/`LATEX`/`PDF`; the great-tables `GT` builder is the styling surface, the dataframe the BYODF input.
- Cases: `TableFormat` rows `HTML` (`GT.as_raw_html`) · `LATEX` (`GT.as_latex`) · `PDF` (`GT.save` to a vector target) — host-free formats; the PNG path rides Selenium and stays out of the host-free default set.
- Entry: `TablePlan.render` builds the `GT` table from the dataframe, applies the formatting and styling spec (column spanners, value formats, data-driven fill), dispatches the format, and returns a `RuntimeRail[ContentKey]`; the styling is a data-driven spec, never a per-column imperative loop.
- Auto: value formatting folds the column-to-format map through `GT.fmt_currency`/`fmt_percent`/`fmt_date`; spanners fold the column-group map through `GT.tab_spanner`; data-driven coloring folds the domain-to-palette map (from `color-management/colorimetry#COLOR`) through `GT.data_color`.
- Receipt: each render contributes `receipt/artifact-receipt#RECEIPT` `ArtifactReceipt.Table` carrying the content key and the format.
- Packages: `great-tables` (`GT`/`fmt_*`/`tab_spanner`/`data_color`/`as_raw_html`/`as_latex`/`save`), runtime (`content_identity.ContentIdentity`, `faults.RuntimeRail`/`boundary`).
- Growth: a new export format is one `TableFormat` row; a new styling primitive is one builder fold; zero new surface.
- Boundary: no raw data interchange (that stays at `data`); the dataframe arrives as a settled polars/pandas input; the PNG path rides host-coupled Selenium and is gated optional, never the default; great-tables admission rides the `PUBLICATION_TABLE_OWNER` task.

```python signature
from enum import StrEnum
from typing import assert_never

from great_tables import GT
from msgspec import Struct

from rasm.runtime.content_identity import ContentIdentity, ContentKey
from rasm.runtime.faults import RuntimeRail, boundary


class TableFormat(StrEnum):
    HTML = "html"
    LATEX = "latex"
    PDF = "pdf"


class TablePlan(Struct, frozen=True):
    frame: object
    spanners: dict[str, tuple[str, ...]]
    formats: dict[str, str]
    fmt: TableFormat

    def render(self) -> RuntimeRail[ContentKey]:
        return boundary(f"table.{self.fmt}", self._emit)

    def _emit(self) -> ContentKey:
        data = _build(self.frame, self.spanners, self.formats, self.fmt)
        return ContentIdentity.of(f"table-{self.fmt}", data)
```

## [3]-[RESEARCH]

- [TABLE_SPELLINGS]: great-tables is not yet admitted in the manifest; the `GT`/`fmt_currency`/`fmt_percent`/`fmt_date`/`tab_spanner`/`data_color`/`as_raw_html`/`as_latex`/`save` member spellings and the host-coupled Selenium PNG gate require a folder `.api` catalogue authored on admission via the `PUBLICATION_TABLE_OWNER` task.
