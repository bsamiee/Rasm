# [TODO]

Remaining work on Creative Cloud libraries and products, for a fresh session.

[STANDARDS]:
- `CLAUDE.md` and memories `code-quality-bar`, `typescript-quality-floor`, `swift-quality-floor`, `library-shape`
- Skills `uxp-coding`, `clean-prose`, `search-code`, `use-ghidra`, `watch-apps`

[SOURCES]:
- Host facts in `docs/research/applications/adobe/facts/*.txt`, decompiled hosts in `docs/research/applications/adobe/decompiled/`
- Installed `indesign-uxp-types` declarations, and InDesign's own `$.dictionary` read through `Model` in `model.ts`
- Facts come from these sources, dependency sources, and Ghidra; `.archive/` folders are no source for briefs or code

## [01]-[STATE]

- `libs/swift/macos` holds installed applications, launch, activation, processes, faces, and screenshots for any macOS app
- `libs/swift/macos` `Elements.swift` reads application status and status streams over Accessibility and performs typed element actions
- `apps/creative-cloud/atelier/macos` is the Adobe helper over it: dictionaries, Apple Events, typed native values, Effect RPC on stdio
- `@rasm/automation` is one module: `Installation`, `InstalledFace`, the `Requests` RPC group, and the `Atelier` client layer
- `Installation` members cover discovery, launch, status, watch, press, screenshot, dictionary, commands, and script runtime
- `@rasm/uxp` holds plug-in runtime (`plugin`, `script`, `Uxp`, `Plugin`, `Manifest`) and dialed RPC link (`Link`, `linked`)
- `@rasm/mcp` holds the loopback MCP server, `targeted` routes with a per-result `trailer`, and `listen`/`publish` over panel peers
- `@rasm/indesign` is a flat core (`model`, `references`, `transactions`, `mutations`) beside `service/contract.ts` and `panel.ts`
- Verb classes: `Inspect`, `Kind`, `Change`, `Create`, `Remove`, `Invoke`, `Script`, `FindChange`, `Render`, `Font`, `Menu`, `Preflight`, `Queries`
- Views by host kind: document, page, spread, layer, story, text, style, swatch, link, font; `Inspect` returns the first that matches
- `apps/creative-cloud/indesign/galley/panel.ts` is one `plugin` call composing `panel` with the origin its manifest declares
- Atelier tools: `launch`, `screenshot`, `font`, `execute_script`, and `command` (dictionary command, dialog action, menu action)
- Atelier publishes InDesign `inspect`, `change`, `create`, `remove`, `find_change`, `render` as tools
- Atelier publishes InDesign `menu`, `preflight`, `queries`, `kind` as resources
- Atelier resources: `adobe://applications`, `atelier://faces`, `atelier://executions`, `adobe://dictionary/{application}` templates
- Every routed result ends with its application's status from `Installation.status()`, except a result that is a status
- Each application has one script path: UXP through its panel with TypeScript stripped, else its dictionary script command
- `execute_script` names the runtime that ran the source; `Installation.runtime()` holds Acrobat, ExtendScript, and UXP rows
- Mutation results hold preflight of each touched document: issue count and issues introduced against a per-mutation baseline
- Panel serves a `changed` stream of those rows; Atelier's `follow` emits `notifications/resources/updated` for `indesign://preflight`
- Delivery of that resource notification to an agent is unverified, and status on each result is what agents read
- Errors reach agents as one rendered cause at `register` in `@rasm/mcp/server`, with no per-host error type
- Checkers were at zero over the tree at commit 75f0c7418
- Restart the daemon with `mise daemons restart atelier`, the helper with `nx run Atelier:install`
- Install the panel with `nx run @rasm/galley:install`, then quit and relaunch InDesign through `command` and `launch`

## [02]-[DEPTH]

Each category states what its verbs and views cover and what stays shallow, then rebuilds the whole chain (domain module, contract, panel, app) from host metadata and verifies live once built.

### [01]-[MODEL_AND_CONTRACT]

- `PATH` and `WIRE` in `service/contract.ts` and `ORIGIN` in `@rasm/uxp/runtime` are constants both ends import and hand back
- Derive path, serialization, and origin from the contract or link that owns them, so no consumer spells one
- `READING` in `@rasm/mcp/server` is an exported constant under the same rule

### [02]-[STATUS]

- Swift `statusChanges()` feeds the helper `watch` stream and `Installation.watch()`, which no Atelier code consumes
- Agents see status on each result and as progress while a panel call waits, and nothing between calls (hung app, dialog opened)
- Build one composition point in Atelier that follows `watch()` for each application in use, starting at `launch`
- Push each status change and each `changed` preflight row to the session, so agents learn of hangs, dialogs, and issues unasked
- Claude Code takes server pushes as channel events: `notifications/claude/channel` from a server declaring `claude/channel`
- Sessions opt a channel in with `--channels`, and on the v2 runtime a server negotiating protocol 2026-07-28 delivers no channel event
- `@rasm/mcp/main` offers `McpProtocol.v2026_07_28` first; decide protocol and transport from the Claude Code channels reference
- Investigate before building: which pushes a Claude Code session and a Codex session each receive, under which revision and flags
- Record what each push shows the model versus the terminal, then put the options with their facts to the owner and build the chosen one

### [03]-[TEXT]

- `TextView` reads runs with style, font, size, design axes, OpenType features, and attributes differing from applied styles
- Build every composition attribute of Text, Paragraph, and Character as class rows state them: justification, kerning, leading
- Build per-run reads and writes, fonts with design axes and OpenType features on ranges, and overrides against applied styles

### [04]-[STYLES]

- `StyleView` reads group path, `basedOn` chain, next style, own attributes against its base, and `Composition` of a paragraph style
- `Composition` reads GREP styles, nested and line styles, tab stops, list type and level, bullet, and numbering format
- `text.ts` splits `Composition` into a schema and a `composition` reader; rebuild as one class
- Build paragraph, character, object, table, and cell styles and groups with `basedOn` chains and next style
- Build automatic GREP styles, nested and line styles, drop caps, bullets and numbering, and tabs as one `Composition`
- `Composition` belongs to a style, a text range, and the document text defaults
- Agents must build complex automatic GREP styling in plain calls, with no fragile logic in the library
- Automatic GREP styles stay apart from Find/Change GREP in `find.ts`
- GREP style is a `NestedGrepStyle` {`grepExpression`, `appliedCharacterStyle`} of a paragraph style
- Characters a GREP style matches read resolved attributes, keep `appliedCharacterStyle` [None], and split no `textStyleRanges`

### [05]-[PAGES_AND_PARENTS]

- `PageView` and `SpreadView` read offset, side, extent, parent, applied parent, section, margins, columns, and item counts
- Build page counts, ids, applied parents with inherited items, per-page size, sections, guides, margins, columns, and spreads
- Reorder and resize pages through `page.move` and `page.resize`

### [06]-[COLORS]

- Build swatches, colors, tints, gradients, mixed inks, color groups, and document versus application swatches
- None, Registration, Paper, and Black carry no brackets, and `remove()` throws 90898 "This color cannot be deleted."
- Swatch and Color hold no `removable` or `editable` property, and `Reference.hostOwned` tests brackets, missing those swatches
- New documents hold Black and Paper alone; InDesign refuses changes to Registration and Paper, and color work adds swatches

### [07]-[SETTINGS]

- `DocumentView` reads intent, facing pages, default size, ruler units and origin, preflight profile, and text defaults
- Build preference objects as typed views and writes: document, view, text, grid, transparency, and script preferences
- Measurements stay in points under `Hold`, which sets `scriptPreferences.measurementUnit` for each call

### [08]-[LINKS_AND_GRAPHICS]

- `LinkView` reads status, path, URI, type, needed, edited, size, date, and placed parent
- Build relink, update, unlink, placed graphics, fitting, and effective ppi

### [09]-[PREFLIGHT]

- `Preflight` returns issues under one profile with profile names
- Build profiles and rules as views, process, and report beside the per-mutation summary and the `changed` stream

### [10]-[STRUCTURE]

- Build conditions, notes, footnotes, tables, and cells
- Build one method-call verb over metadata rows for operations `change`, `create`, and `remove` cannot express
- Method calls decode arguments by parameter type, one verb in place of a tool per method

### [11]-[OTHER_APPLICATIONS]

- Illustrator, Photoshop, After Effects, and Acrobat run `execute_script` and `command` through their dictionary commands
- Photoshop UXP extension reuses `@rasm/uxp` as its own contract, panel, and product on the InDesign pattern
- Atelier decides which peer answers which verb

## [03]-[METHOD]

- Agents are fresh Opus agents, never forks, and delegate to at most 4 or 5 agents when work splits without churn
- Briefs point to standards, files, and sources, and state expectations in place of minutiae
- Agents deliver declarations justified by host or package facts, then consumers' rebuilt shapes, then code, then checks at zero
- Reports name each removal and each kept construct with the fact that keeps it
- Domain libraries serve future extensions unrelated to MCP, and no domain module reads a tool schema or imports its consumer
- Rebuilds change source and consumers in one chain to less code, surface, fields, and parameters, never a shuffle into new types
- One declaration per concept (a `Schema.Class` owning schema, reader, operations), algebra before classes, tagged state over strings
- Options flow through schemas end to end, labels derive from operation and target, and no call takes a written description
- Nesting yields to better upstream logic, with no extracted helper, wrapper, forward, alias, exported constant, or deferred note
- Declarations earn their place by a fact no other states; an operation whose answer another contains goes, its callers rebuilt
- Effect 4 and installed packages (MCP, typesetting, Adobe types) are used in full, and a missing package joins at its newest release
- Live probes confirm facts the sources state, never replace reading them
- Tools are verbs over a category; a tool with no value goes, and its value joins a remaining verb
- Facts every result needs (status, preflight) arrive at one composition point, never through agent polling
- Defects met outside scope (tooling, watcher, Swift capture) get a focused agent at once
- Adversarial reviews assume code is wrong and return findings only, each with the rebuild that removes it
- Reviews cover folder flatness, file and function names, overlapping names, coined terms, flow, wrappers, and indirection
- Reviews cover exported constants, large export lists, and deep nesting
- One agent applies one review's findings, in dependency order, sequential on shared files, and pushes back on a wrong finding
- Checkers reach zero before a commit; commits and pushes wait for the owner's word
