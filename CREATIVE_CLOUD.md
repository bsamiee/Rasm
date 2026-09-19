# Creative Cloud

This is the remaining-work plan. Current source and native readbacks decide completion; the retained app-specific documents in `plan/design/` describe native behavior and design intent. Research and input assets remain in `plan/research/` and `plan/inputs/`. The superseded generic application, design-system, repository and server plans are consolidated here.

## Current checkpoint

- Illustrator: resource replacement and cleanup, exact Spot ink, artboards, RTL, guide planning, CAD normalization and graphic-style compilation are implemented. Native CAD/style, artboard, colour and RTL checks passed. Conflicting nested Spot imports now reject without changing target resources, including after save/reopen; exact CMYK/Lab inspection also passed. Text geometry remains a draft.
- InDesign: native constructor/enum generation, editorial operations, eight public library tools, publishing operations, a public grid operation, enum resources and the typed snapshot viewer are implemented. Native query range isolation, exact change counts, undo and preference restoration passed. Preflight profile export/import, Unicode paths and English dictionary export passed; embedded-profile rollback and the remaining readbacks are open.
- Photoshop: the nine existing tools are retained, with type-style application, shared layer composition and drift comparison added. Source checks and adversarial review passed; the new workflow handlers are not yet native-verified.
- Acrobat: 30 public capabilities are implemented. Review defects in page geometry, output identity, metadata, partial failures and Unicode transport were corrected. Native Unicode replies and input literals now round-trip exactly.
- Shared: canonical colour definitions and ASE compile/read tools are integrated. Native shortcut parsing/compilation is implemented without public tools yet. Drift comparison and pixel fingerprints are integrated into Illustrator, Photoshop and Acrobat readers.
- Typography: the existing TypeScript library owns font inspection through HarfBuzz, OpenType Builder, Fontkit and the WOFF2 decoder. The temporary Python package, worker transport and HarfBuzz patch are removed. Grid solvers and native geometry application are integrated. Font parity, physical source/variation binding and final template readbacks remain open.

## Remaining tasks

### Integration and deployment

- [ ] Remove the final UXP declaration patch through a correct owning dependency or equivalent native lifecycle contract. Ghidra confirms the current package merges configuration and API interfaces incorrectly; the older official package also misdeclares the consumed contract. Module startup alone loses scoped native cleanup. Do not replace this with casts, dummy configuration, declaration shims or a moved patch.
- [ ] Finish the generator correction for native measurement unions. Ghidra confirms both JavaScript metadata and SDEF erase mixed structured types while retaining a native-generated type clause; decode that contract at the existing boundary and prove query measurement writes/readback.
- [ ] Finish native deployment and host readbacks against the integrated server checkpoint. InDesign deployment now selects the exact discovered Beta installation and reconnects successfully. Rebuild after the latest font, grid, file-boundary and blank-panel changes, then finish the native checks below.
- [ ] Verify the eight public InDesign library tools and snapshot viewer through actual calls. Existing snapshot capture is shared. Confirm the new Illustrator CAD/style/guide/menu tools and palette tools through public calls.
- [ ] Prove bounded deployment failure and continued operation with the panel closed. Diagnostic panel text is removed; both plugins have deployed and attached, and Photoshop startup attached with its panel closed. Finish the remaining InDesign panel/lifecycle readbacks after the current source changes. Repository-managed deployment replaces the old standalone launcher/updater.
- [ ] Complete process-wide ownership for whole Illustrator and Acrobat operations. A compound operation must remain serialized across clients, including every native step. Prove native recovery after disconnect, late completion, duplicate requests and rejected attachment.
- [ ] Replace the custom deployment MCP framing/protocol adaptation with the official SDK client in the existing owner. Reconcile the final diff with the original tools and current public surface; compare code at equivalent functional coverage, finish scoped diagnostics and finished-file review. Keep root documentation and tool descriptions accurate; do not introduce a new README, harness or compatibility layer.

### Illustrator

- [ ] Finish text geometry and expose it only after correct logical range-to-glyph mapping. Preserve partial selections, linked and justified area text, mixed styles, RTL/vertical/transformed text, whitespace, custom glyph height, anchors, dashes and zigzag behavior.
- [ ] Complete native guide checks after the planner rebuild: first-only text conversion, artboard-only scope, clipped/compound bounds, rotated copy anchors, all Bento modes, hero scale/centering, isometric spacing, invalid targets and degenerate/extended edges. Confirm the actual character-range selection shape.
- [ ] Expose and verify cleanup operations: empty-selection document scope, nested clipping groups, inside-out cropping and dependent failure short-circuiting. Resolve opacity-mask Clip through a safe native dispatcher.
- [ ] Finish the uncovered original actions: parameterized personal/long shadows, B&W outline/offset/pathfinder/style/group compositions, selected/used-colour collection, unused-swatch removal and saturation. Preserve selection/document variants and original options.
- [ ] Complete explicit ownership and resolution of conflicting named Spot dependencies during gradient import; the current boundary rejects them. Finish native resource readbacks, repeatable mixed-model Spot creation and already-open destination ownership. Implement true Pattern replacement through a supported native route.
- [ ] Add explicit gradient-table compilation and native library readback. Complete template assembly with shared styles, layer taxonomy, guides, metadata, type styles, bound libraries and sheet content; preserve the current template/catalogue behavior.
- [ ] Derive the complete current command registry, including commands absent from factory shortcut files. Keep direct native command execution available while inventory is incomplete. Use the native canvas extent where required; artwork bounds are different data.
- [ ] Compare the final tool surface with the current Illustrator Beta native MCP and retain meaningful added capability without losing the original tools' functionality.

### InDesign and typography

- [ ] Normalize vendor presets into the existing grid solvers and apply structured/preset grids to native documents. Cover every recovered mode, unit, margin mode, fit scope, unequal column, subdivision, secondary grid, image-line rule, parent/facing-page scope, lock, bleed and slug. Verify actual written geometry.
- [ ] Finish font source and variable-instance identity, generated PostScript names, authoritative axes, collection members and all required metrics. Preserve provenance, source/name-table digests, physical member identity and independent failures across native, Adobe Fonts, Typeface Beta, parametric_forge and premium/free Drive collections. Complete native activation and deployed binding; family/style-name equality alone is insufficient. Select multiple principled Latin/Persian/Arabic premium pairings from those actual sources.
- [ ] Prove editorial direction, numbering continuation, special characters, RTL defaults/styles and text/GREP operations. Check range isolation, real change counts, undo, preference restoration and partial failures. Implement Reverse Layout only through a proven native operation.
- [ ] Verify all eight library jobs: asynchronous UXP file representation, Unicode paths, grouped item/style IDs, anchored placement, snippet round trips, export maps, layer order, borrowed handles and partial asset failures. Establish completion semantics for Book/Library close.
- [ ] Finish contextual snapshot fidelity: parent/layer transforms, overrides, threaded text, dynamic variables, transparency, shadows, pasteboard regions and rotated clipping. Preserve original artifacts and correct preview dimensions, density, colour, alpha and orientation. Correct layout items and guides to the same native page coordinate space, independent of caller ruler origin and page transforms; prove pagination.
- [ ] Verify offline enum resources and the snapshot viewer through the actual MCP Apps bridge, including CSP, lifecycle, fit, pan, zoom and resize. Keep the presentation in typed DOM code.
- [ ] Finish flexible text fitting and reflow: width preservation, sibling movement, nested/master overrides, a common rational leading lattice, optical anchors distinct from containment, physical glyph minima and variable-instance remeasurement. Prove Latin/Persian/Arabic marks, digits, baselines, heading rules, captions and footers.
- [ ] Complete meaningful composition families: dark/image overlays, full-bleed/banded pages, agenda/kicker, two-column body, asymmetric galleries and diagrams. Verify resize and saved/reopened geometry with actual content.
- [ ] Complete technical sheets: designer/revision/management/project/sheet fields, production strips, module addresses, standard cap heights, competition anonymity and panel identity. Derive physical units and bleed from the sheet standard.
- [ ] Implement functional content where only resource definitions currently exist: data-merge fields/data, forms and tab order, linked TOC/index/cross-references/hyperlinks, conditions and variables, notes, anchored objects, repeating table headers, content-sized rows, RTL tables, Articles and accessibility metadata.
- [ ] Finish coherent application defaults with no document open and verify inheritance after restart. Land the intended document preset, palette, notes, variables, conditions, dictionary/word-list and style/glyph resources through their real native owners. Native text-default assignments do not create undo history. Persian has no installed dictionary provider; current Lilak source is prepared, but supported Hunspell registration, restart and readback remain.
- [ ] Complete publishing policies and native readbacks: preflight rule visibility, clone/update/rollback, process completion, saved queries, PDF review behavior, tagged output, effective profiles, PDF/printer presets, individual joboptions and import/export persistence. Changing a PDF comment's status does not apply its edit. Execute the intended whitespace and Persian character cleanup policies.
- [ ] Verify all produced templates and catalogue rows: fonts, resources, dimensions, margins, columns, gutters and baselines; INDT opens as a new unsaved document. Build books from successful outputs and synchronize shared resources without overwriting size-specific typography.
- [ ] Finish the outstanding native Grid Calculator experiments needed to settle unresolved preset semantics and distinguish recovered behavior from deliberate improvements.

### Photoshop

- [ ] Run the actual type-style and layer-composition handlers after MCP startup is stable. Verify hierarchy, source-layer identity, native units, colours, clipping, blend/opacity, locks, transaction behavior and failure cleanup.
- [ ] Complete template construction using those existing workflows, caller-owned content and native document presets; verify save/reopen rather than adding a second layer builder.
- [ ] Complete native preset/library inventory and supported writers/readers for swatches, gradients, brushes, patterns, shapes and styles, preserving groups and identity. Resolve the observed preset descriptor failures.
- [ ] Complete the remaining render-pass and look workflows, including Camera Raw and Color Lookup, with actual native descriptors and result readbacks.

### Acrobat

- [ ] Finish bookmarks, structured page text, template/data-driven spawning, batch printing, portfolios and dynamic stamp-library construction. Resolve installed stamp category/appearance/anchor identity beyond raw appearance names and rectangles.
- [ ] Implement prefix-only page labels through a native PDF API that can express them; the JavaScript bridge cannot.
- [ ] Complete preflight-library compilation with dependency-preserving native packages and import/readback. Verify positive fingerprint/audit lookup using a document with a real embedded audit trail.
- [ ] Implement Distiller discovery, native conversion and output/error handling. Complete an actual conversion after normal macOS Automation authorization is available.
- [ ] Complete caller-owned typed user/machine preferences and cab values, persisted snapshots and comparison, and native export/settings behavior.
- [ ] Complete privileged folder-script deployment and the remaining Action Wizard command/settings coverage and production policies, including Bates numbering. Keep interface arrangement outside this server's document/resource operations.
- [ ] Correct remaining form-depth gaps: same-name radio widgets and dependency-ordered calculated totals. Preserve existing field/tab-order/accessibility behavior.
- [ ] Verify live XMP/Info precedence, namespace-preserving metadata writes, staged multi-file/HTML export, rotated-page links, source-preserving production output and installed stamp behavior. Finish actual converter-settings ownership rather than inferring settings from output files.

### Shared compilers and drift

- [ ] Finish native-generated/imported ASE comparison for Lab scale and Gray polarity. All 40 bundled Adobe ASE files already round-trip byte-for-byte through the unpatched declarative codec. Complete ACO and native Photoshop swatch/gradient synchronization plus typed InDesign Color/ColorGroup synchronization.
- [ ] Integrate native CLR read/write with ICC validation, component/model checks and archive readback. Define explicit handling for Adobe Spot/global/tint/group information that CLR cannot represent.
- [ ] Expose shortcut compile/read/write tools, explicit set naming and native application readback. Obtain host-derived identities and insertion support for unassigned InDesign/Photoshop commands absent from the supplied set.
- [ ] Integrate the proven native CSF serializer in an isolated process with typed settings, ownership and failure cleanup; then synchronize and read back colour policy through the existing host operations.
- [ ] Add InDesign baseline comparison and generalized Acrobat preference snapshots. Verify first-baseline/unchanged/changed/unreadable outcomes through each real reader and canonical palette/image readback, with one native read per request.

### Native interfaces and portable assets

- [ ] Finish the Illustrator, Photoshop, InDesign and Acrobat interface passes: preferences, functional extension panels, docking, toolbars, shortcut sets, native workspaces and startup profiles. Preserve useful third-party panels, including Camera Raw/TK9 and Mockup/Retype; close unwanted assistant/promotional/Home surfaces without hiding stock commands. Use the existing native operation or UI that owns each setting and read it back after relaunch.
- [ ] Assemble actual house libraries through the implemented compilers: profile-aware palette ramps/contrast variants, premium type pairings, stroke/dash/arrow families, drawing/render/document layers, architectural symbols/hatches/brushes, register fields, XMP identity and deliverable-specific exports. Bind these resources into templates; a serializer alone is not a completed library.
- [ ] Preserve each host's native deliverables: Illustrator swatches/gradients/styles/symbols/patterns/brushes/type styles; Photoshop swatches/gradients/brushes/patterns/shapes/styles/tool presets/actions/looks/type styles; InDesign style graphs/snippets/title blocks/registers/variables/queries/glyph/dictionary resources; Acrobat stamps/preflight/actions/production resources.
- [ ] Finish the shared digital canvas and size catalogues, reference-derived presentation/report/portfolio/board/booklet/social families, technical sheets and bilingual layouts. Exercise current CAD normalization, render compositing, data merge, issue sets, review packs, bilingual export and drift on owned content; extend their existing owners rather than adding parallel builders.
- [ ] Present the Software Related Assets cleanup/portability proposal for approval before changing Drive. The full read-only inventory contains 16,688 files in 369 directories. Blank AI pages contain real swatches; My Color Palette, My Default Profile and the older INDT contain substantial resources. Three exact standalone duplicates total 735.65 MB. Preserve valuable assets, review historical scripts as assets, and complete unresolved Photoshop brush/CSF readback. After approval, organize native exports with source/provenance and restoration instructions for another machine; do not promote old backups to sources of truth.

### Design decisions

Recorded decisions remain inputs, not universal code constants: Accent/Ink/Field = Persian Blue/Gunmetal/Seasalt; multiple premium type pairings; digital-first sRGB IEC61966-2.1 with embedded profiles preserved, Adobe ACE, relative colorimetric and black-point compensation. Print conversion belongs to output. The default digital design is 3840 × 2160 at 72 ppi, Photoshop 16-bit, InDesign non-facing WEB_INTENT, with a 45 px module and three-module side margins. Font sizing derives from measured faces. Keep stock menu commands and save workspaces without menu customization.

Resolve only the still-unanswered policies when their deliverable needs them:

- Coordinated print profile/output intent/PDF standard, Distiller defaults, the single review-downsampling stage, and page labels versus printed sheet numbering/placement.
- Acrobat Persian paragraph/digit/ligature defaults, OCR languages and actual page/ruler units; local versus cloud tagging and the proven structure/Alt repair route.
- Authorized Acrobat Enhanced Security/privileged folders/menu JavaScript, plus notification/Home/session/tab/autosave/dialog preferences not already recorded in the app references. Reconcile Protected Mode with actual authorized state rather than retaining contradictory old instructions.
- House stamp statuses/dynamic fields/symbol categories, document permissions, redaction patterns/overlays, folder-menu commands and optional human-triggered Action Wizard coverage. Keep capability parameterized until policy is chosen.
- Actual need for geospatial assets, PDF viewport measurement scales, and bookmarked combined review PDFs versus portfolios. Do not implement guessed native capabilities.

## Research needed for the remaining tasks

- Illustrator's styled RTL SVG export truncates text. PDF retains complete glyph geometry; named Spot markers preserve exact identities, but paint ownership does not determine logical text clusters. RGB marker values are quantized even with colour conversion disabled.
- Illustrator's factory shortcut files provide 655 distinct menu spellings across installed locales, including the internal commands already used. They omit newer commands. Third-party UXP cannot access Adobe's restricted menu-registry context, and nested action playback has crashed this Beta.
- Native Pattern replacement exists internally; a supported callable bridge has not been established. This Beta also permits some scripted edits to locked artwork, so a universal locked-item rejection would be invented behavior.
- InDesign metadata capture must run in UXP and serialize its result inside that runtime. The installed edition exposes 640 of 651 declared constructors and 195 owned static values. ExtendScript inspects a different namespace.
- InDesign Document close has an observed completion event; Book and Library close are scheduled. Native operations are responsive after restarting a host stuck in menu tracking. Book idle completion and Flex/preflight behavior still require explicit readbacks. An exit code alone proves neither completion nor cleanup.
- InDesign static fonts throw on direct variation-property access; their supported-property snapshot omits those fields. Variable-font snapshots expose the full axis group. PDF preset `fullName` resolves asynchronously to a UXP file whose `nativePath` preserves Unicode; stringifying that property is incorrect.
- Native Flex text-leaf AUTO does not grow, and legacy text autosizing is forced off; fitting can do nothing or alter both axes. Native PDF bleed is limited to 432 pt, so it cannot capture arbitrarily distant pasteboard content. Saved copies also change filename/path variables.
- Vendor preset application establishes Main-before-Custom precedence, image-mode aliases and count-based type-area application. Blank values are not zero; saved counts and per-parent settings must survive. Quick subdivisions, some leading relationships and missing-key behavior still need resolution.
- Adobe ASE excludes gradients, patterns and tints. AppKit's native CLR archive preserves ICC data, but malformed-profile validation must use an API that rejects invalid profiles. Its name equality differs from normalized Swift string equality.
- Photoshop colour-settings Save is unavailable. Adobe's native ACE serializer independently round-tripped all 26 CSF payloads and changed only the requested setting; retaining metadata is necessary. The app setter does not immediately persist the current CSF.
- Acrobat JavaScript converts page-label styles to strings and cannot pass the native null style required for prefix-only labels. Check-only preflight does not create an embedded audit trail. The internal library-management object is unavailable in this runtime; a complete library export remains necessary.
