# [PY_ARTIFACTS_API_GREAT_TABLES]

`great_tables` supplies the publication-quality table rendering surface for the artifacts tables rail: a `GT` builder whose fluent method family owns column formatting (`fmt_number`, `fmt_currency`, `fmt_percent`, `fmt_scientific`, `fmt_engineering`, `fmt_partsper`, `fmt_units`, `fmt_duration`, `fmt_bytes`, `fmt_roman`, `fmt_date`, `fmt_datetime`, `fmt_time`, `fmt_tf`, `fmt_flag`, `fmt_icon`, `fmt_image`, `fmt_markdown`, `fmt_nanoplot`), structural layout (`tab_header`, `tab_spanner`, `tab_spanner_delim`, `tab_source_note`, `tab_footnote`, `tab_stub`, `tab_stubhead`), value substitution (`sub_missing`, `sub_zero`, `sub_small_vals`, `sub_large_vals`, `sub_values`), cell-text transformation (`text_transform`, `text_case_when`, `text_case_match`, `text_replace`), style injection (`tab_style`, `data_color`), column control (`cols_label`, `cols_label_with`, `cols_align`, `cols_hide`, `cols_unhide`, `cols_width`, `cols_move`, `cols_move_to_start`, `cols_move_to_end`, `cols_reorder`, `cols_merge`, `cols_merge_range`, `cols_merge_uncert`, `cols_merge_n_pct`, `cols_label_rotate`), summary-row construction (`summary_rows`, `grand_summary_rows`), row-group ordering (`row_group_order`), and theme identity (`opt_stylize`, `opt_row_striping`, `opt_table_font`, `opt_align_table_header`, `opt_all_caps`, `opt_vertical_padding`, `opt_horizontal_padding`, `opt_table_outline`, `opt_footnote_marks`, `opt_css`). The `loc` submodule supplies location selectors; the `style` submodule supplies `text`, `fill`, `borders`, and `css` cell-style constructors; the top-level `nanoplot_options`, `define_units`, `from_column`, `google_font`, `md`, `html`, `px`, and `pct` helpers configure plots, units, column-driven arguments, fonts, rich text, and dimensions; `as_raw_html`, `as_latex`, `write_raw_html`, and `save` emit the final artefact.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `great_tables`
- package: `great-tables`
- import: `great_tables`
- owner: `artifacts`
- rail: tables
- asset: runtime library
- surface: `0.22.0`

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: table builder
- rail: tables

| [INDEX] | [SYMBOL] | [PACKAGE_ROLE] | [CAPABILITY]                                                                                  |
| :-----: | :------- | :------------- | :-------------------------------------------------------------------------------------------- |
|  [01]   | `GT`     | table builder  | `(data, rowname_col, groupname_col, auto_align, id, locale)` — fluent table construction root |

[PUBLIC_TYPE_SCOPE]: location selectors (`great_tables.loc`)
- rail: tables

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
|  [14]   | `loc.summary`            | summary rows        | `(groups, columns, rows)` — select summary row cells           |
|  [15]   | `loc.summary_stub`       | summary stub cells  | `(rows)` — select summary row stub cells                       |
|  [16]   | `loc.grand_summary`      | grand summary rows  | `(columns, rows, mask)` — select grand summary row cells       |
|  [17]   | `loc.grand_summary_stub` | grand summary stub  | `(rows)` — select grand summary stub cells                     |

[PUBLIC_TYPE_SCOPE]: cell style constructors (`great_tables.style`)
- rail: tables

| [INDEX] | [SYMBOL]        | [PACKAGE_ROLE] | [CAPABILITY]                                                                                   |
| :-----: | :-------------- | :------------- | :--------------------------------------------------------------------------------------------- |
|  [01]   | `style.text`    | text style     | `(color, font, size, align, v_align, style, weight, stretch, decorate, transform, whitespace)` |
|  [02]   | `style.fill`    | fill style     | `(color)` — background color, accepts a `ColumnExpr` for data-driven fill                      |
|  [03]   | `style.borders` | border style   | `(sides, color, style, weight)`                                                                |
|  [04]   | `style.css`     | raw CSS        | `(rule)` — arbitrary CSS property injection                                                    |

[PUBLIC_TYPE_SCOPE]: top-level helpers (`great_tables`)
- rail: tables

| [INDEX] | [SYMBOL]           | [PACKAGE_ROLE]    | [CAPABILITY]                                                                  |
| :-----: | :----------------- | :---------------- | :---------------------------------------------------------------------------- |
|  [01]   | `nanoplot_options` | nanoplot config   | returns the `options` dict for `fmt_nanoplot` (point/line/bar/area/reference) |
|  [02]   | `define_units`     | unit grammar      | parses a units string into a renderable units object for `fmt_units`          |
|  [03]   | `from_column`      | column-driven arg | binds a styling/format argument to a column's per-row values                  |
|  [04]   | `google_font`      | font import       | a Google Font reference for `opt_table_font(font=...)`                        |
|  [05]   | `md` / `html`      | rich text markers | wrap a string as Markdown or raw HTML for labels, notes, and footnotes        |
|  [06]   | `px` / `pct`       | dimension helpers | pixel and percent dimension values for widths and sizes                       |
|  [07]   | `system_fonts`     | font stack        | a named system font stack for `opt_table_font(stack=...)`                     |
|  [08]   | `vals`             | standalone format | `vals.fmt_*` apply format logic outside a `GT` chain                          |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: construction and structure
- rail: tables

| [INDEX] | [SURFACE]                                                              | [ENTRY_FAMILY] | [CAPABILITY]                                         |
| :-----: | :--------------------------------------------------------------------- | :------------- | :--------------------------------------------------- |
|  [01]   | `GT(data, rowname_col, groupname_col, auto_align, id, locale)`         | builder init   | create a GT from a Polars/Pandas DataFrame or dict   |
|  [02]   | `GT.tab_header(title, subtitle, preheader)`                            | header         | set title, subtitle, and optional preheader          |
|  [03]   | `GT.tab_stub(rowname_col, groupname_col)`                              | stub           | set row name and group name columns                  |
|  [04]   | `GT.tab_stubhead(label)`                                               | stubhead       | label the stub column header                         |
|  [05]   | `GT.tab_spanner(label, columns, spanners, level, id, gather, replace)` | column spanner | group columns under a spanning label                 |
|  [06]   | `GT.tab_spanner_delim(delim, columns, split, limit, reverse)`          | spanner split  | derive spanners by splitting column names on a delim |
|  [07]   | `GT.tab_source_note(source_note)`                                      | source note    | append a source note to the footer                   |
|  [08]   | `GT.tab_footnote(footnote, locations, placement)`                      | footnote       | attach a footnote to one or more cell locations      |

[ENTRYPOINT_SCOPE]: column operations
- rail: tables

| [INDEX] | [SURFACE]                                                              | [ENTRY_FAMILY] | [CAPABILITY]                                              |
| :-----: | :--------------------------------------------------------------------- | :------------- | :-------------------------------------------------------- |
|  [01]   | `GT.cols_label(cases, **kwargs)`                                       | column rename  | set display labels for columns                            |
|  [02]   | `GT.cols_label_with(columns, fn)`                                      | label function | derive each label from the column id                      |
|  [03]   | `GT.cols_label_rotate(columns, dir, align, padding)`                   | label rotate   | render rotated column labels                              |
|  [04]   | `GT.cols_align(align, columns)`                                        | alignment      | `'left'` / `'center'` / `'right'`                         |
|  [05]   | `GT.cols_move(columns, after)`                                         | column reorder | move columns after a named column                         |
|  [06]   | `GT.cols_move_to_start(columns)` / `GT.cols_move_to_end(columns)`      | column anchor  | move columns to the start or end                          |
|  [07]   | `GT.cols_reorder(columns)`                                             | column order   | reorder to the given full column sequence                 |
|  [08]   | `GT.cols_hide(columns)` / `GT.cols_unhide(columns)`                    | column hide    | suppress or restore selected columns                      |
|  [09]   | `GT.cols_width(cases, **kwargs)`                                       | column width   | set explicit pixel/percent widths                         |
|  [10]   | `GT.cols_merge(columns, hide_columns, rows, pattern)`                  | column merge   | merge multiple columns into one display cell              |
|  [11]   | `GT.cols_merge_range(col_begin, col_end, rows, sep, autohide, locale)` | range merge    | merge a numeric range pair (`sep` defaults `None`)        |
|  [12]   | `GT.cols_merge_uncert(col_val, col_uncert, rows, sep, autohide)`       | uncert merge   | merge a value/uncertainty pair (`sep` defaults `" +/- "`) |
|  [13]   | `GT.cols_merge_n_pct(col_n, col_pct, rows, autohide)`                  | n-pct merge    | merge a count and percentage into one cell                |

[ENTRYPOINT_SCOPE]: cell formatting and substitution
- rail: tables

| [INDEX] | [SURFACE]                                                                                                                                      | [ENTRY_FAMILY]     | [CAPABILITY]                                                                                                      |
| :-----: | :--------------------------------------------------------------------------------------------------------------------------------------------- | :----------------- | :---------------------------------------------------------------------------------------------------------------- |
|  [01]   | `GT.fmt(fns, columns, rows, is_substitution)`                                                                                                  | custom format      | arbitrary format function over selected cells                                                                     |
|  [02]   | `GT.fmt_number(columns, rows, decimals, n_sigfig, compact, …, locale)`                                                                         | numeric format     | locale-aware number with separators, sign, compaction                                                             |
|  [03]   | `GT.fmt_integer(columns, rows, use_seps, compact, …, locale)`                                                                                  | integer format     | integer with separators, sign, and compaction                                                                     |
|  [04]   | `GT.fmt_currency(columns, rows, currency, use_subunits, …, locale)`                                                                            | currency format    | currency symbol, subunits, and locale placement                                                                   |
|  [05]   | `GT.fmt_percent(columns, rows, decimals, scale_values, …, locale)`                                                                             | percent format     | percentage with optional auto-scaling                                                                             |
|  [06]   | `GT.fmt_scientific(columns, rows, decimals, exp_style, …, locale)`                                                                             | scientific format  | `x10n` or E-notation with mantissa/exponent control                                                               |
|  [07]   | `GT.fmt_engineering(columns, rows, decimals, n_sigfig, exp_style, …)`                                                                          | engineering format | power-of-three exponent notation                                                                                  |
|  [08]   | `GT.fmt_partsper(columns, rows, to_units, symbol, decimals, …, locale)`                                                                        | parts-per format   | per-mille/per-myriad/ppm/ppb/ppt scaled notation                                                                  |
|  [09]   | `GT.fmt_units(columns, rows, pattern)`                                                                                                         | units format       | render a units-notation string with super/subscripts                                                              |
|  [10]   | `GT.fmt_duration(columns, rows, input_units, output_units, duration_style, …)`                                                                 | duration format    | render an elapsed duration in chosen units                                                                        |
|  [11]   | `GT.fmt_bytes(columns, rows, standard, decimals, incl_space, …, locale)`                                                                       | bytes format       | decimal or binary byte units                                                                                      |
|  [12]   | `GT.fmt_roman(columns, rows, case, pattern)`                                                                                                   | roman format       | render integers as Roman numerals                                                                                 |
|  [13]   | `GT.fmt_date(columns, rows, date_style, pattern, locale)`                                                                                      | date format        | ISO and locale date style tokens                                                                                  |
|  [14]   | `GT.fmt_datetime(columns, rows, date_style, time_style, format_str, …)`                                                                        | datetime format    | combined date-time with optional format string                                                                    |
|  [15]   | `GT.fmt_time(columns, rows, time_style, pattern, locale)`                                                                                      | time format        | ISO and locale time style tokens                                                                                  |
|  [16]   | `GT.fmt_tf(columns, rows, tf_style, true_val, false_val, na_val, colors)`                                                                      | boolean format     | render booleans as labels or symbols with colors                                                                  |
|  [17]   | `GT.fmt_flag(columns, rows, height, sep, use_title)`                                                                                           | flag cells         | render country-code flags                                                                                         |
|  [18]   | `GT.fmt_icon(columns, rows, height, sep, stroke_color, fill_color, …)`                                                                         | icon cells         | render Font Awesome icons                                                                                         |
|  [19]   | `GT.fmt_image(columns, rows, height, width, sep, path, file_pattern, encode)`                                                                  | image cells        | embed image paths or base64-encoded images                                                                        |
|  [20]   | `GT.fmt_markdown(columns, rows)`                                                                                                               | markdown cells     | render Markdown in cell content                                                                                   |
|  [21]   | `GT.fmt_nanoplot(columns, rows, plot_type, plot_height, missing_vals, autoscale, reference_line, reference_area, expand_x, expand_y, options)` | nanoplot cells     | inline SVG sparkline/bar chart per cell (`missing_vals` default `"gap"`, `expand_x`/`expand_y` are `list[float]`) |
|  [22]   | `GT.sub_missing(columns, rows, missing_text)`                                                                                                  | missing sub        | substitute a glyph for missing values                                                                             |
|  [23]   | `GT.sub_zero(columns, rows, zero_text)`                                                                                                        | zero sub           | substitute text for zeros (`zero_text` default `"nil"`)                                                           |
|  [24]   | `GT.sub_small_vals(columns, rows, threshold, small_pattern, sign)`                                                                             | small sub          | substitute a pattern for small magnitudes (`threshold` `0.01`)                                                    |
|  [25]   | `GT.sub_large_vals(columns, rows, threshold, large_pattern, sign)`                                                                             | large sub          | substitute a pattern for large magnitudes (`threshold` `1e12`)                                                    |
|  [26]   | `GT.sub_values(columns, rows, values, pattern, fn, replacement)`                                                                               | value sub          | substitute by literal values or a predicate `fn`                                                                  |

[ENTRYPOINT_SCOPE]: cell-text transform, style, color, summary, and export
- rail: tables

| [INDEX] | [SURFACE]                                                                                           | [ENTRY_FAMILY]   | [CAPABILITY]                                                       |
| :-----: | :-------------------------------------------------------------------------------------------------- | :--------------- | :----------------------------------------------------------------- |
|  [01]   | `GT.text_transform(locations, fn)`                                                                  | text transform   | apply a function to already-rendered cell text                     |
|  [02]   | `GT.text_case_when(*cases, default, locations)`                                                     | text case-when   | replace text by `(predicate, replacement)` cases                   |
|  [03]   | `GT.text_case_match(*cases, default, replace, locations)`                                           | text case-match  | replace text by `(match, replacement)` cases                       |
|  [04]   | `GT.text_replace(pattern, replacement, locations)`                                                  | text replace     | regex replace inside rendered cell text                            |
|  [05]   | `GT.tab_style(style, locations)`                                                                    | cell style       | apply `style.*` objects to `loc.*` targets                         |
|  [06]   | `GT.data_color(columns, rows, palette, domain, na_color, alpha, reverse, autocolor_text, truncate)` | colour scale     | background colouring by value with auto-text contrast              |
|  [07]   | `GT.summary_rows(*, fns, fmt, columns, groups, side, missing_text)`                                 | group summary    | aggregate rows appended to each row group (`fns` keyword-only)     |
|  [08]   | `GT.grand_summary_rows(*, fns, fmt, columns, side, missing_text)`                                   | grand summary    | aggregate rows appended across all row groups (`fns` keyword-only) |
|  [09]   | `GT.row_group_order(groups)`                                                                        | group order      | set the explicit row-group display order                           |
|  [10]   | `GT.opt_stylize(style, color, add_row_striping)`                                                    | theme preset     | apply one of six numbered style presets with a base color          |
|  [11]   | `GT.opt_row_striping(row_striping)`                                                                 | striping         | toggle zebra row striping                                          |
|  [12]   | `GT.opt_table_font(font, stack, weight, style, add)`                                                | table font       | set the font family or a named `FontStackName` stack               |
|  [13]   | `GT.opt_align_table_header(align)`                                                                  | header align     | align the title/subtitle block                                     |
|  [14]   | `GT.opt_all_caps(all_caps, locations)`                                                              | all-caps         | uppercase column labels, stub, and row-group text                  |
|  [15]   | `GT.opt_vertical_padding(scale)` / `GT.opt_horizontal_padding(scale)`                               | padding          | scale cell padding                                                 |
|  [16]   | `GT.opt_table_outline(style, width, color)`                                                         | outline          | draw a table outline                                               |
|  [17]   | `GT.opt_footnote_marks(marks)`                                                                      | footnote marks   | choose the footnote mark sequence (`numbers`, letters, symbols)    |
|  [18]   | `GT.opt_css(css, add, allow_duplicates)`                                                            | raw CSS          | inject raw CSS at the table level                                  |
|  [19]   | `GT.tab_options(**kwargs)`                                                                          | theme options    | the low-level per-keyword theme surface beneath the `opt_*` rows   |
|  [20]   | `GT.as_raw_html(inline_css, make_page, all_important)`                                              | HTML emit        | render the table as an HTML string                                 |
|  [21]   | `GT.as_latex(use_longtable, tbl_pos)`                                                               | LaTeX emit       | render the table as a LaTeX string                                 |
|  [22]   | `GT.write_raw_html(filename, encoding, inline_css, …)`                                              | HTML file        | write the HTML rendering to a file                                 |
|  [23]   | `GT.save(file, selector, scale, expand, web_driver, window_size, …)`                                | file export      | export to PNG/PDF/SVG via a headless Chrome driver                 |
|  [24]   | `GT.show(target)`                                                                                   | interactive show | display in notebook or browser                                     |
|  [25]   | `GT.pipe(func, *args, **kwargs)`                                                                    | pipe             | thread the `GT` through a `GT -> GT` function                      |
|  [26]   | `GT.with_id(id)` / `GT.with_locale(locale)`                                                         | identity         | set the table id or locale after construction                      |

## [04]-[IMPLEMENTATION_LAW]

[TABLES_TOPOLOGY]:
- namespace: `great_tables`; `GT` is the single fluent builder; every method returns `GTSelf` (a copied `GT`) except the terminal emitters `as_raw_html`, `as_latex`, `write_raw_html`, and `save`
- location selectors: `loc.*` functions produce `Loc` objects passed to `tab_style` / `tab_footnote`; they do not mutate the table directly. Their argument shape is per-selector — `loc.body` / `loc.grand_summary` take `(columns, rows, mask)`, `loc.summary` takes `(groups, columns, rows)`, `loc.column_labels` takes `(columns)` only, `loc.stub` / `loc.row_group` / `loc.row_groups` / `loc.summary_stub` / `loc.grand_summary_stub` take `(rows)` only, `loc.spanner_labels` takes `(ids)`, and `loc.stubhead` / `loc.column_header` / `loc.header` / `loc.footer` / `loc.title` / `loc.subtitle` / `loc.source_notes` take no arguments
- mask targeting: `loc.body(mask=<polars predicate>)` is mutually exclusive with `columns`/`rows`; a predicate-targeted location passes the `mask` alone
- style objects: `style.text(…)` / `style.fill(color)` / `style.borders(sides, color, style, weight)` / `style.css(rule)` are value objects passed as the `style` argument to `tab_style`; `style.fill` and `style.text` accept a `ColumnExpr` (from `from_column`) for data-driven values
- summary rows: `summary_rows` / `grand_summary_rows` take `fns` keyword-only as `dict[str, PlExpr]` (a polars expression naming its own target column) or `dict[str, Callable]`; `columns` is rejected (`NotImplementedError`) in 0.22.0, so each expression carries its own column
- nanoplot: `fmt_nanoplot` requires list- or string-valued cells; `nanoplot_options()` produces the `options` dict for point radius, line/bar/area fill, reference line/area color, vertical guides, and the `y_val_fmt_fn` / `y_axis_fmt_fn` / `y_ref_line_fmt_fn` / `currency` value formatting
- `vals` module: `great_tables.vals.fmt_*` apply the same format logic standalone outside a `GT` chain
- export: `save` requires a Chrome or compatible WebDriver; `as_raw_html(inline_css=True)` and `as_latex` are the no-driver paths for portable HTML and LaTeX

[LOCAL_ADMISSION]:
- one `GT(data)` owns the entire table; column selection uses `SelectExpr` (column name strings, selectors, or `None` for all), row selection uses `RowSelectExpr`.
- `tab_style(style, locations)` accepts a `CellStyle` or list of them and a `Loc` or list of them for multi-target application.
- `data_color` derives text colour automatically from the background luminance when `autocolor_text=True`; `palette` is a matplotlib-compatible palette-name string or an explicit hex list, `domain` is a `list[str] | list[int] | list[float]`.
- `save` delegates rendering to a headless browser; `as_raw_html` is preferred for server-side artefact generation without a browser.

[RAIL_LAW]:
- Package: `great-tables`
- Owns: fluent publication-quality table construction, cell formatting, value substitution, cell-text transformation, structural layout, cell styling, theme identity, summary rows, and HTML/LaTeX/image export
- Accept: a Pandas/Polars DataFrame or dict-like as `GT(data)`; `loc.*` / `style.*` objects for structural and styling operations; the polars `DataFrame.style` accessor returns a real `GT`
- Reject: hand-rolled HTML table generation; manual CSS injection where `tab_style` / `opt_*` / `opt_css` / `tab_options` provides the surface
