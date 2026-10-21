# [TODO]

Remaining work on Creative Cloud libraries and products, for a fresh session.

[STANDARDS]:
- `CLAUDE.md` and memories `code-quality-bar`, `typescript-quality-floor`, `swift-quality-floor`, `library-shape`
- Skills `uxp-coding`, `clean-prose`, `search-code`, `use-ghidra`, `watch-apps`, `manage-repo`

[SOURCES]:
- Host facts in `docs/research/applications/adobe/facts/*.txt`, decompiled hosts in `docs/research/applications/adobe/decompiled/`
- Installed `indesign-uxp-types` declarations, and InDesign's own `$.dictionary` read through `Model` in `model.ts`
- Facts come from these sources, dependency sources, and Ghidra; `.archive/` folders are no source for briefs or code

## [01]-[STATE]

- Status streams and the file watch yield typed results; waits on host state observe change events under a caller deadline
- `@rasm/automation` is one module: `Installation`, `InstalledFace`, the `Requests` RPC group, and the `Automation` client layer
- `@rasm/uxp` holds plug-in runtime (`plugin`, `script`, `Uxp`, `Plugin`, `Manifest`) and the dialed RPC link, which dials the manifest's network domain under its `Peer` identity
- `@rasm/mcp` holds the loopback MCP server, `targeted` routes with a status trailer on every result but a pure interrupt, `listen` and `publish`, and the `channel` endpoint
- `@rasm/search` is `Unmatched`, owning `matching` and `single` with collator equality, ranking and a remainder count in its message
- Verb classes: `Inspect`, `Kind`, `Change`, `Create`, `Remove`, `Invoke`, `Script`, `FindChange`, `Render`, `Font`, `Menu`, `Preflight`, `Queries`
- A mutation locates its target once; the baseline covers every document the located elements belong to; a find without a change is a read
- `Kind` answers the index of classes and enumerations or one declaration, published at `indesign://kinds` with name completion
- Atelier tools: `launch`, `screenshot`, `font`, `execute_script`, `command` (dictionary command, dialog actions, menu action), registered again whenever the installed application names change, which tools offer as choices
- Atelier publishes InDesign `inspect`, `change`, `create`, `remove`, `find_change`, and `render` as tools
- Atelier publishes InDesign `menu`, `preflight`, `queries`, and `kinds` as resources
- Every tool result ends with the application's status as a resource block, the form Claude Code forwards beside structured content
- `Unmatched` candidates render ranked and capped with the count of the rest
- Menu actions run under never-interact, an ellipsis-named action opens a dialog and is refused, its settings live in preference objects
- Scripts run through a linked extension with TypeScript stripped, else through the dictionary's script command (`miscDjxM`, `miscdosc`) as ES3
- Mutation results hold preflight of each touched document: issue count and issues introduced against the per-mutation baseline
- The daemon's `/channel` endpoint (`atelier-channel` in `.mcp.json` and `.codex/config.toml`, protocol 2025-11-25 alone) pushes `notifications/claude/channel` status events following every running Adobe application the server serves, when observation starts and after each change, and no preflight events
- A Claude Code session opts in with `--dangerously-load-development-channels server:atelier-channel` and receives each event as a `<channel>` turn; headless sessions and Codex receive none, and nothing else over HTTP reaches a model between calls
- Effect's own resource path renders the first error message alone and turns an interrupt into an error; tools render the full cause

## [02]-[OPEN]

- Menu actions without an ellipsis that still open a window under never-interact are unknown; the first one found joins the refusal rule
- Dictionary refusal rows other than InDesign `show` rest on dictionary text, the first row a send disproves or a new one found changes the table
- Dismissing an InDesign script dialog through an Accessibility press activated InDesign once, closing Keyboard Shortcuts did not; other dialogs unmeasured
- Single key bindings have no DOM member, shortcut editing stops at `generalPreferences.keyboardShortcutSet` and `applyShortcutSet`
- UXP `process` holds no pid, so a panel cannot name its instance and panel routes address the single running one
- Accent folding on PostgreSQL search: reconcile observes, creates, and gates `unaccent` with a text search configuration the query names
- Table members return decoded rows, so consumers stop decoding through the table's own struct and flattening each result

## [03]-[DEPTH]

Each category states what its verbs and views cover and what stays shallow, then rebuilds the whole chain from host metadata and verifies live.

### [01]-[TEXT]

- `TextView` reads runs with style, font, size, design axes, OpenType features, run-only settings, and attributes differing from applied styles
- Build every composition attribute of Text, Paragraph, and Character as class rows state them: justification, kerning, leading
- Build per-run reads and writes, fonts with design axes and OpenType features on ranges, and overrides against applied styles

### [02]-[STYLES]

- `StyleView` reads group path, `basedOn` chain to the root, next style, own attributes against its base, and `Composition` of a paragraph style
- `Composition` reads GREP styles, nested and line styles, tab list, list type and level, bullet, and numbering format
- Build paragraph, character, object, table, and cell styles and groups with `basedOn` chains and next style
- Build automatic GREP styles, nested and line styles, drop caps, bullets and numbering, and tabs as one `Composition`
- `Composition` belongs to a style, a text range, and the document text defaults
- Agents must build complex automatic GREP styling in plain calls, with no fragile logic in the library
- Automatic GREP styles stay apart from Find/Change GREP in `find.ts`
- GREP style is a `NestedGrepStyle` {`grepExpression`, `appliedCharacterStyle`} of a paragraph style
- Characters a GREP style matches read resolved attributes, keep `appliedCharacterStyle` [None], and split no `textStyleRanges`

### [03]-[PAGES_AND_PARENTS]

- `PageView` and `SpreadView` read offset, side, extent, parent, applied parent, section, margins, columns, and item counts
- Build page counts, ids, applied parents with inherited items, per-page size, sections, guides, margins, columns, and spreads
- Reorder and resize pages through `page.move` and `page.resize`

### [04]-[COLORS]

- Build swatches, colors, tints, gradients, mixed inks, color groups, and document versus application swatches
- None, Registration, Paper, and Black carry no brackets, and `remove()` throws 90898 "This color cannot be deleted."
- Swatch and Color hold no `removable` or `editable` property; views derive host ownership from the bracketed-name convention alone
- New documents hold Black and Paper alone; InDesign refuses changes to Registration and Paper, and color work adds swatches

### [05]-[SETTINGS]

- `DocumentView` reads intent, facing pages, default size, ruler units and origin, preflight profile, and text defaults
- Build preference objects as typed views and writes: document, view, text, grid, transparency, and script preferences
- Measurements stay in points under `Hold`, which sets `scriptPreferences.measurementUnit` for each call

### [06]-[LINKS_AND_GRAPHICS]

- `LinkView` reads status, path, URI, type, needed, edited, size, date, and placed parent
- Build relink, update, unlink, placed graphics, fitting, and effective ppi

### [07]-[PREFLIGHT]

- `Preflight` returns issues under one profile with profile names, and every mutation returns its per-document summary
- Build profiles and rules as views, process, and report beside the per-mutation summary and the status push

### [08]-[STRUCTURE]

- Build conditions, notes, footnotes, tables, and cells
- Build one method-call verb over metadata rows for operations `change`, `create`, and `remove` cannot express
- Method calls decode arguments by parameter type, one verb in place of a tool per method
- Fields typed as a class or text (`appliedFont` as Font or String) resolve the name through the class before taking the text, as fields typed by one class do
- `create` passes `properties` as `add`'s `withProperties` where the method declares it, and the `owner` description names `/` as the application

### [09]-[OTHER_APPLICATIONS]

- Illustrator, Photoshop, After Effects, and Acrobat run `execute_script` and `command` through their dictionary commands
- Photoshop UXP extension reuses `@rasm/uxp` as its own contract, panel, and product on the InDesign pattern
- Atelier decides which peer answers which verb

## [04]-[METHOD]

- Rebuilds change source and consumers in one chain to less code, surface, fields, and parameters, never a shuffle into new types
- Domain libraries serve future extensions unrelated to MCP, and no domain module reads a tool schema or imports its consumer
- Effect 4 and installed packages (MCP, typesetting, Adobe types) are used in full, and a missing package joins at its newest release
