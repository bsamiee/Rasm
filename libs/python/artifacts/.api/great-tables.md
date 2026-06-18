# [PY_ARTIFACTS_API_GREAT_TABLES]

`great_tables` supplies the publication-quality table rendering surface for the artifacts tables rail: a `GT` builder with a fluent method family for column formatting (`fmt_number`, `fmt_currency`, `fmt_percent`, `fmt_scientific`, `fmt_date`, `fmt_datetime`, `fmt_image`, `fmt_nanoplot`), structural layout (`tab_header`, `tab_spanner`, `tab_source_note`, `tab_footnote`, `tab_stub`), style injection (`tab_style`, `data_color`), column control (`cols_label`, `cols_align`, `cols_hide`, `cols_width`, `cols_merge`), and summary row construction (`summary_rows`, `grand_summary_rows`). The `loc` submodule supplies location selectors; the `style` submodule supplies `text`, `fill`, `borders`, and `css` cell style constructors; `as_raw_html` and `save` emit the final artefact.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `great_tables`
- package: `great-tables`
- import: `great_tables`
- owner: `artifacts`
- rail: tables
- asset: runtime library
- installed: `0.22.0` reflected via `/tmp/wfpy-artifacts315/bin/python`

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: table builder
- rail: tables

| [INDEX] | [SYMBOL] | [PACKAGE_ROLE] | [CAPABILITY]                                                                                  |
| :-----: | :------- | :------------- | :-------------------------------------------------------------------------------------------- |
|   [1]   | `GT`     | table builder  | `(data, rowname_col, groupname_col, auto_align, id, locale)` — fluent table construction root |

[PUBLIC_TYPE_SCOPE]: location selectors (`great_tables.loc`)
- rail: tables

| [INDEX] | [SYMBOL]             | [PACKAGE_ROLE]      | [CAPABILITY]                             |
| :-----: | :------------------- | :------------------ | :--------------------------------------- |
|   [1]   | `loc.body`           | body cells          | select data body by column/row           |
|   [2]   | `loc.column_labels`  | column label cells  | select column label header cells         |
|   [3]   | `loc.column_header`  | column header       | entire column header area                |
|   [4]   | `loc.spanner_labels` | spanner label cells | select spanner label cells by id/level   |
|   [5]   | `loc.stub`           | row stub cells      | select stub (row label) cells            |
|   [6]   | `loc.stubhead`       | stubhead cell       | select the stubhead label cell           |
|   [7]   | `loc.row_group`      | row group label     | select row group label cells             |
|   [8]   | `loc.header`         | header region       | full header area                         |
|   [9]   | `loc.title`          | title cell          | the table title cell                     |
|  [10]   | `loc.subtitle`       | subtitle cell       | the subtitle cell                        |
|  [11]   | `loc.footer`         | footer region       | full footer area                         |
|  [12]   | `loc.source_notes`   | source note cells   | select source note cells                 |
|  [13]   | `loc.summary`        | summary rows        | select summary row cells by group/column |
|  [14]   | `loc.grand_summary`  | grand summary rows  | select grand summary row cells           |

[PUBLIC_TYPE_SCOPE]: cell style constructors (`great_tables.style`)
- rail: tables

| [INDEX] | [SYMBOL]        | [PACKAGE_ROLE] | [CAPABILITY]                                |
| :-----: | :-------------- | :------------- | :------------------------------------------ |
|   [1]   | `style.text`    | text style     | font-family, size, weight, color, transform |
|   [2]   | `style.fill`    | fill style     | background color or gradient                |
|   [3]   | `style.borders` | border style   | border side, width, color, style            |
|   [4]   | `style.css`     | raw CSS        | arbitrary CSS property injection            |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: construction and structure
- rail: tables

| [INDEX] | [SURFACE]                                                              | [ENTRY_FAMILY] | [CAPABILITY]                                    |
| :-----: | :--------------------------------------------------------------------- | :------------- | :---------------------------------------------- |
|   [1]   | `GT(data, rowname_col, groupname_col, auto_align, id, locale)`         | builder init   | create a GT from a DataFrame or dict-like       |
|   [2]   | `GT.tab_header(title, subtitle, preheader)`                            | header         | set title, subtitle, and optional preheader     |
|   [3]   | `GT.tab_spanner(label, columns, spanners, level, id, gather, replace)` | column spanner | group columns under a spanning label            |
|   [4]   | `GT.tab_stub(rowname_col, groupname_col)`                              | stub           | set row name and group name columns             |
|   [5]   | `GT.tab_source_note(source_note)`                                      | source note    | append a source note to the footer              |
|   [6]   | `GT.tab_footnote(footnote, locations, placement)`                      | footnote       | attach a footnote to one or more cell locations |

[ENTRYPOINT_SCOPE]: column operations
- rail: tables

| [INDEX] | [SURFACE]                                                              | [ENTRY_FAMILY] | [CAPABILITY]                                 |
| :-----: | :--------------------------------------------------------------------- | :------------- | :------------------------------------------- |
|   [1]   | `GT.cols_label(cases, **kwargs)`                                       | column rename  | set display labels for columns               |
|   [2]   | `GT.cols_align(align, columns)`                                        | alignment      | `'left'` / `'center'` / `'right'`            |
|   [3]   | `GT.cols_move(columns, after)`                                         | column reorder | move columns after a named column            |
|   [4]   | `GT.cols_hide(columns)`                                                | column hide    | suppress selected columns from display       |
|   [5]   | `GT.cols_width(cases, **kwargs)`                                       | column width   | set explicit pixel/percent widths            |
|   [6]   | `GT.cols_merge(columns, hide_columns, rows, pattern)`                  | column merge   | merge multiple columns into one display cell |
|   [7]   | `GT.cols_merge_range(col_begin, col_end, rows, sep, autohide, locale)` | range merge    | merge a numeric range pair into one cell     |

[ENTRYPOINT_SCOPE]: cell formatting
- rail: tables

| [INDEX] | [SURFACE]                                                                     | [ENTRY_FAMILY]    | [CAPABILITY]                                        |
| :-----: | :---------------------------------------------------------------------------- | :---------------- | :-------------------------------------------------- |
|   [1]   | `GT.fmt(fns, columns, rows, is_substitution)`                                 | custom format     | arbitrary format function applied to selected cells |
|   [2]   | `GT.fmt_number(columns, rows, decimals, …, locale)`                           | numeric format    | locale-aware number with separators and sign        |
|   [3]   | `GT.fmt_currency(columns, rows, currency, …, locale)`                         | currency format   | currency symbol, subunits, and locale placement     |
|   [4]   | `GT.fmt_percent(columns, rows, decimals, …, scale_values, locale)`            | percent format    | percentage with optional auto-scaling               |
|   [5]   | `GT.fmt_integer(columns, rows, use_seps, …, locale)`                          | integer format    | integer with separators, sign, and compaction       |
|   [6]   | `GT.fmt_scientific(columns, rows, decimals, …, exp_style, locale)`            | scientific format | `x10n` or E-notation with mantissa/exponent control |
|   [7]   | `GT.fmt_date(columns, rows, date_style, pattern, locale)`                     | date format       | ISO and locale date style tokens                    |
|   [8]   | `GT.fmt_datetime(columns, rows, date_style, time_style, format_str, …)`       | datetime format   | combined date-time with optional format string      |
|   [9]   | `GT.fmt_time(columns, rows, time_style, pattern, locale)`                     | time format       | ISO and locale time style tokens                    |
|  [10]   | `GT.fmt_image(columns, rows, height, width, sep, path, file_pattern, encode)` | image cells       | embed image paths or base64-encoded images          |
|  [11]   | `GT.fmt_nanoplot(columns, rows, plot_type, …, options)`                       | nanoplot cells    | inline SVG sparkline or bar chart per cell          |
|  [12]   | `GT.fmt_markdown(columns, rows)`                                              | markdown cells    | render Markdown in cell content                     |

[ENTRYPOINT_SCOPE]: styling, summary, and export
- rail: tables

| [INDEX] | [SURFACE]                                                        | [ENTRY_FAMILY]   | [CAPABILITY]                                                          |
| :-----: | :--------------------------------------------------------------- | :--------------- | :-------------------------------------------------------------------- |
|   [1]   | `GT.tab_style(style, locations)`                                 | cell style       | apply `style.*` objects to `loc.*` targets                            |
|   [2]   | `GT.data_color(columns, rows, palette, domain, na_color, …)`     | colour scale     | background colouring by data value with auto-text                     |
|   [3]   | `GT.summary_rows(fns, fmt, columns, groups, side, missing_text)` | group summary    | aggregate rows appended to each row group                             |
|   [4]   | `GT.grand_summary_rows(fns, fmt, columns, side, missing_text)`   | grand summary    | aggregate rows appended across all row groups                         |
|   [5]   | `GT.tab_options(**kwargs)`                                       | theme options    | global table, heading, column-labels, body, stub, footer theme policy |
|   [6]   | `GT.as_raw_html(inline_css, make_page, all_important)`           | HTML emit        | render the table as an HTML string                                    |
|   [7]   | `GT.save(file, selector, scale, expand, web_driver, …)`          | file export      | export to PNG/PDF/SVG via a headless Chrome driver                    |
|   [8]   | `GT.show(target)`                                                | interactive show | display in notebook or browser                                        |

## [4]-[IMPLEMENTATION_LAW]

[TABLES_TOPOLOGY]:
- namespace: `great_tables`; `GT` is the single fluent builder; all methods return `GT` (self) except terminal emitters `as_raw_html` and `save`
- location selectors: `loc.*` functions produce `Loc` objects passed to `tab_style` / `tab_footnote`; they do not mutate the table directly
- style objects: `style.text(…)` / `style.fill(…)` / `style.borders(…)` / `style.css(…)` are value objects passed as the `style` argument to `tab_style`
- summary rows: `fns` accepts a `dict[str, Callable]` mapping label to aggregation function; `fmt` optionally formats summary output using a `FormatFn`
- nanoplot: `fmt_nanoplot` requires list- or string-valued cells; `nanoplot_options()` produces the `options` dict for colour, size, and reference-line policy
- `vals` module: `great_tables.vals.fmt_*` functions apply the same format logic standalone outside a `GT` chain; useful for formatting without building a full table
- export: `save` requires a Chrome or compatible WebDriver; `as_raw_html(inline_css=True)` is the no-driver path for portable HTML

[LOCAL_ADMISSION]:
- one `GT(data)` owns the entire table; column selection uses `SelectExpr` (column name strings, selectors, or `None` for all).
- `tab_style(style, locations)` accepts a list of `CellStyle` objects and a list of `Loc` objects for multi-target application.
- `data_color` derives text colour automatically from the background luminance when `autocolor_text=True`; palette is a matplotlib-compatible palette name or explicit hex list.
- `save` delegates rendering to a headless browser; `as_raw_html` is preferred for server-side artefact generation without a browser.

[RAIL_LAW]:
- Package: `great-tables`
- Owns: fluent publication-quality table construction, cell formatting, structural layout, cell styling, and HTML/image export
- Accept: a Pandas/Polars DataFrame or dict-like as `GT(data)`; `loc.*` / `style.*` objects for structural and styling operations
- Reject: hand-rolled HTML table generation; manual CSS injection where `tab_style` / `tab_options` provides the surface
