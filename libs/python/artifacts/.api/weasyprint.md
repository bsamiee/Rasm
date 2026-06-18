# [PY_ARTIFACTS_API_WEASYPRINT]

`weasyprint` supplies an HTML/CSS-to-PDF rendering engine for the artifacts pdf rail: `HTML` is the document entry, `CSS` carries supplemental stylesheets, `HTML.write_pdf` and `HTML.render` own output, the `Document`/`Page` pair owns the rendered tree, and `FontConfiguration` plus `default_url_fetcher` own font resolution and resource loading.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `weasyprint`
- package: `weasyprint`
- module: `weasyprint`
- asset: Python library over native `pango`/`gobject`/`cairo` (cffi FFI)
- rail: pdf

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: document input family
- rail: pdf — sources are mutually exclusive: `filename`, `url`, `file_obj`, or `string`
- type family: class

| [INDEX] | [SYMBOL]     | [ROLE]                                              |
| :-----: | :----------- | :-------------------------------------------------- |
|   [1]   | `HTML`       | parse an HTML source; entry to render and write PDF |
|   [2]   | `CSS`        | parse a supplemental stylesheet for rendering       |
|   [3]   | `Attachment` | file embedded into the output PDF                   |

[PUBLIC_TYPE_SCOPE]: rendered output family
- rail: pdf — `weasyprint.document`
- type family: class

| [INDEX] | [SYMBOL]   | [ROLE]                                           |
| :-----: | :--------- | :----------------------------------------------- |
|   [1]   | `Document` | laid-out tree of pages; writes or copies to PDF  |
|   [2]   | `Page`     | one rendered page with links, anchors, bookmarks |

[PUBLIC_TYPE_SCOPE]: `Document` members
- rail: pdf
- kind: attribute

| [INDEX] | [SYMBOL]      | [ROLE]                                    |
| :-----: | :------------ | :---------------------------------------- |
|   [1]   | `pages`       | list of `Page` objects                    |
|   [2]   | `metadata`    | document metadata (title, authors, dates) |
|   [3]   | `url_fetcher` | resource-loading callable                 |
|   [4]   | `fonts`       | resolved font set                         |
|   [5]   | `font_config` | `FontConfiguration` used for layout       |

[PUBLIC_TYPE_SCOPE]: `Page` members
- rail: pdf
- kind: attribute

| [INDEX] | [SYMBOL]    | [ROLE]                        |
| :-----: | :---------- | :---------------------------- |
|   [1]   | `width`     | page width in CSS pixels      |
|   [2]   | `height`    | page height in CSS pixels     |
|   [3]   | `bleed`     | bleed-box margins             |
|   [4]   | `bookmarks` | bookmark entries on this page |
|   [5]   | `links`     | hyperlink rectangles          |
|   [6]   | `anchors`   | named-anchor positions        |
|   [7]   | `forms`     | interactive form fields       |

[PUBLIC_TYPE_SCOPE]: font and resource family
- rail: pdf — `weasyprint.text.fonts`, `weasyprint.urls`

| [INDEX] | [SYMBOL]              | [TYPE_FAMILY] | [ROLE]                                         |
| :-----: | :-------------------- | :------------ | :--------------------------------------------- |
|   [1]   | `FontConfiguration`   | class         | per-render font face registry                  |
|   [2]   | `default_url_fetcher` | function      | default resource loader for URLs and data URIs |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: document construction
- rail: pdf — pass exactly one source argument
- entry family: construct

| [INDEX] | [SURFACE]                                                                                                                                                              | [ROLE]                                |
| :-----: | :--------------------------------------------------------------------------------------------------------------------------------------------------------------------- | :------------------------------------ |
|   [1]   | `HTML(guess=None, filename=None, url=None, file_obj=None, string=None, encoding=None, base_url=None, url_fetcher=None, media_type='print')`                            | parse an HTML document                |
|   [2]   | `CSS(guess=None, filename=None, url=None, file_obj=None, string=None, base_url=None, url_fetcher=None, media_type='print', font_config=None, counter_style=None, ...)` | parse a stylesheet                    |
|   [3]   | `Attachment(guess=None, filename=None, url=None, file_obj=None, string=None, name=None, description=None, relationship='Unspecified')`                                 | declare an embedded file              |
|   [4]   | `FontConfiguration()`                                                                                                                                                  | create a font registry for one render |

[ENTRYPOINT_SCOPE]: render and output
- rail: pdf — `weasyprint.HTML`, `weasyprint.document.Document`

| [INDEX] | [SURFACE]                                                                                                                  | [ENTRY_FAMILY] | [ROLE]                                        |
| :-----: | :------------------------------------------------------------------------------------------------------------------------- | :------------- | :-------------------------------------------- |
|   [1]   | `HTML.write_pdf(target=None, zoom=1, finisher=None, font_config=None, counter_style=None, color_profiles=None, **options)` | render+write   | render and write PDF (bytes if `target=None`) |
|   [2]   | `HTML.render(font_config=None, counter_style=None, color_profiles=None, **options)`                                        | render         | produce a `Document` without writing          |
|   [3]   | `Document.write_pdf(target=None, zoom=1, finisher=None, **options)`                                                        | write          | write a rendered `Document` to PDF            |
|   [4]   | `Document.copy(pages='all')`                                                                                               | transform      | new `Document` from a page subset             |
|   [5]   | `Document.make_bookmark_tree(scale=1, transform_pages=False)`                                                              | navigation     | build the PDF outline tree                    |
|   [6]   | `Page.paint(stream, scale=1)`                                                                                              | render         | paint one page to a drawing stream            |

[ENTRYPOINT_SCOPE]: font and resource resolution
- rail: pdf — `weasyprint.text.fonts`, `weasyprint.urls`

| [INDEX] | [SURFACE]                                                                                           | [ENTRY_FAMILY] | [ROLE]                          |
| :-----: | :-------------------------------------------------------------------------------------------------- | :------------- | :------------------------------ |
|   [1]   | `FontConfiguration.add_font_face(rule_descriptors, url_fetcher)`                                    | register       | register an `@font-face` rule   |
|   [2]   | `default_url_fetcher(url, timeout=10, ssl_context=None, http_headers=None, allowed_protocols=None)` | fetch          | load a URL or data URI resource |

## [4]-[IMPLEMENTATION_LAW]

[WEASYPRINT_TOPOLOGY]:
- one-shot path: `HTML(...).write_pdf(target)` parses, lays out, and writes in a single call
- two-phase path: `HTML(...).render(font_config=...)` returns a `Document`, then `Document.write_pdf` or `Document.copy(pages=...)` for paged output
- input is mutually exclusive: pass exactly one of `filename`, `url`, `file_obj`, or `string`; `base_url` resolves relative resources
- `media_type='print'` selects print CSS; `media_type='screen'` selects screen styles
- supplemental styles are passed as `stylesheets=[CSS(...)]` in `**options`; `@font-face` rules route through `FontConfiguration`
- requires native `libpango`/`libgobject`/`libcairo`; the engine loads them via cffi at import time

[LOCAL_ADMISSION]:
- Build with one source argument and an explicit `base_url`; never concatenate relative paths into the HTML string.
- Use `write_pdf(target=None)` to obtain PDF bytes for an in-memory pipeline; pass a path or file object only for direct file output.
- Share one `FontConfiguration` per render and register `@font-face` rules through it; do not mutate global font state.
- Use the two-phase `render` + `Document` path when paging, bookmark trees, or page-subset copies are needed.
- Override `url_fetcher` to sandbox or cache remote resource loading; the default fetcher allows `http`/`https`/`data` by configuration.

[RAIL_LAW]:
- Package: `weasyprint`
- Owns: HTML/CSS-to-PDF rendering, paged layout, font resolution, bookmark/link/anchor extraction, and embedded attachments
- Accept: a single HTML source with `base_url`; supplemental `CSS` stylesheets; a per-render `FontConfiguration`
- Reject: hand-rolled HTML-to-PDF conversion; parallel layout engines; wrapper-renames of `write_pdf`/`render`
