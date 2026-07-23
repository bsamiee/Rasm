# [PY_ARTIFACTS_API_GREAT_TABLES]

`great_tables` mints the publication-quality table surface for the artifacts tables rail: one fluent `GT` builder folds locale-aware cell formatting, structural layout, value substitution, cell styling, data-driven colour, summary rows, and theme identity, then emits HTML, LaTeX, or a raster image. Its `loc` and `style` submodules supply the location selectors and cell-style value objects every targeted operation consumes; the top-level helpers configure plots, units, fonts, and footnote marks, and `vals.fmt_*` formats a series outside any `GT` chain.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `great_tables`
- package: `great-tables` (MIT)
- module: `great_tables`
- namespaces: `great_tables.loc`, `great_tables.style`, `great_tables.vals`
- rail: tables

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: table builder

| [INDEX] | [SYMBOL] | [PACKAGE_ROLE] | [CAPABILITY]                                                    |
| :-----: | :------- | :------------- | :-------------------------------------------------------------- |
|  [01]   | `GT`     | table builder  | fluent construction root; every transform returns a copied `GT` |

[PUBLIC_TYPE_SCOPE]: location selectors (`great_tables.loc`)

| [INDEX] | [SYMBOL]                 | [PACKAGE_ROLE]      | [CAPABILITY]                                                   |
| :-----: | :----------------------- | :------------------ | :------------------------------------------------------------- |
|  [01]   | `loc.body`               | body cells          | `(columns, rows, mask)` — select data cells by column/row/mask |
|  [02]   | `loc.column_labels`      | column label cells  | `(columns)` — select column label header cells                 |
|  [03]   | `loc.column_header`      | column header       | `()` — entire column header area                               |
|  [04]   | `loc.spanner_labels`     | spanner label cells | `(ids)` — select spanner label cells by id                     |
|  [05]   | `loc.stub`               | row stub cells      | `(rows)` — select stub (row label) cells                       |
|  [06]   | `loc.stubhead`           | stubhead cell       | `()` — select the stubhead label cell                          |
|  [07]   | `loc.row_group`          | row group label     | `(rows)` — select row group label cells                        |
|  [08]   | `loc.row_groups`         | row group region    | `(rows)` — select whole row-group regions                      |
|  [09]   | `loc.header`             | header region       | `()` — full header area                                        |
|  [10]   | `loc.title`              | title cell          | `()` — the table title cell                                    |
|  [11]   | `loc.subtitle`           | subtitle cell       | `()` — the subtitle cell                                       |
|  [12]   | `loc.footer`             | footer region       | `()` — full footer area                                        |
|  [13]   | `loc.source_notes`       | source note cells   | `()` — select source note cells                                |
|  [14]   | `loc.grand_summary`      | grand summary rows  | `(columns, rows, mask)` — select grand summary row cells       |
|  [15]   | `loc.grand_summary_stub` | grand summary stub  | `(rows)` — select grand summary stub cells                     |
|  [16]   | `loc.summary`            | group summary rows  | `(groups, columns, rows)` — select group-summary row cells     |
|  [17]   | `loc.summary_stub`       | group summary stub  | `(groups, rows)` — select group-summary stub cells             |

[PUBLIC_TYPE_SCOPE]: cell style constructors (`great_tables.style`)

| [INDEX] | [SYMBOL]        | [PACKAGE_ROLE] | [CAPABILITY]                                                                                   |
| :-----: | :-------------- | :------------- | :--------------------------------------------------------------------------------------------- |
|  [01]   | `style.text`    | text style     | `(color, font, size, align, v_align, style, weight, stretch, decorate, transform, whitespace)` |
|  [02]   | `style.fill`    | fill style     | `(color)` — background color, accepts a `FromColumn` for data-driven fill                      |
|  [03]   | `style.borders` | border style   | `(sides, color, style, weight)`                                                                |
|  [04]   | `style.css`     | raw CSS        | `(rule)` — arbitrary CSS property injection                                                    |

[PUBLIC_TYPE_SCOPE]: top-level helpers (`great_tables`)

| [INDEX] | [SYMBOL]              | [PACKAGE_ROLE]    | [CAPABILITY]                                                                             |
| :-----: | :-------------------- | :---------------- | :--------------------------------------------------------------------------------------- |
|  [01]   | `nanoplot_options`    | nanoplot config   | the `options` dict for `fmt_nanoplot` (point/line/bar/area/reference)                    |
|  [02]   | `define_units`        | unit grammar      | parse a units string into a renderable units object for `fmt_units`                      |
|  [03]   | `from_column`         | column-driven arg | bind a styling/format argument to a column's per-row values                              |
|  [04]   | `google_font`         | font import       | a Google Font reference for `opt_table_font(font=...)`                                   |
|  [05]   | `md` / `html`         | rich text markers | wrap a string as Markdown (`Md`) or raw HTML (`Html`) for labels/rich text               |
|  [06]   | `px` / `pct`          | dimension helpers | pixel and percent dimension values for widths and sizes                                  |
|  [07]   | `system_fonts`        | font stack        | `system_fonts(name)` → a `FontStackName` stack for `opt_table_font(stack=...)`           |
|  [08]   | `vals`                | standalone format | `vals.fmt_*` format a `pl.Series`/list outside a `GT` chain → `list[str]` (roster below) |
|  [09]   | `LETTERS` / `letters` | mark alphabets    | `LETTERS()`/`letters()` → uppercase/lowercase A–Z `list[str]` for `opt_footnote_marks`   |
|  [10]   | `random_id`           | id minter         | `random_id(n=10) -> str` — a table id for `GT(id=...)` / `with_id`                       |
|  [11]   | `quarto`              | quarto bridge     | `quarto.is_quarto_render() -> bool` → gate `tab_options(quarto_disable_processing=...)`  |

- [08]-[VALS_ROSTER]: `vals` carries `fmt_number` `fmt_integer` `fmt_currency` `fmt_percent` `fmt_scientific` `fmt_engineering` `fmt_partsper` `fmt_bytes` `fmt_roman` `fmt_date` `fmt_time` `fmt_duration` `fmt_image` `fmt_markdown`; `fmt_units`/`fmt_datetime`/`fmt_tf`/`fmt_flag`/`fmt_icon`/`fmt_nanoplot` stay `GT`-only.

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: construction and structure

| [INDEX] | [SURFACE]                                                              | [ENTRY_FAMILY] | [CAPABILITY]                                |
| :-----: | :--------------------------------------------------------------------- | :------------- | :------------------------------------------ |
|  [01]   | `GT(data, rowname_col, groupname_col, auto_align, id, locale)`         | builder init   | GT from a Polars/Pandas frame or dict       |
|  [02]   | `GT.tab_header(title, subtitle, preheader)`                            | header         | set title, subtitle, and optional preheader |
|  [03]   | `GT.tab_stub(rowname_col, groupname_col)`                              | stub           | set row name and group name columns         |
|  [04]   | `GT.tab_stubhead(label)`                                               | stubhead       | label the stub column header                |
|  [05]   | `GT.tab_spanner(label, columns, spanners, level, id, gather, replace)` | column spanner | group columns under a spanning label        |
|  [06]   | `GT.tab_spanner_delim(delim, columns, split, limit, reverse)`          | spanner split  | derive spanners by splitting column names   |
|  [07]   | `GT.tab_source_note(source_note)`                                      | source note    | append a source note to the footer          |
|  [08]   | `GT.tab_footnote(footnote, locations, placement)`                      | footnote       | auto-marked footnote at `loc.*` targets     |

- [08]-[FOOTNOTE]: `placement` default `'auto'` ∈ `{'auto','left','right'}`.

[ENTRYPOINT_SCOPE]: column operations

| [INDEX] | [SURFACE]                                                              | [ENTRY_FAMILY]    | [CAPABILITY]                              |
| :-----: | :--------------------------------------------------------------------- | :---------------- | :---------------------------------------- |
|  [01]   | `GT.cols_label(cases, **kwargs)`                                       | column rename     | set display labels for columns            |
|  [02]   | `GT.cols_label_with(columns, fn)`                                      | label function    | derive each label from the column id      |
|  [03]   | `GT.cols_label_rotate(columns, dir, align, padding)`                   | label rotate      | render rotated column labels              |
|  [04]   | `GT.cols_align(align, columns)`                                        | alignment         | `'left'` / `'center'` / `'right'`         |
|  [05]   | `GT.cols_move(columns, after)`                                         | column reorder    | move columns after a named column         |
|  [06]   | `GT.cols_move_to_start(columns)`                                       | column anchor     | move columns to the start                 |
|  [07]   | `GT.cols_move_to_end(columns)`                                         | column anchor     | move columns to the end                   |
|  [08]   | `GT.cols_hide(columns)` / `GT.cols_unhide(columns)`                    | column hide       | suppress or restore selected columns      |
|  [09]   | `GT.cols_width(cases, **kwargs)`                                       | column width      | set explicit pixel/percent widths         |
|  [10]   | `GT.cols_reorder(columns)`                                             | column reorder    | reorder all columns to the given order    |
|  [11]   | `GT.cols_merge(columns, hide_columns, rows, pattern)`                  | column merge      | merge columns into one cell via `pattern` |
|  [12]   | `GT.cols_merge_range(col_begin, col_end, rows, sep, autohide, locale)` | range merge       | merge a begin/end pair into a dash range  |
|  [13]   | `GT.cols_merge_uncert(col_val, col_uncert, rows, sep, autohide)`       | uncertainty merge | merge a value/uncertainty column pair     |
|  [14]   | `GT.cols_merge_n_pct(col_n, col_pct, rows, autohide)`                  | n-pct merge       | merge a count/percent column pair         |

[ENTRYPOINT_SCOPE]: cell formatting and substitution
- every `fmt_*`/`sub_*` carries the `(columns, rows)` selector prefix; each cell shows the format-specific tail

| [INDEX] | [SURFACE]                                                       | [ENTRY_FAMILY]     | [CAPABILITY]                                      |
| :-----: | :-------------------------------------------------------------- | :----------------- | :------------------------------------------------ |
|  [01]   | `GT.fmt(fns, columns, rows, is_substitution)`                   | custom format      | arbitrary format function over selected cells     |
|  [02]   | `GT.fmt_number(decimals, n_sigfig, compact, …, locale)`         | numeric format     | number with separators, sign, compaction          |
|  [03]   | `GT.fmt_integer(use_seps, compact, …, locale)`                  | integer format     | integer with separators, sign, and compaction     |
|  [04]   | `GT.fmt_currency(currency, use_subunits, …, locale)`            | currency format    | currency symbol, subunits, locale placement       |
|  [05]   | `GT.fmt_percent(decimals, scale_values, …, locale)`             | percent format     | percentage with optional auto-scaling             |
|  [06]   | `GT.fmt_scientific(decimals, exp_style, …, locale)`             | scientific format  | `x10n`/E-notation, mantissa+exponent control      |
|  [07]   | `GT.fmt_engineering(decimals, n_sigfig, exp_style, …)`          | engineering format | power-of-three exponent notation                  |
|  [08]   | `GT.fmt_units(pattern)`                                         | units format       | units-notation with super/subscripts              |
|  [09]   | `GT.fmt_bytes(standard, decimals, incl_space, …, locale)`       | bytes format       | decimal or binary byte units                      |
|  [10]   | `GT.fmt_roman(case, pattern)`                                   | roman format       | render integers as Roman numerals                 |
|  [11]   | `GT.fmt_date(date_style, pattern, locale)`                      | date format        | ISO and locale date style tokens                  |
|  [12]   | `GT.fmt_datetime(date_style, time_style, format_str, sep, …)`   | datetime format    | combined date-time with optional `format_str`     |
|  [13]   | `GT.fmt_time(time_style, pattern, locale)`                      | time format        | ISO and locale time style tokens                  |
|  [14]   | `GT.fmt_tf(tf_style, pattern, true_val, false_val, …)`          | boolean format     | render booleans as labels or symbols with colors  |
|  [15]   | `GT.fmt_flag(height, sep, use_title)`                           | flag cells         | render country-code flags                         |
|  [16]   | `GT.fmt_icon(height, sep, stroke_color, fill_color, …)`         | icon cells         | render Font Awesome icons                         |
|  [17]   | `GT.fmt_image(height, width, sep, path, …)`                     | image cells        | embed image paths or base64-encoded images        |
|  [18]   | `GT.fmt_markdown()`                                             | markdown cells     | render Markdown in cell content                   |
|  [19]   | `GT.fmt_nanoplot(plot_type, plot_height, missing_vals, …)`      | nanoplot cells     | inline SVG sparkline/bar chart per cell           |
|  [20]   | `GT.sub_missing(missing_text)`                                  | missing sub        | substitute a glyph for missing values             |
|  [21]   | `GT.sub_zero(zero_text)`                                        | zero sub           | substitute text for zeros                         |
|  [22]   | `GT.sub_small_vals(threshold, small_pattern, sign)`             | small sub          | substitute a pattern for values below `threshold` |
|  [23]   | `GT.sub_large_vals(threshold, large_pattern, sign)`             | large sub          | substitute a pattern for values above `threshold` |
|  [24]   | `GT.sub_values(values, pattern, fn, replacement)`               | value sub          | substitute by value, predicate, or pattern        |
|  [25]   | `GT.fmt_partsper(to_units, symbol, scale_values, …, locale)`    | parts-per format   | per-mille/ppm/ppb/ppt notation                    |
|  [26]   | `GT.fmt_duration(input_units, output_units, duration_style, …)` | duration format    | humanized time duration across unit fields        |

- [19]-[NANOPLOT]: `missing_vals` default `"gap"` ∈ `{"marker","gap","zero","remove"}`; `expand_x`/`expand_y` are `list[int | float]`.
- [25]-[PARTSPER]: `to_units` default `'per-mille'` ∈ `{'per-mille','per-myriad','pcm','ppm','ppb','ppt'}`.

[ENTRYPOINT_SCOPE]: cell-text transform, style, color, summary, and export

| [INDEX] | [SURFACE]                                                     | [ENTRY_FAMILY]   | [CAPABILITY]                                          |
| :-----: | :------------------------------------------------------------ | :--------------- | :---------------------------------------------------- |
|  [01]   | `GT.tab_style(style, locations)`                              | cell style       | apply `style.*` objects to `loc.*` targets            |
|  [02]   | `GT.data_color(palette, domain, na_color, autocolor_text, …)` | colour scale     | value-driven colouring with auto-text contrast        |
|  [03]   | `GT.grand_summary_rows(*, fns, fmt, columns, side, …)`        | grand summary    | aggregate rows across all row groups                  |
|  [04]   | `GT.row_group_order(groups)`                                  | group order      | set the explicit row-group display order              |
|  [05]   | `GT.opt_stylize(style, color, add_row_striping)`              | theme preset     | six presets (`1`-`6`) + base color                    |
|  [06]   | `GT.opt_row_striping(row_striping)`                           | striping         | toggle zebra row striping                             |
|  [07]   | `GT.opt_table_font(font, stack, weight, style, add)`          | table font       | set the font family or a named `FontStackName` stack  |
|  [08]   | `GT.opt_align_table_header(align)`                            | header align     | align the title/subtitle block                        |
|  [09]   | `GT.opt_all_caps(all_caps, locations)`                        | all-caps         | uppercase column labels, stub, row-group text         |
|  [10]   | `GT.opt_vertical_padding(scale)`                              | padding          | scale vertical cell padding                           |
|  [11]   | `GT.opt_horizontal_padding(scale)`                            | padding          | scale horizontal cell padding                         |
|  [12]   | `GT.opt_table_outline(style, width, color)`                   | outline          | draw a table outline                                  |
|  [13]   | `GT.opt_footnote_marks(marks)`                                | footnote marks   | mark sequence: `'numbers'`, `letters()`, or symbols   |
|  [14]   | `GT.opt_css(css, add, allow_duplicates)`                      | raw CSS          | inject raw CSS at the table level                     |
|  [15]   | `GT.tab_options(**kwargs)`                                    | theme options    | low-level per-keyword theme surface beneath `opt_*`   |
|  [16]   | `GT.render(context) -> str`                                   | HTML render      | core HTML renderer keyed by `context` (`'html'`)      |
|  [17]   | `GT.as_raw_html(inline_css, make_page, …) -> str`             | HTML emit        | HTML string; `inline_css` for email-safe markup       |
|  [18]   | `GT.as_latex(use_longtable, tbl_pos) -> str`                  | LaTeX emit       | render the table as a LaTeX string                    |
|  [19]   | `GT.write_raw_html(filename, inline_css, …) -> None`          | HTML file        | write the HTML rendering to a file                    |
|  [20]   | `GT.save(file, web_driver, window_size, …) -> GTSelf`         | file export      | PNG/PDF/SVG via a headless WebDriver                  |
|  [21]   | `GT.show(target)`                                             | interactive show | notebook or browser (`'notebook'`/`'browser'`)        |
|  [22]   | `GT.pipe(func, *args, **kwargs)`                              | pipe             | thread the `GT` through a `GT -> GT` function         |
|  [23]   | `GT.from_data(data, rowname_col, groupname_col, …)`           | functional ctor  | function-style constructor mirroring `GT(...)`        |
|  [24]   | `GT.with_id(id)` / `GT.with_locale(locale)`                   | identity         | set the table id or locale after construction         |
|  [25]   | `GT.summary_rows(*, fns, fmt, columns, groups, …)`            | group summary    | aggregate rows per row-group (the `groups` axis)      |
|  [26]   | `GT.text_transform(locations, fn)`                            | cell transform   | apply a `str -> str` fn to cell text at `loc.*`       |
|  [27]   | `GT.text_case_when(*cases, default, locations)`               | cell transform   | rewrite cell text by `(predicate, replacement)` cases |
|  [28]   | `GT.text_case_match(*cases, default, replace, locations)`     | cell transform   | rewrite by `(match, replacement)` cases               |
|  [29]   | `GT.text_replace(pattern, replacement, locations)`            | cell transform   | substring/regex replacement over cell text            |
|  [30]   | `GT.gtsave(file, selector, expand, zoom, delay, …) -> GT`     | static image     | static-image saver; distinct kwargs from `save`       |

- [SUMMARY]: `fns` is keyword-only (`dict[str, PlExpr | Callable]`); a non-`None` `columns` raises `NotImplementedError`.
- [20]-[SAVE]: `web_driver` default `'chrome'` ∈ chrome/safari/firefox/edge or a `webdriver.Remote`.
- [28]-[TEXT_CASE_MATCH]: `match` is `str | list[str]`, `replace` ∈ `{'all','partial'}`.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `GT` is the single fluent builder; every transform returns a copied `GT` (`GTSelf`), so a chain never mutates in place. Terminal emitters split by return: `render`/`as_raw_html`/`as_latex` -> `str`, `write_raw_html` -> `None`, `save` -> `GTSelf`, `show` displays.
- `loc.*` produce `Loc` objects and `style.*` produce `CellStyle` objects; both are values consumed by `tab_style`/`tab_footnote`/`text_*` and never mutate the table. `loc.body(mask=<polars predicate>)` excludes `columns`/`rows` — a predicate-targeted location carries the `mask` alone.
- `summary_rows` (group-scoped, `groups` axis) and `grand_summary_rows` share the keyword-only `fns` (`dict[str, PlExpr]` or `dict[str, Callable[[TblData], Any]]`); the dict key is the row label and the expression names its target columns, so `pl.col("a","b").sum()` fills every named column on one row. `grand_summary_rows` accepts a polars frame, but group-scoped `summary_rows` faults on polars input (`IndexError` inside the group resolver), so a per-group subtotal takes pandas-backed data.
- `fmt_nanoplot` requires list- or string-valued cells; `nanoplot_options()` builds its `options` dict (point radius, line/bar/area fill, reference line/area, vertical guides, `y_val_fmt_fn`/`y_axis_fmt_fn`/`currency` value formatting).
- `render(context='html')`, `as_raw_html`, and `as_latex` are the no-driver paths; `save` alone drives a headless browser.

[STACKING]:
- `polars`(`.api/polars.md`): `DataFrame.style` returns a real `great_tables.gt.GT`, so a polars pipeline ends `.style` then chains `fmt_*`/`tab_*` directly with no DataFrame-to-table marshalling.
- `from_column(col, na_value, fn)` binds a `style.fill`/`style.text` colour or a format argument to a column's per-row values, so one `tab_style` is data-driven across rows without a Python loop; `data_color` is the higher-level value→colour scale with `autocolor_text` contrast.
- `weasyprint`(`.api/weasyprint.md`): `as_raw_html(inline_css=True)` produces email/print-safe markup weasyprint prints to PDF; `jinja2`(`.api/jinja2.md`) embeds the same fragment via `{{ table_html }}` under an autoescape-off marker, and `pymupdf`(`.api/pymupdf.md`)/`pikepdf`(`.api/pikepdf.md`) stitch it into a document. `as_latex` feeds a Typst/LaTeX rail; `save` is the sole browser-dependent path, reserved for raster previews.
- `nanoplot_options()` returns the plain `dict[str, Any]` passed as `fmt_nanoplot(options=...)`, so plot styling is data, not a parallel object; `define_units` parses a units string once for reuse across `fmt_units`/`cols_label`.

[LOCAL_ADMISSION]:
- one `GT(data)` owns the entire table; column selection uses `SelectExpr` (name strings, selectors, or `None` for all), row selection uses `RowSelectExpr`, and `tab_style` accepts a `CellStyle` or list and a `Loc` or list for multi-target application.
- `data_color` derives text colour from background luminance under `autocolor_text=True`; `palette` is a matplotlib-compatible palette name or a hex list, `domain` a `list[str] | list[int] | list[float]`.
- `as_raw_html` is the server-side artefact path; `save` is reserved for raster previews where a headless browser is available.

[RAIL_LAW]:
- Package: `great-tables`
- Owns: fluent publication-quality table construction, locale-aware cell formatting, value substitution, structural layout, cell styling, data-driven colouring, theme identity, group and grand summary rows, and HTML/LaTeX/image export
- Accept: a Pandas/Polars DataFrame or dict-like as `GT(data)`; `loc.*`/`style.*` objects for structural and styling operations; the polars `DataFrame.style` accessor returning a real `GT`
- Reject: hand-rolled HTML table generation; manual CSS where `tab_style`/`opt_*`/`opt_css`/`tab_options` owns the surface; hand-rolled value substitution, column merging, cell-text transformation, or footnote marking where `sub_*`/`cols_merge_*`/`text_*`/`tab_footnote` owns the operation
