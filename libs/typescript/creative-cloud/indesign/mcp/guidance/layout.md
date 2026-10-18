# [LAYOUT]

## [01]-[DOCUMENT_STRUCTURE]

Use `inspect` to discover `Document` collections, then read page, spread, parent, section, layer, and story references. `masterSpreads` contains parent spreads; count parent pages through each spread's `pages` collection. Use `Page.documentOffset` for document order and `Page.name` for displayed numbering. `Section` controls numbering starts, continuation, prefixes, markers, and alternate-layout names. Preserve page references when sections reuse numbers.

Change page setup and bleed through `Document.documentPreferences`, and page margins and columns through `Page.marginPreferences`. Individual pages can differ from document defaults. For page moves or foldouts, inspect document and spread shuffle settings. Spreads can contain more than facing-page pairs.

Use [scripting](indesign://scripting/reference) for native discovery and execution.

## [02]-[PARENT_PAGES]

InDesign's DOM names parent spreads `MasterSpread`. `Page.appliedMaster` assigns a parent, and `MasterSpread.appliedMaster` defines parent inheritance. `Page.masterPageItems` contains inherited items that remain neither overridden nor detached. Inspect local page items separately, and follow `overriddenMasterPageItem` to retain an overridden item's origin.

Call native `override` when an inherited item needs a local edit. Unmodified attributes continue to inherit; `detach` ends inheritance. Removing an override restores parent control. Overriding a threaded parent frame can override other visible frames in that thread. Use `showMasterItems` to control display, print, and export while retaining parent assignment.

Parent content on a differently sized page can have a `masterPageTransform`. Preserve native parent transforms when positioning inherited content.

Place native page-number or section-marker characters in parent text frames. Discover marker values through `SpecialCharacters`. Document pages resolve displayed values from page numbering and section settings.

## [03]-[LAYERS_AND_CONTENT]

Read `pageItems` for contained objects and `allPageItems` for nested content. Follow `itemLayer` and `parent` references to distinguish layer membership, groups, containing frames, and placed content. Objects on a pasteboard belong to a spread without requiring a document page.

Read layer visibility, locking, and printability alongside item settings to explain effective state. Use native layer and object arrangement methods for stacking order. Story order comes from text containers, independently of stacking or page position.

## [04]-[MEASUREMENTS]

`Document.viewPreferences` owns ruler units and origin; `Document.zeroPoint` supplies the custom origin. `Application.scriptPreferences.measurementUnit` governs script measurements. Use unit-bearing strings where native parameters accept measurements, and retain each property's declared array order. Page bounds use `[y1, x1, y2, x2]`, while coordinate points use `[x, y]`.

Use native `resolve` with an explicit coordinate space for positions. Native `transform`, `resize`, and `reframe` apply the selected origin and bounds. Geometric bounds exclude stroke extent; visible bounds include strokes. Neither establishes exported image dimensions or a screenshot coordinate transform.

## [05]-[GRIDS_AND_COLUMNS]

### [05.1]-[BASELINE_RHYTHM]

Use [text](indesign://text/reference) to choose body leading and paragraph alignment. Set `Document.gridPreferences.baselineDivision` from body leading, then set `baselineGridRelativeOption` and `baselineStart` together to position baselines relative to page or top margin. Inspect `TextFrame.baselineFrameGridOptions` before changing document settings; an enabled frame grid takes precedence.

When document-grid lines must coincide with baselines, coordinate intervals in `Document.gridPreferences` with `Document.zeroPoint`. Equal intervals with different origins remain displaced. Fit margins and column widths to grid intervals when column edges must coincide with grid lines. Choose margins from content, reading conditions, binding, and trim.

For CJK character cells, spacing, and grid alignment, use native `Page` or `Story` `gridData`. `Document.namedGrids` stores reusable grid formats independently of baseline grids.

### [05.2]-[MODULAR_GUIDES]

Enabling `removeExisting` deletes existing guides, including guides on hidden or locked layers. Choose guide layer and removal behavior before calling `Spread.createGuides` or `MasterSpread.createGuides` for rows and columns fitted to pages or margins. Select row heights and gutters from text rhythm and image or caption space.

For glyph-top alignment, request `font` with the selected native face name and chosen glyph in `text`. Use [text](indesign://text/reference) for glyph measurement and native appearance constraints. Place a horizontal guide at measured glyph top relative to composed baseline, then repeat at baseline intervals, or shift relevant modular row starts by that offset. Keep body text on baselines while image or display tops follow guides.

Store reusable guides on parent spreads and apply parents to document pages. Inspect spread and parent-spread `guides`, then follow guide layer references. `Layer.guides` alone does not inventory parent-spread guides. Choose page or spread guides by required extent. Render composed pages to judge text and image alignment; guide positions alone do not establish visible text edges.

### [05.3]-[FRAME_ALIGNMENT]

Set text columns, gutters, insets, balancing, and vertical justification through `TextFrame.textFramePreferences`. Choose insets before first-line placement. Set `firstBaselineOffset` to `FirstBaseline.CAP_HEIGHT` for uppercase tops aligned to top inset, or `FirstBaseline.FIXED_HEIGHT` for a measured baseline distance through `minimumFirstBaselineOffset`. Use a measured fixed offset for chosen glyph-top alignment. Read composed `Line.baseline` and render with intended paragraph grid alignment to judge final placement. Use [styles](indesign://styles/reference) for reusable frame settings.

For separators inside a multi-column text frame, use native `columnRule*` preferences or an `ObjectStyle`'s column-rule category. Column rules follow text from first-line ascent to last-line descent, with native insets and horizontal offset. Use page-item lines between separate frames or for independent artwork. Use [text](indesign://text/reference) for paragraph rules.

## [06]-[OBJECT_ARRANGEMENT]

`Document.align` and `distribute` arrange native objects against the chosen page, margin, spread, selection, or reference object. Inspect method arguments and native enums for the requested reference. Paragraph justification and frame vertical justification align text within an object.

For placement that follows text, call an item's `anchoredObjectSettings.insertAnchoredObject` with an `InsertionPoint`. Choose inline, above-line, or custom positioning through native anchor settings. Custom positioning distinguishes object and page reference points, offsets, and spine-relative placement. Use `releaseAnchoredObject` to end anchoring. Use [graphics](indesign://graphics/reference) for fitting, wrap, and appearance.

## [07]-[THREADS_AND_REFLOW]

`Story.textContainers` supplies thread order, including text paths. Native `previousTextFrame` and `nextTextFrame` change connections. Unthreading leaves content in an upstream story and can produce overset; separate stories require an explicit text operation. `Story.overflows` reports overset composition.

For frame auto-sizing, assign related `TextFrame.textFramePreferences` attributes before `autoSizingType`; InDesign adjusts reference points to match sizing dimensions. When final line positions are needed after edits, call native document or story `recompose` before reading composition.

For automatic page addition or removal, configure Smart Text Reflow through `Document.textPreferences`. Choose insertion location, primary-frame restriction, facing-page preservation, and empty-page deletion. Select primary flow through `MasterSpread.primaryTextFrame` and thread primary frames across both parent pages in a facing spread. Reflow from non-parent frames creates full-page, single-column frames rather than copying arbitrary frame settings. Empty-page deletion requires an emptied frame to be the page's only object.

When synchronous pagination is needed, inspect `TextPreference.smartTextReflowSync` for native page addition and deletion after reflow. Smart Text Reflow must be enabled.

## [08]-[FLEX_LAYOUT]

Inspect `FlexObject` and its native collection methods for container creation, child insertion, direction, wrapping, spacing, and sizing. `ObjectStyle` enables reusable Flex attributes through `enableFlexLayoutAttributes` and `flexLayoutAttributeOptions`.

Choose container and child sizing together. Changing a container to Auto makes Fill Space children fixed; changing a child to Fill Space makes an Auto container fixed. Fill Space disables wrapping. Flex disables enclosed text-frame auto-sizing; use native child sizing. Graphic frames retain fitting settings when filling space.

## [09]-[LAYOUT_VARIATIONS]

To resize existing layout, call `Document.adjustLayout` with only changed page dimensions, margins, or bleed. Bleed changes apply at document scope.

To create another layout, call `Document.createAlternateLayout` on chosen spreads with explicit page size, text-style creation, story linking, and liquid-layout rule choices. Sections retain layout names. Use `Page.layoutRule` for adaptation and native guide or object constraints for resizing or movement. Choose linked stories when content must retain native update relationships.

Use native whole-page or spread `render` for composed layout and `screenshot` for application controls or dialogs. Image extent remains independent of native geometric bounds.
