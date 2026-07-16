# [PY_ARTIFACTS_API_WEASYPRINT]

`weasyprint` supplies an HTML/CSS-to-PDF rendering engine for the artifacts pdf rail: `HTML` is the document entry, `CSS` carries supplemental stylesheets, `HTML.write_pdf`/`HTML.render` own output, the `Document`/`Page` pair owns the rendered tree, `FontConfiguration`/`CounterStyle`/`URLFetcher` own font, counter-style, and resource policy, and the `write_pdf` `**options` family selects PDF/A and PDF/UA archival variants, forms, tagging, image optimization, and a `pydyf.PDF` finisher hook.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `weasyprint`
- package: `weasyprint`
- module: `weasyprint`
- installed: `69.0`
- license: `BSD-3-Clause`
- asset: pure-Python PDF generation via `pydyf` (release) — WeasyPrint 69 has no cairo dependency; PDF objects/streams are emitted directly. Text layout/shaping still uses native `pango`/`harfbuzz`/`fontconfig` and `pillow` for raster images, loaded via `pangocffi`/`pydyf` at render time
- runtime deps: `pydyf`, `cssselect2`, `tinycss2`, `tinyhtml5`, `fonttools[woff]`, `pyphen`, `pillow`, `cffi`
- rail: pdf

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: document input family
- rail: pdf — sources are mutually exclusive: `filename`, `url`, `file_obj`, or `string`
- type family: class

| [INDEX] | [SYMBOL]     | [ROLE]                                              |
| :-----: | :----------- | :-------------------------------------------------- |
|  [01]   | `HTML`       | parse an HTML source; entry to render and write PDF |
|  [02]   | `CSS`        | parse a supplemental stylesheet for rendering       |
|  [03]   | `Attachment` | file embedded into the output PDF                   |

[PUBLIC_TYPE_SCOPE]: rendered output family
- rail: pdf — `weasyprint.document`
- type family: class

| [INDEX] | [SYMBOL]   | [ROLE]                                           |
| :-----: | :--------- | :----------------------------------------------- |
|  [01]   | `Document` | laid-out tree of pages; writes or copies to PDF  |
|  [02]   | `Page`     | one rendered page with links, anchors, bookmarks |

[PUBLIC_TYPE_SCOPE]: `Document` members
- rail: pdf
- kind: attribute

| [INDEX] | [SYMBOL]         | [ROLE]                                                      |
| :-----: | :--------------- | :---------------------------------------------------------- |
|  [01]   | `pages`          | list of `Page` objects                                      |
|  [02]   | `metadata`       | `DocumentMetadata` (title/authors/description/dates/custom) |
|  [03]   | `url_fetcher`    | `URLFetcher` used for resource loading                      |
|  [04]   | `fonts`          | resolved `Font` set                                         |
|  [05]   | `color_profiles` | ICC color-profile registry resolved at render               |
|  [06]   | `output_intent`  | resolved ICC output-intent the PDF/A write embeds           |

[PUBLIC_TYPE_SCOPE]: `Page` members
- rail: pdf
- kind: attribute

| [INDEX] | [SYMBOL]    | [ROLE]                        |
| :-----: | :---------- | :---------------------------- |
|  [01]   | `width`     | page width in CSS pixels      |
|  [02]   | `height`    | page height in CSS pixels     |
|  [03]   | `bleed`     | bleed-box margins             |
|  [04]   | `bookmarks` | bookmark entries on this page |
|  [05]   | `links`     | hyperlink rectangles          |
|  [06]   | `anchors`   | named-anchor positions        |
|  [07]   | `forms`     | interactive form fields       |

[PUBLIC_TYPE_SCOPE]: `DocumentMetadata` members
- rail: pdf
- kind: attribute

| [INDEX] | [SYMBOL]             | [ROLE]                                          |
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

[PUBLIC_TYPE_SCOPE]: font, counter, and resource family
- rail: pdf — `weasyprint.text.fonts`, `weasyprint.css.counters`, `weasyprint.urls`

| [INDEX] | [SYMBOL]              | [TYPE_FAMILY] | [ROLE]                                                        |
| :-----: | :-------------------- | :------------ | :------------------------------------------------------------ |
|  [01]   | `FontConfiguration`   | class         | per-render `@font-face` registry; one instance per render     |
|  [02]   | `CounterStyle`        | class         | dict storing `@counter-style` rules for list/marker numbering |
|  [03]   | `URLFetcher`          | class         | configurable resource loader (timeout/ssl/headers/protocols)  |
|  [04]   | `URLFetcherResponse`  | class         | response wrapper a custom fetcher returns                     |
|  [05]   | `default_url_fetcher` | function      | default resource loader for URLs and data URIs                |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: document construction
- rail: pdf — pass exactly one source argument
- entry family: construct

Every source parser shares the mutually-exclusive input set `(guess=None, *, filename=None, url=None, file_obj=None, string=None, encoding=None, base_url=None)`; the `HTML`/`CSS`/`Attachment` rows carry only the arguments added past it, and `URLFetcher(timeout=10, ssl_context=None, http_headers=None, allowed_protocols=None, allow_redirects=True, fail_on_errors=False)` builds the fetcher a parser accepts as `url_fetcher=`.

| [INDEX] | [SURFACE]                                                                                             | [ROLE]                      |
| :-----: | :---------------------------------------------------------------------------------------------------- | :-------------------------- |
|  [01]   | `HTML(…, url_fetcher=default_url_fetcher, media_type='print')`                                        | parse an HTML source        |
|  [02]   | `CSS(…, url_fetcher=default_url_fetcher, font_config=None)`                                           | parse a stylesheet          |
|  [03]   | `Attachment(…, name=None, description=None, created=None, modified=None, relationship='Unspecified')` | declare an embedded file    |
|  [04]   | `FontConfiguration()`                                                                                 | per-render font registry    |
|  [05]   | `CounterStyle()`                                                                                      | `@counter-style` registry   |
|  [06]   | `URLFetcher(…)`                                                                                       | configured resource fetcher |

[ENTRYPOINT_SCOPE]: render and output
- rail: pdf — `weasyprint.HTML`, `weasyprint.document.Document`

A `write_pdf` returns PDF `bytes` when `target=None` and prepends `target=None, zoom=1, finisher=None`; the `HTML` render pair adds `font_config=None, counter_style=None, color_profiles=None` (elided `…` below) and every entry threads `**options`.

| [INDEX] | [SURFACE]                                                           | [ENTRY_FAMILY] | [ROLE]                               |
| :-----: | :------------------------------------------------------------------ | :------------- | :----------------------------------- |
|  [01]   | `HTML.write_pdf(target=None, zoom=1, finisher=None, …, **options)`  | render+write   | render and write PDF                 |
|  [02]   | `HTML.render(…, **options)`                                         | render         | produce a `Document` without writing |
|  [03]   | `Document.write_pdf(target=None, zoom=1, finisher=None, **options)` | write          | write a rendered `Document` to PDF   |
|  [04]   | `Document.copy(pages='all')`                                        | transform      | new `Document` from a page subset    |
|  [05]   | `Document.make_bookmark_tree(scale=1, transform_pages=False)`       | navigation     | build the PDF outline tree           |

[ENTRYPOINT_SCOPE]: PDF output `**options`
- rail: pdf — keyword options threaded through `write_pdf`/`render`

These keywords are the variant/forms/tagging/optimization policy. Archival profiles (`pdf/a-1b`..`pdf/a-4`, `pdf/ua-1`) set `pdf_variant`; `pdf_tags=True` adds the structure tree required by PDF/UA and tagged PDF/A.

| [INDEX] | [OPTION]                   | [ROLE]                                                                       |
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

[ENTRYPOINT_SCOPE]: CSS Paged Media running-content surface
- rail: pdf — WeasyPrint honors CSS Paged Media Level 3 + GCPM; these are CSS declarations in a `CSS(string=...)`/`stylesheets=` sheet the render consumes, not Python calls. This is the surface the `document/emit#DOCUMENT` V12 rebuild composes for running heads/feet, section-aware headers, and cross-reference page numbers — the static one-string `onPage` furniture dies here.

| [INDEX] | [SURFACE]                                          | [CAPABILITY]                                           |
| :-----: | :------------------------------------------------- | :----------------------------------------------------- |
|  [01]   | `@page` rule + page selectors                      | per-page geometry and spread-aware margins             |
|  [02]   | named pages                                        | assign an element run to a named page master           |
|  [03]   | `@page` margin boxes                               | running header/footer content in any margin slot       |
|  [04]   | `string-set` + `string()`                          | section-aware running heads captured off a heading     |
|  [05]   | `target-counter()` / `target-text()`               | cross-reference to a target's page number or text      |
|  [06]   | page counters                                      | current page number and total page count               |
|  [07]   | `leader()`                                         | dotted-leader fill between a label and its page number |
|  [08]   | CSS counters + `@counter-style`                    | figure/section auto-numbering and custom markers       |
|  [09]   | `bookmark-label`/`bookmark-level`/`bookmark-state` | drive the PDF outline tree from CSS                    |

- [01]-[PAGE_SELECTORS]: `@page { size: A4; margin: 2cm } @page :first {…} @page :left/:right/:blank {…}` — `:first`/`:left`/`:right`/`:blank` select the page class.
- [02]-[NAMED_PAGES]: `.chapter { page: chapter } @page chapter { … }` — per-section page masters, never a forked writer.
- [03]-[MARGIN_BOXES]: `@page { @top-center { content: … } @bottom-right { content: … } }` — the running-content furniture across the 16 boxes: 4 corners + `@top/bottom-left/center/right` + `@left/right-top/middle/bottom`.
- [04]-[RUNNING_HEADS]: `h1 { string-set: chaptitle content() } @page { @top-left { content: string(chaptitle, first) } }` — `first`/`start`/`last`/`first-except` picks the value valid for the page.
- [05]-[CROSS_REFERENCE]: `a::after { content: target-counter(attr(href url), page) }` / `content: target-text(attr(href url))` — live "see page N" / table-of-contents references.
- [06]-[PAGE_COUNTERS]: `content: counter(page)` / `counter(pages)` / `counter(page, lower-roman)` — current and total page count in any list-style numbering.
- [07]-[LEADER]: `content: target-text(...) leader('.') target-counter(...)` — dotted-leader fill between a TOC label and its page number.
- [08]-[COUNTER_STYLE]: `counter-reset` / `counter-increment` / `counters(name, sep)`; `@counter-style` (or the `CounterStyle` registry) — custom marker styles.
- [09]-[BOOKMARKS]: `h1 { bookmark-level: 1; bookmark-label: content() }` — the declarative peer of `Document.make_bookmark_tree`.

[ENTRYPOINT_SCOPE]: font, counter, and resource resolution
- rail: pdf — `weasyprint.text.fonts`, `weasyprint.urls`

| [INDEX] | [SURFACE]                                                        | [ENTRY_FAMILY] | [ROLE]                                            |
| :-----: | :--------------------------------------------------------------- | :------------- | :------------------------------------------------ |
|  [01]   | `FontConfiguration.add_font_face(rule_descriptors, url_fetcher)` | register       | register an `@font-face` rule                     |
|  [02]   | `CounterStyle` name -> rule assignment                           | register       | assign rules; resolve/render markers              |
|  [03]   | `URLFetcher.fetch(url, headers=None)`                            | fetch          | configured resource load; pass as `url_fetcher=`  |
|  [04]   | `default_url_fetcher(url, timeout=10, …)`                        | fetch          | load a URL or data URI resource                   |
|  [05]   | `finisher(document: Document, pdf: pydyf.PDF) -> None`           | post-process   | mutate the `pydyf.PDF` after layout, before write |

- [02]-[COUNTER_STYLE]: the `dict` subclass assigns `name -> @counter-style` rule, then `resolve_counter(values, previous_types)`, `render_value(counter_value, counter_name)`, and `render_marker(counter_name, counter_value)` resolve markers while `copy()` clones the registry.
- [04]-[DEFAULT_FETCHER]: `default_url_fetcher(url, timeout=10, ssl_context=None, http_headers=None, allowed_protocols=None)` is the module function; `allow_redirects`/`fail_on_errors` live on the `URLFetcher` class, not this function.

## [04]-[IMPLEMENTATION_LAW]

[WEASYPRINT_TOPOLOGY]:
- one-shot path: `HTML(...).write_pdf(target)` parses, lays out, and writes in a single call
- two-phase path: `HTML(...).render(font_config=...)` returns a `Document`, then `Document.write_pdf` or `Document.copy(pages=...)` for paged output
- input is mutually exclusive: pass exactly one of `filename`, `url`, `file_obj`, or `string`; `base_url` resolves relative resources
- `media_type='print'` selects print CSS; `media_type='screen'` selects screen styles
- supplemental styles are passed as `stylesheets=[CSS(...)]` in `**options`; `@font-face` rules route through `FontConfiguration`, custom list markers through `CounterStyle`
- variant axis: archival/accessible output is a `**options` policy — `pdf_variant` selects PDF/A (`pdf/a-1b`..`pdf/a-4`) or PDF/UA (`pdf/ua-1`), `pdf_tags` emits the structure tree, `pdf_forms` emits AcroForm fields, `output_intent` supplies the ICC profile PDF/A requires; never a parallel writer per profile
- pydyf backend: WeasyPrint 69 generates PDF directly through `pydyf` — there is no cairo dependency. The `finisher=` callback receives `(document, pydyf.PDF)` for post-layout object/stream mutation (custom annotations, page-piece metadata)
- native surface: text layout/shaping uses `pango`/`harfbuzz`/`fontconfig` and `pillow` for raster decode, loaded via cffi at render time; PDF assembly itself is pure Python
- paged-media surface: running heads/feet, section-aware headers, and cross-reference page numbers are CSS Paged Media (`@page` margin boxes) + GCPM (`string-set`/`string()`, `target-counter()`/`target-text()`, `leader()`) declarations in the supplied stylesheet — WeasyPrint resolves them at layout. The `document/emit#DOCUMENT` running-content furniture is authored as CSS in a `CSS(string=...)` sheet threaded through `stylesheets=`, never a per-page Python `onPage` callback or a hand-drawn header string; a section-aware header is `h1 { string-set: … content() }` echoed by `@top-center { content: string(…) }`, and a "see page N" reference is `target-counter(attr(href url), page)`

[LOCAL_ADMISSION]:
- Author running heads/feet, TOC page references, and section-aware headers as CSS Paged Media (`@page` margin boxes, `string-set`/`string()`, `target-counter()`/`leader()`) in a supplemental `CSS` sheet — never a per-page `finisher`/`onPage` callback drawing header text, and never a static one-string header baked at build time.
- Build with one source argument and an explicit `base_url`; never concatenate relative paths into the HTML string.
- Use `write_pdf(target=None)` to obtain PDF `bytes` for an in-memory pipeline; pass a path or file object only for direct file output.
- Share one `FontConfiguration` per render and register `@font-face` rules through it; do not mutate global font state.
- Use the two-phase `render` + `Document` path when paging, bookmark trees, or page-subset copies are needed.
- Select archival output through `pdf_variant`/`pdf_tags`/`output_intent` options, not a forked code path; for interactive forms set `pdf_forms=True` and let HTML controls drive the AcroForm.
- Inject custom PDF objects through a `finisher(document, pdf)` against the `pydyf.PDF`; never post-process the byte stream out of band.
- Construct a `URLFetcher` (or override `url_fetcher`) to sandbox or cache remote loads; `allowed_protocols` gates `http`/`https`/`data`.

[RAIL_LAW]:
- Package: `weasyprint`
- Owns: HTML/CSS-to-PDF rendering via `pydyf`, paged layout, font/counter resolution, bookmark/link/anchor/form extraction, embedded attachments, and PDF/A + PDF/UA archival/accessible variant emission
- Accept: a single HTML source with `base_url`; supplemental `CSS`; a per-render `FontConfiguration`/`CounterStyle`; archival output via `pdf_variant`/`pdf_tags`/`output_intent`; a `pydyf.PDF` `finisher`
- Reject: hand-rolled HTML-to-PDF conversion; parallel layout engines; a cairo-era backend assumption; driving the real low-level `Page.paint(stream, scale=1)` draw primitive directly when `write_pdf`/`render` already own the page-to-PDF path; wrapper-renames of `write_pdf`/`render`; forked code paths per PDF variant where a `**options` row suffices
