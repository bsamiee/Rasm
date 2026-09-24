# [APPLICATIONS]

Hosts apply a desktop application's settings, layout, input, colors, lines, templates, and extensions from declared rows, read back after a relaunch.

## [01]-[ARCHIVE]

Application interface folders hold a `.archive/` with the proven, durable knowledge of their applications:
- `facts/` holds one text file per topic, each fact with its evidence
- `decompiled/` holds managed assemblies and native binaries as source, `inventories/` store dumps and factory baselines
- `color/` holds color specimens and runnable map generators
- Session, agent, decision, and task ids, transcript and scratch paths, work dates, interface code references, reports, plans, and notes stay out
- Evidence is the application's own: source file and line at the installed tag, decompiled path, dated documentation URL, or probe and result
- Work in the interface folder starts with `tree <interface>/.archive -L 2`, then reads every facts file on the topic
- `rg -l <topic> <interface>/.archive/facts` finds the facts files on a topic
- Facts the archive holds are used as written, a fact it lacks is read from the application before any row uses it, never guessed
- New facts join the facts file that owns their topic once proven, and a fact proven wrong is corrected in place

## [02]-[STORES]

Every setting is written through the store and API that own it, and every write is proven by a read after the application relaunches:
- Rows read each value and write on difference alone, and a rerun after a relaunch reports no change
- Rows report the value before any write and the value read back, and a value that reads back unequal to its target fails the run
- Reports are rows (header, change, skip, measure, error) the host decodes into typed outcomes, one protocol for every application
- In-application code writes its report on the error path, and a run with no report fails
- Settings a native object holds in memory take writes through that object's API, a key write beside it is overwritten at quit
- Key writes with no API reach the native object at the next idle save, and the run flushes the settings queue before any quit
- Settings with no owner API take an edit of the stored file after quit, a file edited while the application runs is lost
- Files the application rewrites at quit (settings, layout, toolbar) converge whole, declared rows alone leave stray entries
- Rows keep factory-equal values, and the declared state holds from any prior state
- Stores that drop default-equal keys at quit take a factory-equal value through a live owner API, a file edit there changes the file every run
- Steps write a setting before the settings it constrains (display device before view transform), an excluded value resets with no error
- Settings that raise a dialog in a run, at close, or at quit (clipboard, file lock, missing font, save prompt) take the suppressing value
- Update checks, repository sync at startup, and telemetry take their off value, license and server settings take no row
- Settings a platform mechanism replaces (an autosave timer under system document versions) take no row
- Styles an application keeps per project or per object (a layer's plot weight, a note layer's thickness) take no row
- Members with no reader in the installed source take no row
- Library rows point at a folder holding content and its catalog, an empty or absent library takes one skip row naming the folder
- One-time cleanups (a stale add-on record, stored panel state, a probe leftover) run once by hand with the application quit and leave no row
- Scripted runs turn preference auto-save at quit off and save once at their end, auto-save persists a failed run's partial writes
- Embedded interpreters keep imported modules between runs, and each run evicts the shared modules and writes no bytecode

[PROCESS]: Hosts launch, wait on, and quit each application through the operating system:
- Hosts find an application by bundle id or by a configured executable matched inside its bundle, background instances excluded
- Launches take no focus (`open -g`, the application's own flag such as `--no-window-focus`), a keystroke the user types never reaches a new window
- Launches open straight into a document with no splash or start window, a start window holds no document for a run to act on
- Readiness comes from an event the application sends back, and a run that quits its application ends at process exit
- Quit events return before the process exits, and the effect is read from process exit alone
- Hosts quit every application one way: titled documents with changes saved, untitled ones closed unsaved, a process alive at the deadline terminated

## [03]-[EVIDENCE]

Facts come from the application's source, decompile, documentation, stores, or pixels, and a claim no read proves stays out of the declared state:
- Behavior (poll order, event routing, draw and scale formulas) comes from the source at the installed tag
- Managed code is read through a decompiler, native code through `use-ghidra`
- Documentation of the installed release decides over older posts, community sources are dated
- Every store the application reads or rewrites at quit is dumped whole and diffed against a factory-startup instance before a row is written
- Store dumps follow the property metadata recursively
- Factory values come from a factory-startup instance, a property's declared default misreports them
- Enum values come from the enum type, a dynamic enum's valid set comes from the error a rejected assignment raises, then the value is restored
- Keys an import reads come from the loader's code or a round-trip export, a key no code path reads takes no row
- Built-in variants of one family (themes, display modes, templates) are diffed key by key, a varying key is a decision and a constant key a default
- Writability comes from a live write read back per setting, a setter a stub or schema declares proves nothing
- Probes set, read, and capture through the application's API with the application in the background, with no synthetic input and no focus switch
- Settings only a physical pointer could prove rest on their source evidence
- Probes restore every value they set in the same call sequence that set it, a camera or selection restored in a later call is lost
- Probes that write use a scratch document, instance, or identity the probe opened and delete what they create
- Probe values differ from the default and from every neighboring case
- Code sent to a shared process raises no modal (a license check-out, a missing table index), a modal holds every caller's interface thread
- Reads in a live process go one named member per call, and a probe batch ends with a read of the crash-report folder
- Members that crash the process join an unreadable list every later reader follows, their setting goes through its stored key or stays unwritten
- Background instances report defaults for window-bound state (scale, pixel size, keymaps), sizes and keymaps are read in a windowed instance
- Captures target one window by id at 1:1 device pixels without activation, a window capture shows what a pipeline capture omits (clipping, chrome)
- Captures follow a redraw in a later call, a screenshot taken in the call that changed the view records the old frame
- Colors, widths, and sizes come from row and column profiles of a 1:1 capture, read as device bytes before any color profile applies
- Comparisons read drawn values, not stored members, against their roles and against their counterpart in the other applications
- Line and fill visibility is judged by L* difference on the drawn ground, contrast ratios are recorded and not required
- Doubts are settled by a live read, a doubt left as a note is a defect

## [04]-[FILES]

One folder holds every application's interface, one module per concept, and no file holds one type or one value:
- Hosts derive the application set from the tree, each application folder holds one apply entry the host calls and keeps its stages private
- Shared facts (color roles, typography, layout sizes, units, render parity, site) sit once in root modules every in-application runtime imports
- Values join a shared module only when every application reads an equivalent member, a value one application reads stays in that application
- Runtimes that cannot import the shared modules take their values from the host as arguments, colors as 0-255 channels
- In-application code imports the standard library, the application's API, and its bundled packages, and parses at its interpreter's version
- Tables and values derive from the application's API (property metadata, enums, public defaults, bundled files) wherever it supplies the fact
- Built-ins and maintained add-ons that hold a behavior replace own code
- Declared files take the format and extension the application reads and writes (alias export, display-mode file, skin, rule set, manifest)
- Declared files exist where the application reads a file or where one replaces an inline table
- Files state a key only when it takes effect and differs from the value its omission imports, identity keys (a mode id, a parent) stay declared
- Rendered and compared files keep the application's encoding, markers, and number spelling, a reformatted value compares unequal
- Color roles render into a temporary copy at apply time, the committed file holds no color literal
- Imports take the fresh path (delete, then import) when the live export differs from the rendered file, a second run imports nothing
- Keys a re-import or relaunch drops take a load path that holds them
- Elements in layout and toolbar files resolve by id and owning file, a name or position match breaks across files
- Manifests hold required keys and the optional keys a consumer reads, code reads the extension id from the manifest
- Extensions reach the application as the package its own build writes, validated in strict mode and installed on difference
- Declared state holds no machine path, build number, install folder, version literal, or one display's measurement
- Values the application reports (version, configuration folder, repository, template path, window extent, scale) are read at run time
- Ids derive from the API or a compiled table, an id copied from one machine drifts
- Names avoid words the application or the standard library uses
- Hosts create the folders they write
- Prerequisites no run creates sit once in the setup reference of the skill that drives the application
- Decisions sit in code as the values and names it acts on, evidence sits in the archive, and no comment, note, or memory file restates either
- Probes, captures, logs, reports, and built packages stay outside the folder, and run output goes to the repository's output folder

## [05]-[LAYOUT]

Every application takes one frame, one size scale, and one place per role:
- Workspaces share one vocabulary across applications, a native term stays where the shared name means something else
- Workspaces exist for a distinct task, workspaces that differ by one strip merge
- Workspaces inside one application share one frame, where the same editors keep the same region, size, and order
- Frames hold a full-height left column of sources and browsers, the main editors in the center, and a full-height right column
- Right columns hold the selection's properties on top and the document tree at the bottom
- Docked strips span the region they serve, a strip running under the side columns cuts those columns short
- Strips of time appear only in workspaces where time drives the task
- Panels, containers, tabs, ribbons, and toolbars take one place and relative order by role in every application
- Toolbar tabs order by role (general and selection, creation by geometry kind, editing and transform, drafting and output, display and view)
- Toolbar and ribbon content stays the application's own, appearance rows (button size, padding, tab style) stay declared
- Editors show one header row, a control appears once
- Tabs over a strip of tools show icons, workspace tabs show text
- Lists, file browsers, asset views, and panels show list views
- Columns are decided for visibility, order, and width, and a stretch column takes the remaining band
- Pop-up and context menus hold the commands used most on the current selection
- Command options show inline at the prompt, a floating options dialog goes
- Stock panels keep the application's order, an add-on's reordering is undone
- Add-on panels re-register with their whole subtree and open collapsed with their header shown
- Panel defaults reach only panels no region has drawn, and collapse runs before the first draw of each session
- Re-registered panels declare the owner id of the add-on that includes them, a panel with an empty owner passes every workspace filter
- Workspaces filter add-on types by owner id, an add-on stays in every workspace only when a pie row depends on a type the filter drops

[SIZES]: Every region, strip, flyout, popover, floating panel, and dialog a user can open is sized for its content and opens at its declared size:
- Interface scale is the smallest that keeps text legible and lines one device pixel wide
- Icons, rows, thumbnails, palettes, and panel text take the smallest size offered that keeps labels legible
- Icons take one size per strip family, equal to the tab icons beside them
- Regions take a tier by role: small for single-row strips, medium for side columns, large for main editors
- Roles keep their tier, edge (left, right, bottom), and size in every workspace and every application, stated once in logical units
- Sizes convert with the application's own scale at run time, a fraction of one window or display drifts
- Sizes the content decides (whole rows of a tree, a history strip, a prompt) come from the row pitch and row count
- Other sizes take multiples of 5 logical pixels, composite sizes reach the step through parts the application offers
- Captures prove each size, a truncated label or an empty band marks a wrong one

[TYPOGRAPHY]: Faces are declared once beside the color roles, each as a file in the user font folder and a family name:
- Interface text and command prompts take the interface face where the application lets it be chosen, code and consoles the monospace face
- Every text role takes one weight, at the smallest legible size for the interface scale
- Applications taking a file path get the file, applications taking a family name get a family their font API resolves, proven by a read of the face

## [06]-[INPUT]

Navigation and bindings follow one rule in every application, mouse and trackpad alike:
- Right drags and two-finger swipes orbit a perspective view and pan a plan, elevation, or camera view
- Wheel zoom takes one ratio per notch, one event per detent
- Where one button opens a menu and navigates, the menu opens on a still click and the drag navigates
- New bindings are read against stock items on the same key and modifiers
- Add-on items precede stock items in their keymap, an earlier press item consumes a stock drag
- Extension bindings register in the application's add-on keymap and unregister with the extension, user keymaps hold edits of stock bindings alone
- Add-on keymap items and user keymap edits register in separate event-loop passes, a single pass records the add-on item as removed
- Wrapped operators copy the stock item's properties after the name, a changed operator name resets an item's properties

[KEY_FAMILIES]: Commands group by what they do into families on left-hand keys, the pointer hand stays on the mouse:
- First keys name the family
- Single keys run the family's most-used command, a family with none opens at its second keys
- Second keys follow the letter rows (Q W E R T, A S D F G, Z X C V B) and rank a series from simple to complex
- Planar families list their solids on the second keys (E circle, EQ sphere, EW cylinder)
- Digits select variants of the single-key command, a doubled key its natural sibling (line and polyline)
- Suffix V draws the single-key command vertical to the construction plane
- Named series take mnemonic suffixes (length, area, volume), opposite actions take adjacent keys (hide and show)
- Macros join the family of the command they extend, scripts stay out of key families
- Families whose single key runs the same stock command (rotate) keep the stock key
- Applications without a command line open each family as a pie behind a leader key that keeps every stock key
- Pie slices follow second-key order, the single-key command and its variants at the top, a slice with no counterpart stays empty and keeps its place
- Pie rows show operators registered at draw time, and a row grays outside the state in which it acts

| [INDEX] | [KEY] | [FAMILY]                                | [SINGLE_KEY]  |
| :-----: | :---: | :-------------------------------------- | :------------ |
|  [01]   |   Q   | Curves and points                       | Line          |
|  [02]   |   W   | Polygonal primitives                    | Rectangle     |
|  [03]   |   E   | Round primitives                        | Circle        |
|  [04]   |   R   | Rotate, orient, mirror, and view        | Rotate        |
|  [05]   |   T   | Text, scale, and deformation            | Text          |
|  [06]   |   A   | Offset, boolean, arrays, and flow       | Offset        |
|  [07]   |   S   | Surfaces                                | Plane         |
|  [08]   |   D   | Dimensions and measurement              | Distance      |
|  [09]   |   F   | Cut and join                            | Trim          |
|  [10]   |   G   | Group, visibility, and guides           | Group         |
|  [11]   |   Z   | Zoom and views                          | Zoom window   |
|  [12]   |   X   | Extrude                                 | Push and pull |
|  [13]   |   C   | Copy, construction planes, and clipping | Copy          |
|  [14]   |   V   | Move, align, and select                 | Move          |
|  [15]   |   B   | Blocks                                  | Block         |
|  [16]   |   M   | Merge and match                         | None          |
|  [17]   |   L   | Layouts                                 | None          |
|  [18]   |   I   | Import and insert                       | None          |
|  [19]   |   P   | Purge and cleanup                       | None          |

[ALIASES]: Aliases come from the user's alias list, an alias outside it goes:
- Macros take the application's dialog-free, locale-independent command form
- Commands and option forms are verified against the application's command list before a row joins
- Macros that pause for a pick block a listener and take no dry run
- New aliases take a name by the family rule, checked against command names, factory aliases, and every existing alias
- User rows win over a factory alias they collide with
- Tables use the application's alias export format and converge the whole set through the one writer that replaces it
- Shortcuts write one key at a time through the owner, a whole-table rewrite drops bindings the reader omits

## [07]-[COLOR]

Colors come from one published color system with an untinted gray scale and a stepped scale per hue, read through one role module:
- Elements without a meaning take the gray scale, any hue on screen marks a meaning
- Surfaces are flat, gradients, gloss, embossing, drop shadows, and zebra rows go wherever a setting removes them
- Chrome shows no operating-system tint, an application-scoped setting removes it
- Icons are monochrome wherever a store reaches them
- Corner radius, line weight, icon style, and text weight hold one value per application and match across applications

[ROLES]: Roles name meanings, grouped by category (surface, text, line, accent, selection, field, guide, axis, status, tag), then role, then state:
- Each role names one primitive once and holds that value on every ground, and roles sharing a step each name it
- Settings take the role their meaning names, read from the member's draw path in source, and a row sorts by where it draws
- Meanings no role names add a role at the step the scale publishes for the use, measured against ground and neighbors before joining
- Primitives are a module generated from the scale's package, roles, alphas, and faces are literals
- New applications map every color member of their theme, skin, and settings stores, members no reader draws take the role of the sibling they match

[DEPTH]: Regions separate by lightness alone, darker reading farther back:
- Depth runs frame, well, panel, box, field, and the canvas as the lightest large surface
- Lists, trees, tables, consoles, history, and code sit in a well below their panel, headers and grouped boxes above it
- Bodies holding controls take the panel step
- Surfaces meeting without a line sit a glance apart, closer steps meet at a header, a divider, or a straight edge
- Dividers between editors and containers are gaps on the frame step, pop-ups and menus sit on the frame step inside a border edge
- Canvases sit at the darkest byte on which every application draws black lines black, with black-to-white switching off

[STATES]: Each control class orders its states rest, hover, pressed or checked, selected, active, each apart from its neighbors in lightness first:
- Hover is neutral, one step above the ground and apart from every accent fill by hue, a tab under the pointer lifts one step above its strip
- Editable, clickable, and read-only separate by edge and fill: entries a fill with a border, buttons a fill flush with their edge, read-only no fill
- Control edges stay flush with their fill in every state, an edge the application derives from other members is solved to draw flush
- Field value states (animated, keyed, driven, overridden, changed) replace the rest fill, a glance apart where they share an editor
- Disabled drops the fill and lowers the label to the disabled step
- Disabled canvas bodies keep their fill under a veil of their own ground at the alpha that puts the label on the disabled step

[ACCENT]: Chrome takes the hue family of the operating system accent, the canvas takes its own:
- One accent family holds rows, tabs, pressed and checked controls, indicators, links, and focus, and no accent step draws on a canvas
- Accent steps rise with state order: animated fields, selected rows, active tabs and keyed fields, active rows and pressed controls, indicators
- Focus and checked boxes take the solid step, selected names and carets the step above it
- Selection on a canvas (geometry, nodes, wires, keys) and the gizmos that act on it take a second family a glance apart from the accent
- Selected bodies take a low step, selected items a higher one, the active and hovered item the lightest
- One solid accent line marks focus alone: the focused list, the field being edited, the default button
- Editors that follow the pointer draw no outline
- Active tabs fill with the active step behind a primary label, a checked tab opens into its page on the page's step
- Tab sets that cannot fill their tabs draw one solid bar
- Pressed and checked controls fill, the active tool is a pressed control, and the active command shows by the fill behind its icon
- Hover, pressed, and selected states draw no accent line
- Text on an accent fill is primary text

[HUES]: Hues outside the accent and selection families hold one meaning each in every application:
- Magenta marks transient feedback while a command runs alone: tracking lines, smart points, snap markers, the body of an object being placed
- Quiet orange near the accent's complement marks persistent construction: document guides, symmetry axes, normals, analysis hairs, notes
- Handles, control polygons, crosshairs, and camera frames take the lightest neutral step
- Axis triads, errors, warnings, and success take their own hues
- Edit marks and code tokens take the text step of their hue
- Categories colored by type (icons, node kinds, strip kinds, socket links) take the neutral step
- Gold, brown, olive, sand, and muddy mixed hues stay out, dark steps of a warm scale included
- Accepted hues stay a glance apart from the accent, the selection, and each other in normal vision and every color-deficiency simulation

[MESSAGES]: One color per severity (error, warning, info, success) in every application:
- Bodies and borders a message tints take the same role, and where no role reaches a message the native mark stands
- Warnings take a light solid under a dark glyph, the one badge that reads by inversion
- Info is the quietest badge, a lighter step there breaks a badge under a light glyph
- Dimming is reserved for disabled, preview-off lifts the body one step, and disabled and preview-off share no mark

[TAGS]: Tags are the one categorical set a user assigns:
- Slots keep the order the applications number them, each slot a solid with its selected and active steps
- Slots leave the accent, selection, construction, and tracking hues and stay a glance apart from each other under every simulation
- Every color slot an application stores takes a tag, the neutral tag, or the geometry role
- Layer color pickers offer the geometry role, then the tag set

[ALPHA]: Opacity is a role property naming what it composites over:
- Alphas sit once in the role module as fractions, each application converts channels in its own code and rounds to what the application stores
- Applications that drop alpha take the role pre-blended over the ground it draws on
- Lines drawn wider than one device pixel take the alpha that keeps energy per line (width times step distance) equal to the one-pixel line
- Members the application mixes into a derived color (hover lift, axis blend, grid lift) are solved in their row for a drawn color equal to the role
- Theme colors an application stores as bytes are written as byte-exact fractions, a float that is not byte-exact rereads changed every run
- Scene-linear members take the role converted through the application's transfer curve, display-encoded members take the byte

[NATIVE]: Colors no setting reaches are recorded as the application's limit, neighbors take the nearest scale step, and no role changes to match them.

## [08]-[LINES]

One style per category in every application, drawn one device pixel wide where the application reaches it, dashed only where a convention dashes it:

| [INDEX] | [CATEGORY]                             | [ROLE]                       | [WIDTH]             | [PATTERN]                      |
| :-----: | :------------------------------------- | :--------------------------- | :------------------ | :----------------------------- |
|  [01]   | Curves, angled edges, creases, borders | Geometry by layer or object  | 1                   | Solid or the assigned linetype |
|  [02]   | Silhouettes and outlines               | Geometry                     | 1                   | Solid                          |
|  [03]   | Isocurves, mesh wires, smooth edges    | None in modeling views       | None                | None                           |
|  [04]   | Hidden lines                           | Geometry                     | 1                   | Dashed                         |
|  [05]   | Section boundary                       | Geometry by object           | 1                   | Solid                          |
|  [06]   | Section fill                           | Section surface              | Fill                | Solid                          |
|  [07]   | Hatch lines                            | Geometry by object           | 1                   | Pattern's own                  |
|  [08]   | Linetypes                              | Geometry by layer            | Linetype width      | At the declared resolution     |
|  [09]   | Dimensions and annotations             | Geometry by layer            | 1                   | Solid                          |
|  [10]   | Defect indicators, off by default      | Error                        | 1                   | Solid                          |
|  [11]   | Edit marks                             | Seam, sharp, crease, bevel   | 1                   | Solid                          |
|  [12]   | Minor grid                             | Grid at the minor alpha      | 1                   | Solid                          |
|  [13]   | Major grid                             | Grid                         | 1                   | Solid                          |
|  [14]   | Axis lines X and Y, Z off              | Axis triad                   | 1                   | Solid                          |
|  [15]   | Tracking and snap feedback             | Tracking, magenta            | 1                   | Solid                          |
|  [16]   | Construction guides                    | Construction, orange         | 1                   | Solid                          |
|  [17]   | Drawing feedback                       | Geometry, tentative tracking | 1                   | Solid, tentative wires dashed  |
|  [18]   | Control polygons and handles           | Handle, lightest neutral     | 1                   | Solid                          |
|  [19]   | Crosshair                              | Handle                       | 1                   | Solid, full length             |
|  [20]   | Camera frame and safe areas            | Handle                       | 1                   | Dashed                         |
|  [21]   | Measure and relationship lines         | Geometry                     | 1                   | Dashed                         |
|  [22]   | Gizmos                                 | Axis triad, gizmo selection  | 2, rotate rings 3   | Solid                          |
|  [23]   | Selection window                       | Selection stroke and fill    | 1                   | Window solid, crossing dashed  |
|  [24]   | Selection on geometry                  | Selection, active lightest   | Line's own          | Solid                          |
|  [25]   | Node wires                             | Geometry, selected selection | 1 logical at zoom 1 | Structure 3 on 1, stock dashes |
|  [26]   | Node outlines and paper edge           | Geometry                     | 1 logical at zoom 1 | Solid, no emboss               |
|  [27]   | Freehand note strokes                  | Construction                 | 3                   | Solid                          |
|  [28]   | Chrome dividers                        | Frame                        | 1                   | Solid                          |
|  [29]   | Grid on lists and timelines            | Grid well, grid panel        | 1                   | Solid                          |

Sheets print cut lines 0.35 mm, projection and annotation 0.25 mm, fine and hidden 0.18 mm, in the layer print color.

[DISPLAY]: Display modes and line stores draw the table's categories the same way in every application:
- Widths an application cannot draw at one device pixel are recorded as its minimum in the archive and matched by the other applications
- Width scales that thin a line below one device pixel go
- Points draw one width in every application, each dot member solved from its own draw formula for that width
- Display modes pass role bytes unconverted, the render path owns tone mapping, gamma, and texture linearization
- Modeling modes override assigned materials with the neutral shaded surface, under the head lamp and without shadows, cavity, or specular highlight
- Rows target the store that draws and set the usage flag that selects it, a value a flag routes elsewhere takes no row
- Per-object line widths stay honored, a pixel override that flattens them goes
- Lines that look alike and serve different functions keep separate categories

## [09]-[TEMPLATES]

Templates and startup documents hold the declared tables, views, units, and startup data:
- Displays show imperial with metric configured and correct beside it (templates, unit switch, alternate units, precision)
- Declared lengths are round imperial values stored in meters, a round metric literal shows as an odd imperial value
- Quantities with no imperial form (a lens length, irradiance) keep their native unit
- Unit-bearing settings (tolerances, grid, snap, nudge, precision, hatch and linetype scales, import scale) derive from one declaration per system
- Tools that draw a dimension agree on one fractional precision, derived from one resolution
- Unit systems are written system first, then each unit token of that system, a system write resets its tokens
- Tables a command reads (hatches, linetypes, annotation styles) are read on a fresh document before a step edits them, superseded entries go
- Current table entries name an entry the template holds, set from the index the add call returns, a missing index raises a modal
- Template steps write through the API that reaches every table the template holds, a round trip proves views and styles
- New documents open in the declared display mode in every view, and the modeling view targets the origin at the declared plan distance
- Startup documents hold no object every task deletes, no add-on log or secret, and add-on keys for unit rows alone
- New documents hold one container under the application's own name that receives new objects, and no project tree
- Saved states (views, visibility sets, work planes, positions, snapshots) belong to the project document
- Organization names are domain terms interchange keeps, one per level and the same in every application
- Definitions from another file insert embedded by default, a link is chosen per insert
- Commands that ungroup or remove objects from a set leave every object in the document

## [10]-[RENDER]

Render, sun, location, and materials serve one output, and each application holds the settings that output needs from it:
- Render parity members (frame size, sample limit, noise threshold, pixel density, caustics) hold one value in every application
- Photometric parity comes from one calibration render of one scene per application
- Location and moment come from a cited survey record and a fixed clock time, the machine's location and clock stay unread
- Sun stores take the standard offset and a daylight flag, and north conventions convert per application, proven by comparing sun vectors
- Add-on solar fields that chain updates are written as raw items, an update chain repoints the sun
- Materials use the metallic-roughness model every application shares, built through the typed content path the material editor uses
- Data textures read without color conversion, color textures with the display encoding, set where the material is built
- Normal maps share one convention across applications
- Texture size belongs to mesh coordinates and material repeat, a projection computed at render time survives no exporter
- Texture sets and environment images sit once in a shared folder every application builds its native materials from
- Interchange formats are proven by a scratch round trip each way that records what survives (units, axes, layers, materials)
- Native formats hold structure, interchange formats hold materials, and parameters no format holds are recorded with their probe

## [11]-[EXTENSIONS]

Extensions stay by daily use and key on stable ids:
- Dropped plug-ins and add-ons are disabled, a startup load no startup task uses moves to on demand
- Add-ons whose import starts a GUI toolkit register a background instance as the running application, they load on demand or go
- Rows for an optional plug-in or add-on key on its manifest id or registered name and run while it is enabled, a download folder name keys no row
- Disable and enable cycles free an add-on's saved preferences, every add-on preference row is written after one
- Own extensions hold behavior every session needs (navigation, panel collapse, pies), the apply run holds one-time settings and the install
- Hidden core add-ons stay enabled, a disable there holds for one session
