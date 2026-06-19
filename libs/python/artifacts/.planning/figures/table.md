# [PY_ARTIFACTS_TABLE]

The publication-quality tabular-artifact owner. `TablePlan` is ONE owner over great-tables (Bring-Your-Own-DataFrame: polars first-class, pandas optional) producing styled tables — spanners, value formatting, data-driven coloring, inline sparklines — exported to HTML, LaTeX, and PDF. It is the missing third artifact pillar beside documents and charts, so numeric study results render as formatted tables rather than raw dumps, feeding `reports/report#REPORT` and `documents/emit#DOCUMENT`. great-tables is admitted in the manifest (cp315-clean, pure-Python); this page closes the `PUBLICATION_TABLE_OWNER` idea.

## [1]-[INDEX]

- [1]-[TABLE]: great-tables styled-table export owner over HTML/LaTeX/PDF, carrying the full publication surface (`loc`-targeted cell-style injection, `fmt_image`/`fmt_nanoplot` cells, `tab_options` global-theme, `cols_merge_range`, `data_color(autocolor_text)` merge folds) as a data-driven spec row in the one `reduce`-over-spec pipeline.

## [2]-[TABLE]

- Owner: `TablePlan` the one table owner discriminating export format; `TableFormat` the closed `StrEnum` over `HTML`/`LATEX`/`PDF`; the great-tables `GT` builder is the styling surface, the dataframe the BYODF input. `FORMATTERS` (the value-format row catalogue), `STRUCTURE` (the header/footnote/source-note/stub structural-fold catalogue), and `STYLERS` (the `loc`-targeted `tab_style` cell-style catalogue) are the three data-driven spec-row catalogues the build reduces over.
- Cases: `TableFormat` rows `HTML` (`GT.as_raw_html(inline_css=True)`, host-free, no driver) · `LATEX` (`GT.as_latex`, host-free) · `PDF` (`GT.save` to a vector target through a headless Chrome WebDriver) — HTML and LaTeX are the host-free default set; `save` (PNG/PDF/SVG) rides a Chrome driver and is gated optional.
- Entry: `TablePlan.render` builds the `GT` table from the dataframe, folds the full publication spec (theme, structure, spanners, column-control, value formats, summary rows, cell styles, data-driven fill) through the one `reduce`-over-spec pipeline, dispatches the format, and returns a `RuntimeRail[ContentKey]`; the styling is a data-driven spec, never a per-column imperative loop or a per-feature method chain.
- Auto: the build is one ordered `reduce` chain — the `tab_options(**theme)` global theme map seeds the publication identity; the `STRUCTURE` catalogue folds `tab_header`/`tab_source_note`/`tab_footnote`/`tab_stub` rows; the column-control map folds `cols_label`/`cols_align`/`cols_width`/`cols_hide`/`cols_merge`/`cols_merge_range`; spanners fold the column-group map through `tab_spanner`; value formatting folds the column-to-format map through `FORMATTERS` (`fmt_number`/`fmt_currency`/`fmt_percent`/`fmt_scientific`/`fmt_integer`/`fmt_date`/`fmt_datetime`/`fmt_time`/`fmt_image`/`fmt_nanoplot`/`fmt_markdown`); the summary-row algebra folds `summary_rows`/`grand_summary_rows`; the `STYLERS` catalogue folds `tab_style(style.*, loc.*)` cell-style injection; data-driven coloring folds the domain-to-palette map (from `figures/color#COLOR`) through `data_color(autocolor_text=True)` so the text color derives from the background luminance.
- Receipt: each render contributes `receipt/receipt#RECEIPT` `ArtifactReceipt.Table` carrying the content key and the format.
- Packages: `great-tables` (`GT`; `tab_options`/`tab_header`/`tab_source_note`/`tab_footnote`/`tab_stub`/`tab_spanner`/`tab_style`; `cols_label`/`cols_align`/`cols_width`/`cols_hide`/`cols_merge`/`cols_merge_range`; `fmt_number`/`fmt_currency`/`fmt_percent`/`fmt_scientific`/`fmt_integer`/`fmt_date`/`fmt_datetime`/`fmt_time`/`fmt_image`/`fmt_nanoplot`/`fmt_markdown`; `summary_rows`/`grand_summary_rows`; `data_color`; the `loc.*` selectors and `style.text`/`style.fill`/`style.borders` constructors; `as_raw_html`/`as_latex`/`save`), runtime (`content_identity.ContentIdentity`, `faults.RuntimeRail`/`boundary`).
- Growth: a new export format is one `TableFormat` row; a new structural feature is one `STRUCTURE` catalogue row; a new value format is one `FORMATTERS` row; a new cell style is one `STYLERS` row; the global theme is one `tab_options` map; zero new surface.
- Boundary: no raw data interchange (that stays at `data`); the dataframe arrives as a settled polars/pandas input; `as_raw_html(inline_css=True)` is the no-driver host-free path and the PDF `save` path rides a host-coupled Chrome WebDriver, gated optional and never the default. A per-feature method chain (`gt.tab_header(...).tab_source_note(...).fmt_number(...)`) and a per-cell imperative loop are the deleted forms — every feature folds through the one `reduce`-over-spec pipeline; the `tab_options` theme is one named option-map row carried as the table's publication identity, never a per-call style scatter.

```python signature
from collections.abc import Callable
from enum import StrEnum
from functools import reduce
from io import BytesIO
from typing import Final, assert_never

from great_tables import GT, loc, style
from msgspec import Struct

from rasm.runtime.content_identity import ContentIdentity, ContentKey
from rasm.runtime.faults import RuntimeRail, boundary


class TableFormat(StrEnum):
    HTML = "html"
    LATEX = "latex"
    PDF = "pdf"


type Formatter = Callable[[GT, str], GT]
type Structurer = Callable[[GT, object], GT]
type Styler = Callable[[GT, object, object], GT]

FORMATTERS: Final[dict[str, Formatter]] = {
    "number": lambda gt, col: gt.fmt_number(columns=col),
    "currency": lambda gt, col: gt.fmt_currency(columns=col),
    "percent": lambda gt, col: gt.fmt_percent(columns=col),
    "scientific": lambda gt, col: gt.fmt_scientific(columns=col),
    "integer": lambda gt, col: gt.fmt_integer(columns=col),
    "date": lambda gt, col: gt.fmt_date(columns=col),
    "datetime": lambda gt, col: gt.fmt_datetime(columns=col),
    "time": lambda gt, col: gt.fmt_time(columns=col),
    "image": lambda gt, col: gt.fmt_image(columns=col),
    "nanoplot": lambda gt, col: gt.fmt_nanoplot(columns=col),
    "markdown": lambda gt, col: gt.fmt_markdown(columns=col),
}

STRUCTURE: Final[dict[str, Structurer]] = {
    "header": lambda gt, v: gt.tab_header(title=v["title"], subtitle=v.get("subtitle")),
    "source_note": lambda gt, v: gt.tab_source_note(source_note=v),
    "footnote": lambda gt, v: gt.tab_footnote(footnote=v["text"], locations=v.get("locations")),
    "stub": lambda gt, v: gt.tab_stub(rowname_col=v["rowname"], groupname_col=v.get("group")),
}

COLUMNS: Final[dict[str, Structurer]] = {
    "label": lambda gt, v: gt.cols_label(cases=v),
    "align": lambda gt, v: gt.cols_align(align=v["align"], columns=v["columns"]),
    "width": lambda gt, v: gt.cols_width(cases=v),
    "hide": lambda gt, v: gt.cols_hide(columns=v),
    "merge": lambda gt, v: gt.cols_merge(columns=v["columns"], pattern=v.get("pattern")),
    "merge_range": lambda gt, v: gt.cols_merge_range(col_begin=v["begin"], col_end=v["end"], sep=v.get("sep", "—")),
}

STYLERS: Final[dict[str, Styler]] = {
    "text": lambda gt, target, spec: gt.tab_style(style=style.text(**spec), locations=target),
    "fill": lambda gt, target, spec: gt.tab_style(style=style.fill(**spec), locations=target),
    "borders": lambda gt, target, spec: gt.tab_style(style=style.borders(**spec), locations=target),
}

LOCATIONS: Final[dict[str, Callable[..., object]]] = {
    "body": loc.body,
    "column_labels": loc.column_labels,
    "stub": loc.stub,
    "summary": loc.summary,
    "source_notes": loc.source_notes,
}


class TablePlan(Struct, frozen=True):
    frame: object
    theme: dict[str, object]
    structure: dict[str, object]
    columns: dict[str, object]
    spanners: dict[str, tuple[str, ...]]
    formats: dict[str, str]
    summaries: dict[str, object]
    stylers: tuple[tuple[str, str, dict[str, object]], ...]
    fills: dict[str, object]
    fmt: TableFormat

    def render(self) -> RuntimeRail[ContentKey]:
        return boundary(f"table.{self.fmt}", self._emit)

    def _emit(self) -> ContentKey:
        return ContentIdentity.of(f"table-{self.fmt}", self._build())

    def _build(self) -> bytes:
        themed = GT(self.frame).tab_options(**self.theme) if self.theme else GT(self.frame)
        structured = reduce(lambda gt, item: STRUCTURE[item[0]](gt, item[1]), self.structure.items(), themed)
        columned = reduce(lambda gt, item: COLUMNS[item[0]](gt, item[1]), self.columns.items(), structured)
        spanned = reduce(lambda gt, item: gt.tab_spanner(label=item[0], columns=list(item[1])), self.spanners.items(), columned)
        formatted = reduce(lambda gt, item: FORMATTERS[item[1]](gt, item[0]), self.formats.items(), spanned)
        summarized = reduce(lambda gt, item: gt.summary_rows(fns=item[1], columns=item[0]), self.summaries.items(), formatted)
        styled = reduce(lambda gt, spec: STYLERS[spec[0]](gt, LOCATIONS[spec[1]](), spec[2]), self.stylers, summarized)
        colored = reduce(lambda gt, item: gt.data_color(columns=item[0], palette=item[1], autocolor_text=True), self.fills.items(), styled)
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

No open items. The `GT`/`tab_options(**kwargs)`/`tab_header(title, subtitle)`/`tab_source_note(source_note)`/`tab_footnote(footnote, locations)`/`tab_stub(rowname_col, groupname_col)`/`tab_spanner(label, columns)`/`tab_style(style, locations)`/`cols_label`/`cols_align`/`cols_width`/`cols_hide`/`cols_merge(columns, pattern)`/`cols_merge_range(col_begin, col_end, sep)`/`fmt_number`/`fmt_currency`/`fmt_percent`/`fmt_scientific`/`fmt_integer`/`fmt_date`/`fmt_datetime`/`fmt_time`/`fmt_image`/`fmt_nanoplot`/`fmt_markdown`/`summary_rows(fns, columns)`/`grand_summary_rows`/`data_color(columns, palette, autocolor_text)`/`as_raw_html(inline_css)`/`as_latex`/`save(file, scale)` member spellings, the `loc.body`/`loc.column_labels`/`loc.stub`/`loc.summary`/`loc.source_notes` selectors, and the `style.text`/`style.fill`/`style.borders` constructors verify against the folder `.api` catalogue for `great-tables` (`0.22.0` reflected, cp315-clean). HTML (`as_raw_html`) and LaTeX (`as_latex`) are the host-free no-driver paths and PDF (`save`) rides a host-coupled Chrome WebDriver, gated optional. The `FORMATTERS`/`STRUCTURE`/`COLUMNS`/`STYLERS` row catalogues collapse the per-feature method family into one ordered `reduce`-over-spec fold; `data_color(autocolor_text=True)` derives the cell text color from the background luminance (catalogue `LOCAL_ADMISSION`), and the `tab_options` theme map is the table's named publication identity carried as one row.
