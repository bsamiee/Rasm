# [APPLICATIONS]

Hosts apply declared rows to each desktop application through its own stores.

## [01]-[ARCHIVE]

One `.archive/` in the interface folder holds knowledge of every application:
- `facts/` holds one text file per topic, one fact per line, and `facts/index.txt` names each file's topic, product, version, and form
- `decompiled/` holds managed assemblies and native binaries as source, `inventories/` store dumps and factory baselines
- `color/` holds the generator rule of every hue family
- Facts state the member, key, value, order, limit, or answer a row uses, a paraphrase or general claim stays out
- Facts record the installed build alone, older versions, their differences, and their guidance stay out
- Citations, source paths, line numbers, URLs, versions, provenance, narration, and coined terms stay out of fact lines
- Session, agent, decision, and task ids, transcript and temporary paths, work dates, interface code references, reports, plans, and notes stay out
- Work reads `facts/index.txt`, then each facts file on its topic
- Archived facts are used as written, a missing fact is read from the application before any row uses it
- New facts join the facts file that owns their topic, and a fact found wrong is corrected in place

## [02]-[STORES]

Every setting is written through its owning store and API:
- Settings every user of a file needs go in the template or startup document (units, layers, styles, render quality, environment, sun, ground, views)
- Settings one user keeps across files go in the application store (layout, navigation, keys, compute device, libraries, updates, dialogs)
- Settings take one declaration in one store, a document key overrides its application duplicate
- Rows read each value and write on difference alone
- Rows report each written value as read before the write and as written
- Reruns that report no change show every write persisted, a write the application dropped reports again
- Reports are typed rows the host decodes into outcomes, one protocol for every application
- In-application code writes its report on the error path, and a run with no report fails
- Settings a native object holds in memory take writes through its API, a key write beside it is overwritten at quit
- Key writes with no API reach their native object at the next idle save, and runs flush settings queues before any quit
- Settings with no owner API take a stored-file edit after quit, a file edited while the application runs is lost
- Files the application rewrites at quit (settings, layout, toolbar) converge whole, declared rows alone leave stray entries
- Rows keep factory-equal values, and the declared state holds from any prior state
- Stores that drop default-equal keys at quit take a factory-equal value through a live owner API, a file edit there changes the file every run
- Steps write a setting before the settings it constrains, an excluded value resets with no error
- Settings that raise a dialog in a run, at close, or at quit (clipboard, file lock, missing font, save prompt) take the suppressing value
- Update checks, repository sync at startup, and telemetry take their off value, license and server settings take no row
- Promotional, AI, tip, and suggestion features take their off value wherever a store reaches them
- Settings a platform mechanism replaces (an autosave timer under system document versions) take no row
- Members with no reader in the installed source take no row
- Rows declare the intended value, a row that offsets a defect elsewhere goes and the defect is fixed at its source
- Content libraries (assets, materials, textures, environments) join through the application's own library integration as declared rows
- Library rows point at a folder holding content and its catalog, an empty or absent library takes one skip row naming the folder
- Published libraries join as their publisher's remote library, fetched on use, and applies build only assets the interface authors
- One-time cleanups (a stale add-on record, stored panel state, a run leftover) run once with the application quit and leave no row
- Scripted runs turn preference auto-save at quit off and save once at their end, auto-save persists a failed run's partial writes
- Embedded interpreters keep imported modules between runs, and each run evicts the shared modules and writes bytecode under the host's cache prefix

[PROCESS]: Hosts launch, wait on, and quit each application through the operating system:
- Hosts take an application's bundle from its `mise.toml` `[env]` path row, else from its bundle ids through Spotlight
- Launches take no focus (`open -g`, an application's no-focus flag), a focused new window takes the user's keystrokes
- Runs act through the application's API and post no OS pointer or key event, activation, or window raise to the user's session
- Launches open straight into a document with no splash or start window, a start window holds no document for a run to act on
- Readiness comes from an event the application sends back
- Quit events return before the process exits, and a run reads their effect from process exit alone
- Hosts release untitled documents and quit every running instance before a run, a titled document with unsaved edits fails the run
- Runs launch each application once to write
- Hosts reopen files open at discovery when a run ends, cancellation included
- Processes alive at the deadline after a quit are terminated
- Runs leave no residue in the application (autosave, history line, temporary document, run file)

## [03]-[EVIDENCE]

Facts come from the application's source, decompile, documentation, stores, or pixels:
- Behavior (poll order, event routing, draw and scale formulas) comes from source at the installed tag
- Managed code is read through a decompiler, native code through `use-ghidra`
- Documentation of the installed release decides over older posts and community sources
- Every store the application reads or rewrites at quit is dumped whole and diffed against a factory-startup instance before a row is written
- Store dumps follow the property metadata recursively
- Factory values come from a factory-startup instance, a property's declared default misreports them
- Enum values come from their enum type, a dynamic enum's valid set from the callback or registry that supplies its items
- Keys an import reads come from the loader's code or a round-trip export, a key no code path reads takes no row
- Built-in variants of one family (themes, display modes, templates) are diffed key by key, a varying key is a decision and a constant key a default
- Writability comes from each setter's source or decompile, a stub or schema declares setters the application ignores
- Settings only a physical pointer reaches rest on their source evidence
- Temporary values restore in the call sequence that set them, a camera or selection restored in a later call is lost
- Code sent to a shared process raises no modal (a license check-out, a missing table index), a modal holds every caller's interface thread
- Reads in a live process name each member, a reflection walk over a class crashes it
- Members that crash the process join an unreadable list every later reader follows, their setting goes through its stored key or stays unwritten
- Background instances report factory or last windowed values for window-bound state (scale, pixel size, keymaps), a windowed instance reads them
- Captures target one window by id at 1:1 device pixels without activation, a window capture shows what a pipeline capture omits (clipping, chrome)
- Captures follow a redraw in a later call, a screenshot in a view-changing call records the previous frame
- Colors, widths, and sizes come from row and column profiles of a 1:1 capture, its bytes read through one tool under its ICC profile
- Comparisons read drawn values against their roles and against their counterpart in the other applications
- Line and fill visibility is judged by L* difference on the drawn ground, contrast ratios are recorded
- One apply writes a change whole, and confirmation is a visual pass over a 1:1 capture, a rerun that reports convergence confirms nothing
- Passes crop and measure each touched strip for gaps, misalignment, and cut text
- Passes refine a shared value per application where one application needs its own
- Live reads resolve doubts, a doubt left as a note is a defect

## [04]-[FILES]

One folder holds every application's interface, one module per concept, and no file holds one type or one value:
- Shared facts sit once in root modules in-application code imports
- Values join a shared module only when every application reads an equivalent member, a value one application reads stays in its application
- Runtimes that cannot import shared modules take their values from the host as arguments, colors as 0-255 channels
- In-application code imports the standard library, its application's API, and bundled packages, and parses at its interpreter's version
- Tables and values derive from the application's API (property metadata, enums, public defaults, bundled files) wherever it supplies a fact
- Built-ins and maintained add-ons that hold a behavior replace own code
- Declared files take the format and extension their application reads and writes (alias export, skin, rule set, manifest)
- Declared files exist where the application reads a file or where one replaces an inline table
- Files state a key only when it takes effect and differs from the value its omission imports, identity keys (a mode id, a parent) stay declared
- Rendered and compared files keep the application's encoding, markers, and number spelling, a reformatted value compares unequal
- Declared files name color roles as placeholders an apply renders, the committed file holds no color literal
- Elements in layout and toolbar files resolve by id and owning file, a name or position match breaks across files
- Manifests hold required keys and optional keys a consumer reads, code reads the extension id from its manifest
- Extensions reach their application as the package its own build writes, installed on difference
- Declared state holds no machine path, build number, install folder, version literal, or one display's measurement
- Values the application reports (version, configuration folder, repository, template path, window extent, scale) are read at run time
- Ids derive from the API or a compiled table, an id copied from one machine drifts
- Names avoid words the application or standard library uses
- Hosts create the folders they write
- Prerequisites no run creates sit once in the setup reference of each application's driving skill
- Decisions sit in code as values and names it acts on, facts sit in the archive, and no comment, note, or memory file restates either
- Staged packages go under `.cache/<app>/`, run outputs (captures, logs, reports, built packages) under `.artifacts/<app>/`
- Unsaved work renders into `.artifacts/<app>/renders/`, the application's default output folder
- Saved projects render beside themselves through the application's relative path form

## [05]-[LAYOUT]

Every application takes one frame, one size scale, and one place per role:
- Layouts are written through the application's own layout model (workspace, dock, region, and toolbar files or APIs) in its units
- Each application expresses a role through its own constructs and names, a mode, view, or panel named after another application's construct goes
- Workspaces take one set of task names and one order in every application
- Applications holding one workspace keep its stock name
- Workspaces exist for a distinct task, workspaces that differ by one strip merge
- Workspaces inside one application share one frame, where each editor keeps one region, size, and order
- Frames hold a full-height left column (tools, code, console), main editors in the center over lower editors, and a full-height right column
- Right columns hold selection properties on top and document tree at the bottom
- Docked strips span the region they serve, a strip running under a side column cuts it short
- Timelines appear only in workspaces where time drives the task
- Panels, containers, tabs, ribbons, and toolbars take one place and relative order by role in every application
- Toolbar tabs order by role (general and selection, creation by geometry kind, editing and transform, drafting and output, display and view)
- Plug-in commands and panels join the stock tab and column of their role, a plug-in's own tab stands only where no stock tab takes them
- Tab and panel names, stock and plug-in alike, are declared wherever a store renames them, each a full-word name for what the tab holds
- Toolbars and ribbons declare their appearance (button size, padding, tab style)
- Editors show one header row, a control appears once
- Ribbon and workspace tabs show text, sidebar tabs show icons
- Lists, file browsers, asset views, and panels show list views
- Columns are decided for visibility, order, and width, and a stretch column takes the remaining band
- Pop-up and context menus hold commands used most on the current selection
- Command options show inline at the prompt, a floating options dialog goes
- Stock panels keep the application's order, an add-on's reordering is undone
- Add-on panels open collapsed with their header shown and keep their add-on's identity
- Workspaces show every enabled add-on except one that only other workspaces place
- Asset panels hold one asset kind each, docked in the editor with a task that takes the kind

[SIZES]: Every region, strip, flyout, popover, floating panel, and dialog a user can open is sized for its content and opens at its declared size:
- Interface scale is the smallest that keeps text legible and lines one device pixel wide
- Icons, rows, thumbnails, and palettes take the smallest size offered that keeps labels legible
- Icons take one size per strip family, equal to the tab icons beside them
- Sizes convert with the application's own scale at run time, a fraction of one window or display drifts
- Truncated labels and empty bands mark a wrong size

[TYPOGRAPHY]: Faces are declared once beside color roles, each as a file in the user font folder and a family name:
- Interface text and command prompts take the interface face where an application lets it be chosen, code and consoles take monospace
- Every text role takes one weight, interface text the smallest legible size at interface scale, tooltips one size below it
- Applications taking a file path get the file, applications taking a family name get a family their font API resolves

## [06]-[INPUT]

Navigation and bindings follow one rule in every application, mouse and trackpad alike:
- Right drags and two-finger swipes orbit a perspective view and pan a plan, elevation, or camera view
- Wheel zoom takes one ratio per notch, one event per detent
- Where one button opens a menu and navigates, the menu opens on a still click and a drag navigates
- New bindings are read against stock items on the same key and modifiers
- Extension bindings register with the extension and unregister with it, user keymaps hold edits of stock bindings alone

[KEY_FAMILIES]: Commands group by what they do into families on left-hand keys, and pointer hands stay on the mouse:
- First keys name the family
- Single keys run the family's most-used command, a family with none opens at its second keys
- Second keys follow the letter rows (Q W E R T, A S D F G, Z X C V B) and rank a series from simple to complex
- Planar families list their solids on the second keys
- Digits select variants of the single-key command, a doubled key its natural sibling (line and polyline)
- One suffix key draws a family's single-key command vertical to the construction plane
- Named series take mnemonic suffixes (length, area, volume), opposite actions take adjacent keys (hide and show)
- Macros join their extended command's family, scripts stay out of key families
- Families with a single-key command matching a stock key's command (rotate) keep the stock key
- Applications without a command line take a leader key that keeps every stock key
- Leader prompts list family names in the status bar, then aliases matching a typed prefix, each grayed outside its acting state
- Enter or Space runs a typed alias, the last alias on an empty prompt
- Alias rows resolve commands registered when the leader opens

[ALIASES]: Each application binds the shared alias keys to its own commands:
- One writer replaces each alias table whole
- Macros take the application's dialog-free, locale-independent command form
- Commands and option forms come from the application's command list
- Macros that pause for a pick block the scripting server that sent them
- New alias names follow the family rule and collide with no command name, factory alias, or existing alias
- User rows win over a factory alias they collide with
- Shortcuts write one key at a time through their owner, a whole-table rewrite drops bindings the reader omits

## [07]-[COLOR]

One role module computes one palette of named hue families and a neutral scale on shared lightness steps, and applications read colors through its roles:
- Hue families seed from a named color
- Roles needing a missing palette color take a new named family or step
- Neutrals are pure grays at the chrome bytes applications fix, and every application's chrome takes neutral steps
- Elements without a meaning take the neutral scale, any hue on screen marks a meaning
- Hue separation is measured in normal vision alone, color-deficiency separation is recorded
- Fills are flat, gradients, gloss, embossing, drop shadows, and zebra rows go wherever a setting removes them
- Chrome shows no operating-system tint, an application-scoped setting removes it
- Icons take the modality color of their category wherever a store reaches them, other monochrome icons their label's color
- Multicolor icons draw gray at rest and in color on hover wherever a setting desaturates them
- Corner radius, line weight, icon style, and text weight hold one value per application and match across applications

[ROLES]: Roles name meanings by category, then role, then state:
- Each role names one primitive once and holds it on every ground, and roles sharing a step each name it
- Settings take the role their meaning names, read from each member's draw path in source, and a row sorts by where it draws
- Meanings no role names add a role at the palette step for their use, one step or more from ground and neighbors
- Document ink (layer, print, and new-layer colors) is black, screen ink (display members outside black-to-white switching) white
- Roles, alphas, and faces are literals over palette steps, ink raw bytes that paper and shadow take
- New applications map every color member of their theme, skin, and settings stores, members no reader draws take their matching sibling's role

[DEPTH]: Regions separate by lightness alone, darker reading farther back:
- Depth runs recess, frame, well, panel, box, field, with canvases on the well step below every body drawn on them
- Lists, trees, tables, consoles, history, and code sit in a well below their panel, headers and grouped boxes above it
- Bodies holding controls take the panel step
- Regions meeting without a line differ by 8 L* or more, closer steps meet at a header, a divider, or a straight edge
- Dividers between editors and containers are gaps on the frame step, pop-ups and menus sit on it inside a border edge
- Canvases draw screen ink with black-to-white switching on, layout sheets draw document ink on light paper

[STATES]: Each control class orders its states rest, hover, pressed or checked, selected, active, each apart from its neighbors in lightness first:
- Hover lifts a fill one step on its own scale and a tab one step above its strip, a neutral hover stays apart from every accent fill by hue
- Editable, clickable, and read-only separate by edge and fill: entries a fill with a border, buttons a fill flush with their edge, read-only no fill
- Control edges stay flush with their fill in every state, an edge the application derives from other members is solved to draw flush
- Field value states (animated, keyed, driven, overridden, changed) replace the rest fill, 2 steps apart in one family where they share an editor
- Disabled drops its fill and lowers its label to the disabled step
- Disabled canvas bodies keep their fill under a veil of their own ground at an alpha that puts their label on the disabled step

[ACCENT]: Chrome takes the operating system accent's hue family, canvases take their own:
- One accent family holds rows, tabs, pressed and checked controls, indicators, links, and focus, and no accent step draws on a canvas
- Accent steps rise with state order: animated fields, selected rows, active tabs and keyed fields, active rows and pressed controls, indicators
- Focus and checked boxes take the solid step, selected names and carets one step above it
- Canvas selection (geometry, nodes, wires, keys) and its gizmos take teal, 0.08 or more apart in OKLab from every stock selection color
- Canvas selection reads by chroma under screen ink: bodies step 6, inactive items 8, items 9, the active and hovered item 11
- Selection on geometry stops at step 11, its step 12 draws only as a gizmo's hover and the active node's border
- Node borders outrank their body in lightness: neutral 11 at rest, selection 11 when selected
- One solid accent line marks focus alone: focused list, edited field, default button
- Editors that follow the pointer draw no outline
- Active tabs fill with the active step behind a primary label, checked tabs open into their page on its step
- Tab sets that cannot fill their tabs draw one solid bar
- Pressed and checked controls fill, an active tool is a pressed control, and an active command shows by a fill behind its icon
- Hover, pressed, and selected states draw no accent line
- Text on an accent fill is primary text

[HUES]: Hues outside the accent and selection families, modality colors, and tags hold one meaning each in every application:
- Feedback while a command runs takes one hue alone: tracking and snap marks and placed bodies at step 11, the active point at step 12
- Persistent construction (document guides, symmetry axes, normals, analysis hairs, notes) takes a quiet hue near the accent's complement
- Grids an application draws behind canvases, panels, lists, and timelines take neutral steps
- Datum grids a user creates take a dark hue apart from axis hues through a color setting the grid alone reads, a shared setting keeps its role
- Geometry a definition computes and has not baked takes the display modality's mark over a neutral shaded body
- Handles, control polygons, crosshairs, and camera frames take neutral step 11, locked geometry neutral step 8
- Axis triads, errors, warnings, and success take their own hues
- Edit marks take step 11 of their hue
- Gold, brown, olive, sand, and muddy mixed hues stay out, dark steps of a warm family included
- Accepted hue families sit 0.08 or more apart in OKLab at their solid step, the accent and selection families included
- Text contrast is a legibility floor, text and ground above it keep their palette steps

[MESSAGES]: One color per severity (error, warning, info, success) in every application:
- Bodies and borders a message tints take one role, and where no role reaches a message its native mark stands
- Warnings take a light solid under a dark glyph, the one badge that reads by inversion
- Info is the quietest badge, a lighter step there breaks a badge under a light glyph
- Dimming is reserved for disabled, preview-off lifts the body one step, and disabled and preview-off share no mark

[MODALITY]: Modality colors are the one categorical set an application assigns by type:
- Node headers, zones, icon roles, and ribbon tabs share one modality palette, each hue the socket color an application fixes for its data type
- Modalities in the preview, construction, and warning hues (display, analysis, color) draw neutral bodies and headers under a step 9 mark
- Code tokens take step 11 of a modality hue, comments the disabled step

[TAGS]: Tags are the one categorical set a user assigns:
- Slots keep the order applications number them in, each slot one solid, and a tagged item selected or active takes canvas selection steps
- Slots sit 0.08 or more apart in OKLab from each other and the accent, selection, axis, error, construction, tracking, preview, and datum grid hues
- Every color slot an application stores takes a tag, the neutral tag, or document ink
- Layer color pickers offer document ink, then the tag set

[ALPHA]: Opacity is a role property naming what it composites over:
- Alphas sit once in the role module as fractions, each application converts channels in its own code and rounds to what it stores
- Hue borders sit at step 8 of their family, and alpha belongs to overlays that show what lies beneath
- Applications that drop alpha take each role pre-blended over the ground it draws on
- Lines drawn wider than their table width take the alpha that keeps energy per line (width times step distance) equal to a line at table width
- Members an application mixes into a derived color (hover lift, axis blend, grid lift) are solved in their row for a drawn color equal to their role
- Theme colors an application stores as bytes are written as byte-exact fractions, a float that is not byte-exact rereads changed every run
- Scene-linear members take their role converted through the application's transfer curve, display-encoded members take its byte

[NATIVE]: Colors no setting reaches are recorded as an application limit, neighbors take the nearest palette step, and no role changes to match them.

## [08]-[LINES]

One style per category in every application, widths in device pixels (logical at zoom 1 on node canvases), dashed only where a convention dashes it:

| [INDEX] | [CATEGORY]                             | [ROLE]                           | [WIDTH]                | [PATTERN]                      |
| :-----: | :------------------------------------- | :------------------------------- | :--------------------- | :----------------------------- |
|  [01]   | Curves, angled edges, creases, borders | Document ink by layer or object  | 1                      | Solid or the assigned linetype |
|  [02]   | Silhouettes and outlines               | Screen ink                       | 1                      | Solid                          |
|  [03]   | Isocurves, mesh wires, smooth edges    | None in modeling views           | None                   | None                           |
|  [04]   | Hidden lines                           | Screen ink                       | 1                      | Dashed                         |
|  [05]   | Section boundary                       | Document ink by object           | 1                      | Solid                          |
|  [06]   | Section fill                           | Section surface                  | Fill                   | Solid                          |
|  [07]   | Hatch lines                            | Document ink by object           | 1                      | Pattern's own                  |
|  [08]   | Linetypes                              | Document ink by layer            | Linetype width         | At the declared resolution     |
|  [09]   | Dimensions and annotations             | Layer tag, document ink in print | 1                      | Solid                          |
|  [10]   | Defect indicators, off by default      | Error                            | 1                      | Solid                          |
|  [11]   | Edit marks                             | Seam, sharp, crease, bevel       | 1                      | Solid                          |
|  [12]   | Minor grid                             | Grid at the minor alpha          | 1                      | Solid                          |
|  [13]   | Major grid                             | Grid                             | 1                      | Solid                          |
|  [14]   | Axis lines X and Y, Z off              | Axis triad                       | 1                      | Solid                          |
|  [15]   | Tracking and snap feedback             | Tracking                         | 1                      | Solid                          |
|  [16]   | Construction guides                    | Construction                     | 1                      | Solid, center lines dash-dot   |
|  [17]   | Drawing feedback                       | Screen ink, tentative tracking   | 1                      | Solid, tentative dash-dot      |
|  [18]   | Control polygons and handles           | Handle                           | 1                      | Solid                          |
|  [19]   | Crosshair                              | Handle                           | 1                      | Solid, full length             |
|  [20]   | Camera frame and safe areas            | Handle                           | 1                      | Dashed                         |
|  [21]   | Measure and relationship lines         | Screen ink                       | 1                      | Dashed                         |
|  [22]   | Gizmos                                 | Axis triad, gizmo selection      | 2                      | Solid                          |
|  [23]   | Selection window                       | Selection stroke and fill        | 1                      | Window solid, crossing dashed  |
|  [24]   | Selection on geometry                  | Selection item and active        | Line's own             | Solid                          |
|  [25]   | Node wires                             | Neutral 11, selection            | Items 1, collections 2 | Stock per structure and state  |
|  [26]   | Node outlines and paper edge           | Node border                      | 1                      | Solid, no emboss               |
|  [27]   | Freehand note strokes                  | Construction                     | 3                      | Solid                          |
|  [28]   | Chrome dividers                        | Frame                            | 1                      | Solid                          |
|  [29]   | Grid on lists and timelines            | Panel grid, neutral              | 1                      | Solid                          |
|  [30]   | Datum grids a user creates             | Datum grid                       | 1                      | Application's own              |

Sheets print cut lines 0.35 mm, projection, annotation, center, and grid lines 0.25 mm, fine and hidden 0.18 mm, in the layer print color.

[DISPLAY]: Display modes and line stores draw each table category one way in every application:
- Widths below an application's minimum draw at its minimum, recorded in the archive, and other applications keep table width
- Previews drawn in another application's view take the host view's widths
- Wires take one color for every data type, a casing in the canvas color where a store reaches it, and selection at half alpha
- Width scales that thin a line below one device pixel go
- Points draw one width in every application, each dot member solved from its own draw formula for the shared width
- Display modes pass role bytes unconverted, the render path owns tone mapping, gamma, and texture linearization
- Modeling modes draw shaded surfaces with edges, silhouettes, and object transparency, wireframe is a mode the user picks
- Modeling modes override assigned materials with a neutral shaded surface under a headlight, without shadows, cavity, or specular highlight
- Rows set a drawing store's selecting usage flag first, then every color its store keeps takes its role, routed or not
- Modes drawn on paper route every line to fixed document ink where black-to-white switching follows the application canvas
- Per-object line widths stay honored, a pixel override that flattens them goes
- Lines that look alike and serve different functions keep separate categories

## [09]-[TEMPLATES]

New documents start from a template or startup document:
- Displays show imperial with metric configured and correct beside it (templates, unit switch, alternate units, precision)
- Declared lengths are round imperial values stored in meters, a round metric literal shows as an odd imperial value
- Quantities with no imperial form (a lens length, irradiance) keep their native unit
- Unit-bearing settings (tolerances, grid, snap, nudge, precision, clipping, scales, paper, annotation sizes) derive from one declaration per system
- Tools that draw a dimension agree on one fractional precision, derived from one resolution
- Unit grammars and displays no setting reaches are recorded as an application limit with the input forms that parse as intended
- Annotation sizes are paper cap heights, multiplied once by the sheet scale in model space and drawn at paper size in layout space
- Dimension terminators, string spacing, and first offset derive from text height
- Unit systems are written system first, then each unit token of the system, a system write resets its tokens
- Application settings a document reads in its units (nudge, empty size, import scale) follow the open document's system through a document-open hook
- Unit switches rewrite settings and leave document content in place
- Tables a command reads (hatches, linetypes, annotation styles) are read on a fresh document before a step edits them, superseded entries go
- Current table entries name a template entry, set from the index its add call returns, a missing index raises a modal
- Template steps write through an API reaching every template table
- New documents open with every view in the declared display mode, and their modeling view targets origin at a declared plan distance
- Startup documents hold no object every task deletes, add-on log, or secret
- Templates and startup documents start every project with declared layers in the application's own layer construct beside its default container
- Declared layers keep their order and tag slot, object kinds routed to a layer reach it through the application's default target setting
- New objects go in the container an application creates under its own name
- Organization names are domain terms interchange keeps, one per level and the same in every application
- Definitions from another file insert embedded by default, a link is chosen per insert
- Commands that ungroup or remove objects from a set leave every object in the document

## [10]-[RENDER]

Each application declares the render, sun, location, and material settings one shared output needs:
- Render settings (frame size, sample limit, noise threshold, pixel density, caustics) hold one value in every application
- Photometric values (exposure, sun irradiance, sky radiance) come from one calibration render of one scene per application
- Location and moment come from a survey record and a fixed clock time, the machine's location and clock stay unread
- Sun stores take the standard offset and a daylight flag, and north conventions convert per application to one sun vector
- Add-on fields with update callbacks that rewrite related fields are written as raw items, a callback chain overwrites the declared values
- Materials use the metallic-roughness model every application shares, built through each material editor's typed content path
- Data textures read without color conversion, color textures with display encoding, set where their material is built
- Normal maps share one convention across applications
- Texture size belongs to mesh coordinates and material repeat, a projection computed at render time survives no exporter
- Texture sets and environment images sit once in a shared folder every application builds its native materials from
- Look-development light is one declared environment image, named by its file and staged from its publisher's download while absent
- Native formats hold structure, interchange formats hold materials, units, axes, and layers, and parameters no format holds sit in the archive

## [11]-[EXTENSIONS]

Extensions stay by daily use and key on stable ids:
- Dropped plug-ins and add-ons are uninstalled with their files and user data, a dropped core add-on is disabled
- Plug-ins and add-ons load on demand, at startup only when a startup task uses them
- Add-ons that start a GUI toolkit on import register a background instance as the running application, and load on demand or go
- Staged archives take third-party add-on fixes and role colors through `packages.toml` `patches` rows before packing
- Rows for an optional plug-in or add-on key on its manifest id or registered name and run while enabled, a download folder name keys no row
- Packages install on a difference of version or files, a build that reuses one version string differs by files alone
- Applies converge each package on the build its staged archive records, `rasm:upgrade` alone stages a newest or first build
- Own extensions hold behavior every session needs (navigation, panel collapse, aliases), the apply run holds one-time settings and installation

## [12]-[RHINO]

- Rebuilt plug-ins reach Rhino through `rasm:upgrade`, then an apply
- Window layout restores apply live, and the live layout persists to the containers store at quit
- Runs that open a panel to measure it reselect the exported tab, layout exports record a selected tab and report a change otherwise
- Icon size keys cache at load, a layout measured in the run that writes them sees old strip sizes and converges at relaunch
- Toolbar, tab, panel button, osnap, and filter icons have clamped size keys, the gear, page icons, status bar icons, and viewport tabs none
- Toolbar button pitch is the image size plus twice the button padding
- Tab strips size tabs from the tab icon size, keep their gear while tabs fit before it, and overflow into a chevron, fitted counts derive from both
- Osnap and filter bars lay out as one row at a small height or a width that fits the row, their control grid exists under grid geometry alone
- Viewport tab bars belong to the viewport column, status bar and resizer heights are fixed, and a bottom band derives from its stack minus both
- Command history line pitch follows prompt font size, prompt colors drop at small sizes, and a band shows whole lines at a height the pitch divides
- Grid column widths derive from header cell size and the widest template cell text in grid fonts, icon columns keep factory width and a blank header
- Docked panels, tabs, status bar, tooltips, and the sidebar prompt draw the system small font with no key
- Captions, viewport tabs, menus, and dialogs draw the system font with no key
- Eto style flags resize whole font families, and command prompt size, in tenths of a point, is the one keyed text size and reaches history alone

## [13]-[GRASSHOPPER_2]

- Editor chrome draws fixed Eto standard fonts from the system label font with no setting, and canvas text belongs to the document

## [14]-[BLENDER]

- Tool rows are whole pixels, widget unit times row scale truncated then times zoom, and toolbar zoom derives from the whole row
- Status bar and shelf header heights are fixed, shelf height follows preview size and name display, and its row count changes by a user drag alone
- Interface text is the widget, panel title, and tooltip styles in points drawn through system scale
- View scale sizes rows and lines with the text and is no text size control, editor, console, and text object sizes are no interface text
- Icon-only tool buttons up to twice the widget unit wide are fixed-size items at any row scale, wider ones split the row evenly as free items
- Toolbar region widths snap to drag stops scaled by zoom, and one stop alone holds a second tool column
- Panel side margins follow interface scale with no setting
- Adjacent separators left by filtered tools each open an empty block unless the layout generator collapses them
- Instances launched with event simulation skip the idle sleep, drop every OS input event, and spin one core for the whole run
- Area sizes take the area split operator at an exact factor
- Region widths and toolbar zoom have no API and take a startup document edit after quit, read back exactly at launch
- Scripted quits through `wm.quit_blender` write a compressed `quit.blend` over the previous one in the temporary directory
- Incremental autosave writes a compressed copy of a titled dirty file on the main thread at each interval, and its interval keeps the factory value

## [15]-[ILLUSTRATOR]

- Panel text is a fixed theme size times a scale factor with display-dependent slider stops applied at relaunch, and no font size choice exists
- Tool panel width, cell size, side padding, and glyph box are fixed with no setting

## [16]-[INDESIGN]

- Panel text is a fixed theme size, titles and dialogs larger, times an interface scaling slider held in the defaults record with no scripting member
- Story editor text size is code text and no interface text

## [17]-[PHOTOSHOP]

- Native panel text follows the font size option, applied at relaunch and read back through its small size member
- UXP panels draw a fixed host font and the options bar has no text size key
- Tab groups cut labels past their column width, a group holds the tabs with labels that fit and other panels join the icon column

## [18]-[ACROBAT]

- Preference domains hold no interface text size, a font size table written there is a cache and no setting
