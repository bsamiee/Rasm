# [PY_ARTIFACTS_API_WEASYPRINT]

`weasyprint` owns pure-Python HTML/CSS-to-PDF rendering for the artifacts pdf rail: one `HTML` source lays out through `pydyf` into a paged `Document`/`Page` tree, and one `write_pdf` `**options` policy selects every archival, tagged, forms, and font variant. Supplemental CSS Paged Media and GCPM own running heads/feet and cross-references, a per-render `FontConfiguration`/`CounterStyle` owns `@font-face` and `@counter-style`, and a `finisher(document, pydyf.PDF)` hook owns post-layout injection. It never re-parses PDF, forks a writer per variant, or draws page furniture from Python.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `weasyprint`
- package: `weasyprint` (`BSD-3-Clause`, Kozea/CourtBouillon)
- module: `weasyprint`
- namespaces: `weasyprint`, `weasyprint.document`, `weasyprint.text.fonts`, `weasyprint.css.counters`, `weasyprint.urls`
- asset: pure-Python PDF assembly through `pydyf`; text layout/shaping binds native `pango`/`harfbuzz`/`fontconfig` via `cffi` and `pillow` for raster decode at render time
- depends: `pydyf`, `cssselect2`, `tinycss2`, `tinyhtml5`, `fonttools[woff]`, `pyphen`, `pillow`, `cffi`
- rail: pdf

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: document input classes; one source of `filename`/`url`/`file_obj`/`string`

| [INDEX] | [SYMBOL]     | [CAPABILITY]                                        |
| :-----: | :----------- | :-------------------------------------------------- |
|  [01]   | `HTML`       | parse an HTML source; entry to render and write PDF |
|  [02]   | `CSS`        | parse a supplemental stylesheet for rendering       |
|  [03]   | `Attachment` | file embedded into the output PDF                   |

[PUBLIC_TYPE_SCOPE]: rendered output classes (`weasyprint.document`)

| [INDEX] | [SYMBOL]   | [CAPABILITY]                                     |
| :-----: | :--------- | :----------------------------------------------- |
|  [01]   | `Document` | laid-out tree of pages; writes or copies to PDF  |
|  [02]   | `Page`     | one rendered page with links, anchors, bookmarks |

[PUBLIC_TYPE_SCOPE]: `Document` attributes

| [INDEX] | [SYMBOL]         | [CAPABILITY]                                                |
| :-----: | :--------------- | :---------------------------------------------------------- |
|  [01]   | `pages`          | list of `Page` objects                                      |
|  [02]   | `metadata`       | `DocumentMetadata` (title/authors/description/dates/custom) |
|  [03]   | `url_fetcher`    | `URLFetcher` used for resource loading                      |
|  [04]   | `fonts`          | resolved `Font` set                                         |
|  [05]   | `color_profiles` | ICC color-profile registry resolved at render               |
|  [06]   | `output_intent`  | resolved ICC output-intent the PDF/A write embeds           |

[PUBLIC_TYPE_SCOPE]: `Page` attributes

| [INDEX] | [SYMBOL]    | [CAPABILITY]                  |
| :-----: | :---------- | :---------------------------- |
|  [01]   | `width`     | page width in CSS pixels      |
|  [02]   | `height`    | page height in CSS pixels     |
|  [03]   | `bleed`     | bleed-box margins             |
|  [04]   | `bookmarks` | bookmark entries on this page |
|  [05]   | `links`     | hyperlink rectangles          |
|  [06]   | `anchors`   | named-anchor positions        |
|  [07]   | `forms`     | interactive form fields       |

[PUBLIC_TYPE_SCOPE]: `DocumentMetadata` attributes

| [INDEX] | [SYMBOL]             | [CAPABILITY]                                    |
| :-----: | :------------------- | :---------------------------------------------- |
|  [01]   | `title`              | document title (from `<title>`/`@title`)        |
|  [02]   | `authors`            | author list                                     |
|  [03]   | `description`        | document description                            |
|  [04]   | `keywords`           | keyword list                                    |
|  [05]   | `generator`          | generating-tool string                          |
|  [06]   | `created`/`modified` | ISO timestamps                                  |
|  [07]   | `lang`               | document language                               |
|  [08]   | `attachments`        | document-level `Attachment` list                |
|  [09]   | `custom`             | custom metadata dict (-> PDF `custom_metadata`) |

[PUBLIC_TYPE_SCOPE]: font, counter, and resource policy (`weasyprint.text.fonts`, `weasyprint.css.counters`, `weasyprint.urls`)

| [INDEX] | [SYMBOL]              | [TYPE_FAMILY] | [CAPABILITY]                                                  |
| :-----: | :-------------------- | :------------ | :------------------------------------------------------------ |
|  [01]   | `FontConfiguration`   | class         | per-render `@font-face` registry; one instance per render     |
|  [02]   | `CounterStyle`        | class         | dict storing `@counter-style` rules for list/marker numbering |
|  [03]   | `URLFetcher`          | class         | configurable resource loader (timeout/ssl/headers/protocols)  |
|  [04]   | `URLFetcherResponse`  | class         | response wrapper a custom fetcher returns                     |
|  [05]   | `default_url_fetcher` | function      | default resource loader for URLs and data URIs                |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: document construction; pass exactly one source argument

Every source parser shares `(guess=None, *, filename=None, url=None, file_obj=None, string=None, encoding=None, base_url=None)`; rows carry only the arguments added past it. `URLFetcher(timeout=10, ssl_context=None, http_headers=None, allowed_protocols=None, allow_redirects=True, fail_on_errors=False)` builds the `url_fetcher=` a parser accepts.

| [INDEX] | [SURFACE]                                                                                             | [CAPABILITY]                |
| :-----: | :---------------------------------------------------------------------------------------------------- | :-------------------------- |
|  [01]   | `HTML(…, url_fetcher=default_url_fetcher, media_type='print')`                                        | parse an HTML source        |
|  [02]   | `CSS(…, url_fetcher=default_url_fetcher, font_config=None)`                                           | parse a stylesheet          |
|  [03]   | `Attachment(…, name=None, description=None, created=None, modified=None, relationship='Unspecified')` | declare an embedded file    |
|  [04]   | `FontConfiguration()`                                                                                 | per-render font registry    |
|  [05]   | `CounterStyle()`                                                                                      | `@counter-style` registry   |
|  [06]   | `URLFetcher(…)`                                                                                       | configured resource fetcher |

[ENTRYPOINT_SCOPE]: render and output (`weasyprint.HTML`, `weasyprint.document.Document`)

`write_pdf` returns PDF `bytes` when `target=None`; the `HTML` render pair adds `font_config=None, counter_style=None, color_profiles=None` (elided `…`) and every entry threads `**options`.

| [INDEX] | [SURFACE]                                                           | [SHAPE]      | [CAPABILITY]                         |
| :-----: | :------------------------------------------------------------------ | :----------- | :----------------------------------- |
|  [01]   | `HTML.write_pdf(target=None, zoom=1, finisher=None, …, **options)`  | render+write | render and write PDF                 |
|  [02]   | `HTML.render(…, **options)`                                         | render       | produce a `Document` without writing |
|  [03]   | `Document.write_pdf(target=None, zoom=1, finisher=None, **options)` | write        | write a rendered `Document` to PDF   |
|  [04]   | `Document.copy(pages='all')`                                        | transform    | new `Document` from a page subset    |
|  [05]   | `Document.make_bookmark_tree(scale=1, transform_pages=False)`       | navigation   | build the PDF outline tree           |

[ENTRYPOINT_SCOPE]: PDF output `**options` threaded through `write_pdf`/`render`

Archival profiles set `pdf_variant`; `pdf_tags=True` adds the structure tree PDF/UA and tagged PDF/A require.

| [INDEX] | [OPTION]                   | [CAPABILITY]                                                                 |
| :-----: | :------------------------- | :--------------------------------------------------------------------------- |
|  [01]   | `stylesheets`              | list of `CSS`/path/url supplemental stylesheets                              |
|  [02]   | `pdf_variant`              | archival profile (`pdf/a-1b`, `pdf/a-2b`, `pdf/a-3b`, `pdf/a-4`, `pdf/ua-1`) |
|  [03]   | `pdf_version`              | target PDF version string (e.g. `'1.7'`, `'2.0'`)                            |
|  [04]   | `pdf_identifier`           | explicit PDF file identifier bytes                                           |
|  [05]   | `pdf_forms`                | emit interactive AcroForm fields from HTML form controls                     |
|  [06]   | `pdf_tags`                 | emit the tagged-PDF structure tree (PDF/UA, accessibility)                   |
|  [07]   | `uncompressed_pdf`         | skip stream compression (debug/inspectable output)                           |
|  [08]   | `custom_metadata`          | merge `DocumentMetadata.custom` into the PDF info/XMP                        |
|  [09]   | `xmp_metadata`             | explicit XMP metadata packet                                                 |
|  [10]   | `attachments`              | extra `Attachment` list embedded in the PDF                                  |
|  [11]   | `attachment_relationships` | per-attachment AF relationship hints                                         |
|  [12]   | `presentational_hints`     | honor legacy HTML presentational attributes                                  |
|  [13]   | `optimize_images`          | recompress embedded raster images                                            |
|  [14]   | `jpeg_quality`             | JPEG re-encode quality (0-95) when optimizing                                |
|  [15]   | `dpi`                      | raster image resampling resolution                                           |
|  [16]   | `full_fonts`               | embed complete fonts instead of subsetting                                   |
|  [17]   | `hinting`                  | keep TrueType hinting in embedded fonts                                      |
|  [18]   | `output_intent`            | ICC output-intent profile for color-managed PDF/A                            |
|  [19]   | `cache`                    | shared image/render cache dict across documents                              |

[ENTRYPOINT_SCOPE]: CSS Paged Media Level 3 + GCPM running-content, authored in a `CSS(string=...)`/`stylesheets=` sheet the layout consumes — CSS declarations, not Python calls
- [01]-[PAGE_SELECTORS]: `@page { size: A4; margin: 2cm } @page :first/:left/:right/:blank {…}` — per-page geometry and spread-aware margins keyed on page class.
- [02]-[NAMED_PAGES]: `.chapter { page: chapter } @page chapter { … }` — per-section page masters, one writer.
- [03]-[MARGIN_BOXES]: `@page { @top-center { content: … } @bottom-right { content: … } }` — running header/footer content across the 16 margin boxes (4 corners, `@top/bottom-left/center/right`, `@left/right-top/middle/bottom`).
- [04]-[RUNNING_HEADS]: `h1 { string-set: chaptitle content() } @page { @top-left { content: string(chaptitle, first) } }` — section-aware heads; `first`/`start`/`last`/`first-except` picks the value valid for the page.
- [05]-[CROSS_REFERENCE]: `a::after { content: target-counter(attr(href url), page) }` / `target-text(attr(href url))` — live "see page N" and table-of-contents references.
- [06]-[PAGE_COUNTERS]: `content: counter(page)` / `counter(pages)` / `counter(page, lower-roman)` — current and total page count in any list-style numbering.
- [07]-[LEADER]: `content: target-text(...) leader('.') target-counter(...)` — dotted-leader fill between a TOC label and its page number.
- [08]-[COUNTER_STYLE]: `counter-reset`/`counter-increment`/`counters(name, sep)` and `@counter-style` (or the `CounterStyle` registry) — figure/section auto-numbering and custom markers.
- [09]-[BOOKMARKS]: `h1 { bookmark-level: 1; bookmark-label: content() }` — declarative peer of `Document.make_bookmark_tree` driving the PDF outline.

[ENTRYPOINT_SCOPE]: font, counter, and resource resolution (`weasyprint.text.fonts`, `weasyprint.urls`)

| [INDEX] | [SURFACE]                                                        | [SHAPE]      | [CAPABILITY]                                      |
| :-----: | :--------------------------------------------------------------- | :----------- | :------------------------------------------------ |
|  [01]   | `FontConfiguration.add_font_face(rule_descriptors, url_fetcher)` | register     | register an `@font-face` rule                     |
|  [02]   | `CounterStyle` name -> rule assignment                           | register     | assign rules; resolve/render markers              |
|  [03]   | `URLFetcher.fetch(url, headers=None)`                            | fetch        | configured resource load; pass as `url_fetcher=`  |
|  [04]   | `default_url_fetcher(url, timeout=10, …)`                        | fetch        | load a URL or data URI resource                   |
|  [05]   | `finisher(document: Document, pdf: pydyf.PDF) -> None`           | post-process | mutate the `pydyf.PDF` after layout, before write |

- [02]-[COUNTER_STYLE]: `CounterStyle`, a `dict` subclass, assigns `name -> @counter-style` rules, then `resolve_counter(values, previous_types)`, `render_value(counter_value, counter_name)`, and `render_marker(counter_name, counter_value)` resolve markers while `copy()` clones the registry.
- [04]-[DEFAULT_FETCHER]: `default_url_fetcher(url, timeout=10, ssl_context=None, http_headers=None, allowed_protocols=None)` is the module function; `allow_redirects`/`fail_on_errors` live on the `URLFetcher` class, not this function.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- One-shot `HTML(...).write_pdf(target)` parses, lays out, and writes in a single call; the two-phase `HTML(...).render(font_config=...)` returns a `Document` that `Document.write_pdf` or `Document.copy(pages=...)` finishes for paged output.
- Input is mutually exclusive: exactly one of `filename`, `url`, `file_obj`, `string`, with `base_url` resolving relative resources; `media_type='print'`/`'screen'` selects the CSS media.
- Supplemental styles pass as `stylesheets=[CSS(...)]`; `@font-face` routes through `FontConfiguration` and custom markers through `CounterStyle`.
- Archival and accessible output is one `**options` policy: `pdf_variant` selects PDF/A or PDF/UA, `pdf_tags` emits the structure tree, `pdf_forms` emits AcroForm fields, `output_intent` supplies the ICC profile PDF/A requires — one writer, never a code path per profile.
- PDF assembly is pure Python through `pydyf`; `finisher(document, pydyf.PDF)` mutates objects and streams after layout for custom annotations and page-piece metadata, and text layout/shaping runs on native `pango`/`harfbuzz`/`fontconfig` via `cffi`.
- Running heads/feet, cross-reference page numbers, and bookmark trees resolve from CSS Paged Media + GCPM declarations at layout, never a Python per-page callback.

[STACKING]:
- `drawsvg`(`.api/drawsvg.md`): `svg_as_utf8_data_uri(drawing.as_svg())` yields a `data:` URI the HTML source embeds inline as an `<img>` or CSS background, threading a figure artifact into the PDF without a temp file.
- `ziamath`(`.api/ziamath.md`): an equation SVG threads into the same HTML at a `config.svg2`-matched profile so the page and its embedded math emit one consistent SVG version.
- `fonttools`(`.api/fonttools.md`) + `pyphen`(`.api/pyphen.md`): the render consumes `fonttools[woff]` subset/re-flavor for `@font-face` embedding and `pyphen` for CSS `hyphens: auto` soft-hyphenation — both its declared runtime substrate.
- substrate rail: the render boundary offloads the GIL-bound `pango`/`pydyf` work through `anyio`(`.api/anyio.md`) `to_thread.run_sync`, records one kind-discriminated `ArtifactReceipt` via `msgspec`(`.api/msgspec.md`), spans through `structlog`(`.api/structlog.md`) + `opentelemetry`, and lifts a parse/layout fault onto `expression`(`.api/expression.md`) `Result`.
- `document/emit#DOCUMENT`: composes this surface — the `DocumentNode` tree lowers to HTML+CSS, running heads/feet author as `@page` margin-box CSS, and a "see page N" reference authors as `target-counter(attr(href url), page)`.

[LOCAL_ADMISSION]:
- Build with one source argument and an explicit `base_url`; never concatenate relative paths into the HTML string.
- Use `write_pdf(target=None)` for an in-memory `bytes` pipeline; pass a path or file object only for direct file output.
- Share one `FontConfiguration` per render and register `@font-face` through it; take the two-phase `render` + `Document` path when paging, bookmark trees, or page-subset copies are needed.
- Author running heads/feet, TOC page references, and section-aware headers as CSS Paged Media in a supplemental `CSS` sheet; inject custom PDF objects through a `finisher(document, pdf)`, never post-processing the byte stream out of band.
- Select archival output through `pdf_variant`/`pdf_tags`/`output_intent`; construct a `URLFetcher` (or override `url_fetcher`) to sandbox or cache remote loads, `allowed_protocols` gating `http`/`https`/`data`.

[RAIL_LAW]:
- Package: `weasyprint`
- Owns: HTML/CSS-to-PDF rendering via `pydyf`, paged layout, font/counter resolution, bookmark/link/anchor/form extraction, embedded attachments, and PDF/A + PDF/UA archival and accessible variant emission
- Accept: a single HTML source with `base_url`; supplemental `CSS`; a per-render `FontConfiguration`/`CounterStyle`; archival output via `pdf_variant`/`pdf_tags`/`output_intent`; a `pydyf.PDF` `finisher`
- Reject: hand-rolled HTML-to-PDF conversion; a parallel layout engine; driving the low-level `Page.paint(stream, scale=1)` primitive directly where `write_pdf`/`render` own the page-to-PDF path; a wrapper-rename of `write_pdf`/`render`; a forked code path per PDF variant where a `**options` row suffices
