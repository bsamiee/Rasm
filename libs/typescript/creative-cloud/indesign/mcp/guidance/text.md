# [TEXT]

## [01]-[FORMATTING]

Choose styles for reusable formatting or a text range for a local override. Use `inspect` with native class names to discover attributes and methods, then read returned object specifiers with selected fields. Styles describe definitions; character getters report native attributes. Application and document text defaults control creation settings, not existing text.

Aggregate `Text` or `Paragraph` getters and `textStyleRanges` do not establish uniform character formatting. GREP formatting can change inside one text style range while `appliedCharacterStyle` remains `[None]`. Even a character's `pointSize` can disagree with rendered size under conflicting GREP rules. Inspect character fields, `appliedNestedStyles`, and ordered rule definitions to diagnose formatting; render actual text to judge composed output. Use [styles](indesign://styles/reference) for inherited definitions and automatic formatting.

Use returned text objects to address paragraphs or characters inside cells, footnotes, and endnotes. Local indices cannot address `parentStory.characters`. Use `InsertionPoint` to insert at a position without requiring a character, including an empty story.

## [02]-[COMPOSITION]

Inspect `Paragraph` or `ParagraphStyle` for justification, word and letter spacing, glyph scaling, hyphenation, and paragraph columns. Choose a composer and spacing limits together when controlling line breaks. Use keeps for headings with following text and first or last lines at column breaks; `startParagraph` selects a page or column break. Character `noBreak` keeps a text span together. Native drop caps size initial characters by line count and apply a character style while surrounding text composes around them.

`leading` and `autoLeading` belong to text formatting. Font-file metrics alone do not establish composed baselines or pagination. Inspect native `Line` or `Character` positions after composition, then render the page to judge breaks and alignment. `spaceBefore` is ignored at a frame or column top; frame inset and first-baseline settings govern that placement.

For a shared baseline rhythm, choose explicit body leading before setting grid intervals. Enable paragraph-style `alignToBaseline` for text that must follow the grid. For a multiline caption that shares a starting baseline but needs different leading, enable `gridAlignFirstLineOnly` and set caption leading separately. Read composed line baselines to distinguish requested leading from placement constrained by a grid.

`Story.storyPreferences` owns optical margin alignment and story direction. `TextFrame.textFramePreferences` owns frame columns, insets, vertical justification, and auto-sizing. Baseline grids retain document and frame owners. Use [layout](indesign://layout/reference) for frame settings, threading, grids, and reflow.

## [03]-[RULES_AND_BORDERS]

For separators that follow text reflow, use paragraph rules. Choose borders or shading for an enclosing stroke or background. Save decoration on `ParagraphStyle` for reuse; native composition moves and resizes decoration with paragraphs.

Choose text or column width before setting rule indents, which remain independent of text indents. Above-rule offset measures first-line baseline to rule bottom; below-rule offset measures last-line baseline to rule top. `keepRuleAboveInFrame` retains an above rule inside frame bounds.

For borders, choose whether a split paragraph draws borders at frame or column breaks. Native merging joins adjacent borders and shading with identical settings, except across columns or nonthreaded frames. Render split and adjacent styled paragraphs to judge continuity.

## [04]-[TABS]

Paragraph and paragraph-style `tabStops` own `position`, `alignment`, `alignmentCharacter`, and `leader`. Tabs align following text; paragraph indents determine paragraph edges. Ruler guides do not define text tabs.

With `paragraphStyle` as an existing `ParagraphStyle`, a right-aligned dotted leader uses native construction:

```js
paragraphStyle.tabStops.add({
    position: '40 mm',
    alignment: TabStopAlignment.RIGHT_ALIGN,
    leader: '.',
});
```

Insert a tab character before content aligned by the stop. `CHARACTER_ALIGN` with `alignmentCharacter` aligns decimal marks, currency symbols, or other chosen characters. Native leader repeats within tab spacing; repeated punctuation in text does not track layout. Edit or remove `TabStop` members for local changes; `tabList` assigns a complete tab definition.

Use [styles](indesign://styles/reference) for list numbering and generated contents. Automatic list markers remain outside story text; inspect native paragraph results when reading displayed numbering.

## [05]-[LANGUAGE]

Set `appliedLanguage` for spelling and hyphenation, then choose `composer` for composition. `paragraphDirection` governs paragraphs, `characterDirection` handles directional spans, and `Story.storyPreferences.storyDirection` governs story flow through columns. Right alignment does not select right-to-left direction.

Inspect installed language, composer, and enum values. Arabic composition includes digit forms, Kashida, diacritics, and justification alternates. CJK composition includes mojikumi spacing, kinsoku line breaking, ruby, kenten, warichu, and vertical text. Availability depends on host edition, composer, and font; panel visibility alone does not establish scripting support. Render actual language content to assess shaping, breaks, and direction.

## [06]-[FONTS]

Inspect `/font` with fields `fontFamily`, `fontStyleName`, `name`, and `status` to choose a host-visible family and face. Use `font` with a returned name for family styles, document uses, embedding permissions, coverage, features, and metrics. Document fonts can include unavailable faces; native status distinguishes availability from a reference stored in text.

Read `Font.designAxesName`, `designAxesRange`, and `designAxesValues` for variable-axis order, limits, and values. Set point size before explicit coordinates: size changes reset `opsz` to point size within native axis limits. Apply coordinates through text or style `designAxes`, or `setNthDesignAxis(index, value)`. Axis indices come from that face; weight, width, and optical size have no universal position. Static faces can reject axis reads.

To measure a chosen glyph for image or heading alignment, request `font` with the selected face name and `text: 'f'`, or the intended text. Pass `coordinates` for an explicit variable-font instance. `size` makes `opsz` follow point size; omit `size` to retain an explicit optical-size coordinate. Returned geometry remains in em units.

`metrics.glyphs` follows visual glyph order. `codepoint` identifies a glyph; `cluster` identifies a UTF-16 input offset, and clusters can span characters or glyphs. Advances move the pen; offsets shift glyphs from that position. `extents` holds native bounds or `null`. Positive y points upward; negative `height` extends below `yBearing`. With an available extent, `(yOffset + extents.yBearing) * pointSize` gives glyph top in points above the current pen baseline.

Font-file shaping uses one script and direction with default features, not InDesign composition. Lowercase `f` geometry depends on face, size, and variable instance; ascent, cap height, or x-height does not substitute for a chosen glyph. Choose a native first-baseline mode for typographic alignment, or a fixed offset for a measured position. Preserve editable text and judge document alignment from composed first-line positions and native rendering. Use [layout](indesign://layout/reference) for frame baselines and guide placement.

Inspect text or style OpenType attributes and enum values before choosing features. `Text.opentypeFeatures` accepts tag/value pairs beside `otf*` controls; `[['smcp', 1]]` requests small caps. Script, language, glyph context, and face decide behavior. `Font.checkOpenTypeFeature` reports GSUB/GPOS support, which omits AAT features that native composition can apply. Retain native numeric settings without inventing a stylistic-set mask.

Auto underline and strikethrough offsets and weights read as `-9999`. Font metrics do not fully predict composed strokes; render or export actual text to observe them. Reported subset metrics describe available subset data, not an original variable font's complete axes or glyph coverage.

## [07]-[NOTES]

Preserve native note references so numbering and placement follow text edits. Set generated markers, numbering, and note styles through `Document.footnoteOptions` or `endnoteOptions`. `FootnoteOption.spacer` sets separation above footnotes; `spaceBetween` separates notes. Footnote paragraph-style `spaceBefore` and `spaceAfter` do not control that spacing. Inspect `TextFrame.textFramePreferences` for frame overrides when document settings do not explain placement.

## [08]-[CONDITIONS]

For alternative text in one document, use native conditions rather than deleting hidden variants. `Text.appliedConditions` references document conditions; `Condition.visible` controls their visibility. `ConditionSet.setConditions` stores condition/visibility pairs, and `Document.conditionalTextPreferences.activeConditionSet` selects a saved combination. Native search omits hidden conditional text even when hidden-layer inclusion is enabled. Choose the intended visibility before searching or exporting.
