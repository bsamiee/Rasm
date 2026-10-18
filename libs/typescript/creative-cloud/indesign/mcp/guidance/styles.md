# [STYLES]

## [01]-[DEFINITIONS]

Inspect `Document` for style collections. For paragraph, character, object, table, and cell styles, use corresponding `all*Styles` properties to include grouped definitions. Direct collections omit nested members. Retain returned specifiers when names repeat. Use stroke styles for reusable stroke patterns and TOC styles for generated contents.

Reuse matching definitions. For a related variant, set `basedOn` and change intended differences; copying resolved attributes creates explicit values. Limit character styles to attributes their application changes. Inspect collection signatures before creating definitions through `execute` Script. Inspect application methods before choosing override-removal arguments, which differ between families. Preserve native references when moving definitions into groups.

Use [text](indesign://text/reference) for composed formatting and [scripting](indesign://scripting/reference) for native references and calls.

## [02]-[GREP_STYLES]

For pattern formatting that follows text edits, add `nestedGrepStyles` to a named paragraph style. Rules persist in document formatting and apply `CharacterStyle` attributes during composition. Paragraph-local rules create overrides. `basedOn` supplies inherited rules without copied duplicates.

Create a rule from inspected style specifiers:

```javascript
const paragraph = resolve('<paragraph-style-specifier>');
const character = resolve('<character-style-specifier>');
return paragraph.nestedGrepStyles.add({
    appliedCharacterStyle: character,
    grepExpression: '(?:Map|Figure|Table)\\h+\\K\\d+',
});
```

Example returns a rule reference for later edits and formats digits without labels or spacing. Apply paragraph style to intended paragraphs; a definition alone changes no unstyled text. Inspect existing rule references before editing `grepExpression`, assigning `appliedCharacterStyle`, or calling `remove()`.

GREP styles match within one paragraph and leave contents unchanged. Use Find/Change for formatting criteria, replacement text, or cross-paragraph searches. Use native owners for paragraph hierarchy, list markers, tables, and generated contents. When formatting conflicts, inspect characters, `appliedNestedStyles`, and ordered rules, then render composed text; native attribute getters alone do not establish appearance.

Use [GREP](indesign://grep/reference) for native expression syntax.

## [03]-[AUTOMATIC_FORMATTING]

For enlarged opening characters, set `ParagraphStyle.dropCapCharacters`, `dropCapLines`, and `dropCapStyle`. Native composition fits characters across the specified lines. Use `nestedStyles` for counted or delimited opening phrases, including run-in headings.

For sequential formatting, add nested rules in reading order with `delimiter`, `repetition`, and `inclusive`. Rules start at paragraph beginning and continue from preceding endings; inclusive includes the ending delimiter. Use native repeat rules for recurring sequences within one paragraph.

For formatting tied to composed lines, use `nestedLineStyles` with `lineCount`. Affected text follows composition when frame width or contents change. Nested styles take precedence over nested line styles when both set one attribute.

Set `nextStyle` for a style sequence while typing. For existing paragraphs, use Paragraph Styles' Apply [Style Name] Then Next Style command, or `ObjectStyle` with `enableParagraphStyle` and `applyNextParagraphStyle`. Object-style paragraph formatting applies only to unthreaded text frames. Assigning `appliedParagraphStyle` alone does not run a sequence.

## [04]-[OBJECTS_AND_TABLES]

Choose `ObjectStyle` categories according to settings the style must control. Ignored categories preserve existing formatting; disabled effects can remove applied effects. Inspect category settings alongside enable flags when configuring layout, fitting, anchoring, wrapping, effects, or export behavior.

For reusable table formatting, create or reuse `ParagraphStyle` for cell text, select it from `CellStyle`, then assign cell styles to `TableStyle` regions. Paragraph rules apply character formatting within cells. Follow references when changing formatting; inspect explicit cell styles and local overrides when appearance differs from region defaults. Create rows and columns through `Table`; region styles define formatting alone.

Use [layout](indesign://layout/reference) for frames, [graphics](indesign://graphics/reference) for appearance, and [production](indesign://production/reference) for export settings.

## [05]-[LISTS]

Share one `NumberingList` among paragraph styles for levels of one sequence. Use separate lists for independent sequences, including figure and table captions. Inspect native level limits and restart settings before constructing a hierarchy.

Create a list with `document.numberingLists.add(name, continueNumbersAcrossStories, continueNumbersAcrossDocuments)`. Document continuation applies to book documents. Assign the returned object through `appliedNumberingList` on level styles. Configure bullets, number format, level, expression, restart policy, and indentation on paragraph styles. Higher-level references use native numbering expressions; typed digits lose automatic renumbering. Assign marker character styles to control marker formatting independently of paragraph text.

Automatic markers remain outside story contents and GREP matches. Read `Paragraph.bulletsAndNumberingResultText` and `numberingResultNumber` for generated results. Use [text](indesign://text/reference) for indentation and tab leaders.

## [06]-[GENERATED_TEXT]

For a contents list, identify source paragraph styles and add `TOCStyleEntries` using native `styleName`. Assign `formatStyle` for generated text, configure hierarchy, page-number placement, separators, and character formatting, then define tab leaders through output paragraph styles. Set book coverage and bookmarks on `TOCStyle`. Use `numberedParagraphs` to include source list numbers; numbering an output paragraph style creates an independent sequence that can renumber a selected subset.

Keep each TOC in a separate story: replacement overwrites an entire threaded story. Call `Document.createTOC` after source edits with intended replacement, book scope, placement, overset inclusion, and destination layer. Retain output formatting in definitions for regeneration. GREP styles format generated entries without regenerating contents.

For running headers, configure `TextVariable` with a source paragraph or character style. Set first/last occurrence, case, punctuation, and surrounding text through `variableOptions`. Place `TextVariableInstance` objects on parent pages and read `resultText` for composed output. Pages without matching sources use a preceding occurrence; headers remain empty when none exists.

For an index, create topics and native page references to source locations. Configure `Document.indexOptions` for output formatting and replacement, then call `Index.generate` to create index text. `Index.update` refreshes only the Index panel preview.

For cross-references, define text and character formatting through `CrossReferenceFormat` building blocks. Create `CrossReferenceSource` with a native text range and format, then connect that source to a `Hyperlink` destination. Call `CrossReferenceSource.update()` after source edits to regenerate reference text while retaining native relationships.
