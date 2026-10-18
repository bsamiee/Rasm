# [PRODUCTION]

Prepare source content and output through native objects, then use `execute` with `Script` for changes. Use [scripting](indesign://scripting/reference) for native discovery, [layout](indesign://layout/reference) for composition, and [graphics](indesign://graphics/reference) for color and placed artwork.

## [01]-[LINKS]

To locate missing or changed assets, inspect `Document.links` for `status`, `linkResourceURI`, and `parent`. Follow `Link.parent` to linked text, graphics, or media, then follow container references to locate placement. One source can have independent placements; retain native identities instead of matching filenames or coordinates.

Use `Link.update` to refresh changed content or `relink` to choose another source. Use `unlink` when content must become embedded and `unembed` when an external link is required. For SharedContent links, `goToSource` opens source content in InDesign. Inspect `linkedStoryOptions` and `linkedPageItemOptions` before updating local edits: appearance, frame content, size, and interactivity have separate preservation settings. Native linked placement establishes source relationships; duplication alone does not.

## [02]-[PREFLIGHT]

For a requested report, read [preflight](indesign://preflight), then follow `appliedProfile` to inspect enabled rules and configured thresholds in `ruleDataObjects`. Retain profile references when document and application profiles share names. Working and embedded profiles can differ. Inspect `preflightOptions` for profile policy, page scope, layers, pasteboard inclusion, and nonprinting objects before changing evaluation settings.

Interpret `aggregatedResults` as report hierarchy with page labels, descriptions, and labeled details. `parentNodeID` identifies report ancestry, not a page item; `processResults` supplies report text. Follow native `Link.status` and `Story.overflows` to objects that can be edited. Page labels and error names alone cannot establish identity.

When a script creates a process, call `waitForProcess()` before reading results or calling `saveReport`, then release that process with `remove`. Read actual `appliedProfile` instead of assuming a name selects document policy. For feedback during editing, configure native live preflight through document `preflightOptions.preflightOff` and inspect `activeProcess`. Status bar and panel feedback remain separate from a requested report. Check Document accessibility and copy checks require their own installed menu actions or native members.

## [03]-[STRUCTURED_CONTENT]

For repeated layouts populated from records, select or update a source through `Document.dataMergeProperties`, then read `dataMergeFields`. Create native text, image, or QR-code placeholders from field references. Literal field labels in story text are not placeholders. Set records and page arrangement through `dataMergePreferences`; set placement, empty-line handling, and output limits through `Document.dataMergeOptions`.

Use `mergeRecords` for editable merged documents or `DataMerge.exportFile` for direct PDF output that reuses static page content. Request native overset and missing-image reports for generated content. `Multiple Records` excludes templates with fields on document pages in a multipage document or on more than one parent page.

For hierarchical content, inspect `Document.xmlElements` and follow `XMLElement.xmlContent` to native text, tables, or artwork. Use `markup` to associate existing content and `placeXML` to replace destination content. Choose merge or append through XML import preferences before importing: merge can remove unmatched existing elements and repeat placeholders. Native style/tag maps connect formatting with XML structure independently of page stacking.

For XML export, configure tagged scope, CALS tables, image treatment, and XSLT through native preferences. XML tags and PDF export-tag maps serve different outputs. Set publication title, description, and rights in document metadata rather than deriving them from visible text.

## [04]-[BOOKS]

For publications spanning documents, use `Book.bookContents` to retain membership and order. Before synchronization, inspect content status, `styleSourceDocument`, enabled categories, and `smartMatchStyleGroups`. Select dependent style categories together. Synchronization replaces matching definitions and adds absent definitions while retaining unrelated definitions. Parent-page synchronization can change local overrides.

Call `Book.synchronize` for all book contents or `BookContent.synchronize` for one member. Synchronization saves closed documents; save open documents explicitly. After changes, choose native repagination, number updates, or cross-reference updates for the affected relationship. Use book export, preflight, and packaging for combined output without concatenating separately exported files.

## [05]-[EXPORT]

Choose `ExportFormat` from delivery requirements, then inspect the corresponding native preference owner before `Document.exportFile`. Print PDF, interactive PDF, reflowable EPUB, and fixed-layout EPUB have distinct settings. Reuse a native preset when settings match the required output. Select export conversion, destination profile, and output condition independently of document profiles.

For print PDF, choose pages or reader spreads, bleed source, exported layers, and image sampling separately from compression. Reader spreads combine a spread into one PDF page without imposing printer signatures. Set PDF/X compliance, transparency handling, font embedding, bookmarks, hyperlinks, and tagging through native export preferences.

For `asynchronousExportFile`, retain returned `BackgroundTask`, use `waitForTask`, and report `status` with output-affecting `alerts`. An output path alone does not establish completion. Use `render` for whole pages, spreads, or items and `screenshot` for application UI. Inspect exported files separately for tags, font embedding, and reading order.

## [06]-[ACCESSIBILITY]

Before accessible export, inspect paragraph and character `styleExportTagMaps`, then assign semantics for the selected format. PDF tags differ from EPUB/HTML tags, CSS classes, and ARIA roles. Create lists and tables through native structures; typed markers and visually aligned text do not establish equivalent semantics.

Arrange content through `Document.articles` and `Article.articleMembers`. Set `articleExportStatus` and `taggedPDFPreferences.structureOrder` to use articles for PDF order, then enable structure in PDF export. Article order leaves visual stacking unchanged. Anchor artwork when reading order must follow text flow, and retain native destinations for bookmarks, hyperlinks, and cross-references.

Through `ObjectExportOption`, choose alternative-text and actual-text sources and read resolved `altText()` or `actualText()`. Custom text fields alone omit metadata-derived content. Describe informative artwork according to purpose; use `applyTagType` to mark decorative content as artifacts. Preserve captions as text associated with figures.

Inspect `Table` for supported header rows, header columns, and caption paragraphs, then assign native structure instead of relying on cell appearance. Discover Table Summary through runtime members or native UI before assigning a property; UI availability does not establish a scripting member.

For EPUB, set accessibility metadata to describe supported features and hazards. Review exported structure and accessibility before declaring conformance; document appearance and successful export alone do not establish accessible reading order.

## [07]-[PACKAGING]

For source-file delivery, resolve missing assets and production errors before `Document.packageForPrint` or `Book.packageForPrint`. Inspect native arguments to select fonts, links, profiles, hidden content, graphics relinking, IDML, PDF, reports, and document hyphenation exceptions. Return native Boolean results. Adobe Fonts activations are excluded from copied fonts. Packaging collects available assets without recovering missing sources.
