# live-binding-controls — bedrock

## change-set to UI binding

- The binding seam is the last hop of a settled change-set pipeline: `Bind(out ReadOnlyObservableCollection<T>, resetThreshold = 25, useReplaceForUpdates = true)` materializes the stream into the collection screens consume; `BindingOptions` carries the same knobs explicitly.
- Replace-not-remove default: updates emit replace notifications so row identity, selection, and focus survive in-place edits — disabling it is reserved for controls that mishandle replace.
- Reset-threshold default: a single change-set batch larger than the threshold collapses to one reset notification — cheaper for virtualized panels than N granular events.
- Reset drops selection: bulk loads that must preserve selection lower the batch size or raise the threshold deliberately.
- Sorted binding is its own overload family over sorted change-sets with `SortAndBindOptions { ResetThreshold, UseReplaceForUpdates, UseBinarySearch, InitialCapacity, ResetOnFirstTimeLoad, Scheduler }`.
- `UseBinarySearch = true` is the large-list lever and is legal only when the sort key is immutable for a row's lifetime — a mutating key silently corrupts placement.
- `InitialCapacity` pre-sizes for known cardinality; `ResetOnFirstTimeLoad` turns the initial flood into one reset.
- Adaptor variants exist for both shapes (plain and sorted observable-collection adaptors) — a custom adaptor is the extension point for exotic targets, never a reason to hand-apply change-sets.
- Paged projections bind directly: the page-context-keyed change-set has its own `Bind` overloads, so page state stays upstream and the bound collection only ever holds the current page.
- Paging in the view layer over a fully bound list is the rejected form.
- `ObservableCollectionExtended<T>` with `SuspendNotifications()`/`Load` is the manual-load escape hatch for non-change-set sources; inside the suspension window mutations coalesce to one reset on dispose.
- Single-writer per bound collection is the invariant — the suspension hatch is never a side door to mutate a collection a change-set pipeline also binds.
- Rows owning resources dispose at the binding edge: `DisposeMany`/`AsyncDisposeMany` immediately upstream of `Bind` ties row lifetime to cache membership.
- Removing a row disposes its view-model; without the edge operator, replaced rows leak subscriptions sized by churn rate.
- Change-set mechanics (cache construction, filter and transform policy) arrive settled from the owning concurrency law; this lane owns only the projection-to-screen altitude — which overload, which thresholds, which collection type, and that the final hop lands on the UI scheduler seam.

## tree-flatten fold

- The projection union is closed — flat, tree, grouped, paged, windowed — and every member collapses to one flat keyed stream feeding one virtualized list seam.
- A dedicated tree control is rejected (the available tree-grid package remains an unadmitted transitive; the residency holds the line).
- The payoff is one selection model, one virtualization path, one keyboard table, and one binding seam for all five shapes.
- Tree shape: `TransformToTree(pivotOn, predicateChanged)` produces `Node<TObject, TKey> { Item, Key, Depth, IsRoot, Parent, Children }` where `Children` is itself an observable cache.
- The indent-row projection is `(Depth, Item)` flattened in visit order into the bound collection, with indent rendered as a margin bound to `Depth`.
- `Node` is disposable and the flatten owns disposal of retired nodes.
- Expansion is a predicate stream, not control state: `predicateChanged` filters which nodes materialize — expansion toggles push a new predicate derived from an expanded-key set.
- The expanded-key set is plain data the workspace can persist.
- Lazy children land by adding rows to the upstream keyed cache, which flows through the same pivot into new child nodes — there is no "load children" API on a control; materialization is cache membership.
- Grouped shape flattens the same way: group headers become synthetic rows interleaved with member rows in one stream — a header row is a union case of the row type rendered by template dispatch.
- Nested expander controls per group are rejected because they fork virtualization.
- Virtualization substrate: items controls virtualize through the virtualizing stack panel as the items panel; the data grid virtualizes rows natively; `ScrollIntoView` is the reveal verb on both.
- The flat-stream law exists so virtualization never recurses — a tree of nested panels defeats recycling and measures the whole tree eagerly.

## data-grid law

- Two shaping owners exist and exactly one may shape a given grid: the change-set pipeline (live sources, bound via the sorted/paged overloads) or `DataGridCollectionView` (in-memory snapshots).
- `DataGridCollectionView` shaping rows: `Filter` predicate, `SortDescriptions`, `GroupDescriptions`, `PageSize`/`PageIndex` with `MoveToPage`/`MoveToFirstPage`/`MoveToNextPage`, `MoveCurrentTo` for currency.
- Stacking both owners (a collection view over a change-set-bound collection) double-shapes and double-notifies; choose by source liveness.
- `DeferRefresh()` returns a disposable batch window — multiple descriptor mutations (sort + group + filter) coalesce to one rebuild on dispose.
- Descriptor edits outside a defer window each rebuild the entire view.
- Currency and page transitions are observable and vetoable (current-changing and page-changing events) — current-row-dependent screens observe currency rather than selection when the grid drives a master-detail pair.
- `Refresh()` is the full-rebuild verb — reserved for predicate-state changes the view cannot observe; calling it after descriptor edits double-rebuilds.
- Row editing is transactional through the view: `AddNew`/`EditItem`/`CommitEdit`/`CancelEdit` on the collection view pair with grid-level `BeginEdit`/`CommitEdit`/`CancelEdit`.
- The editing events (`BeginningEdit`, `CellEditEnding`, `RowEditEnding`) are the veto points.
- Commit routes the edited row back through the settled admission rail — the grid never owns validity, it surfaces it.
- The `Sorting` event intercepts sort requests — a column whose sort must ride a domain comparison (not property comparison) claims the event and applies its own descriptor; users still get header-click sorting, the domain owns the order.
- `AutoGeneratingColumn` is a column-policy fold, not a convenience: cancel suppresses a column, replacing the column instance retypes it.
- A typed row's column set derives from one policy table (property → column kind, header key, width) applied in this event; hand-declared column lists are reserved for grids whose shape diverges from the row type.
- Column sizing is a value vocabulary (`DataGridLength` with its converter — pixel, auto, star) declared per column row.
- The grid exposes its active `CollectionView`, so shaping state is inspectable from the grid without holding a second reference.
- `LoadingRow`/`LoadingRowDetails`/`LoadingRowGroup` fire on recycled containers — per-row decoration applied there must be idempotent and derived from the row item.
- Storing state on a row container is the canonical virtualization corruption.
- Selection is grid-owned state (`SelectedItem`/`SelectedItems`): the view-model observes selection as an input stream.
- Writing selection back is reserved for programmatic reveal paired with `ScrollIntoView`.

## editor factory rows

- Inspector editing is a factory-row dispatch: `ICellEditFactory` with `ImportPriority` (higher wins; the default sits mid-ladder so domain factories outrank built-ins by declaring higher), `Accept` (match by property context), `HandleNewProperty` (build the editor control), `HandlePropertyChanged` (refresh on external change), `HandleReadOnlyStateChanged`, value transport (`SetPropertyValue`/`GetPropertyValue`/`ValidateProperty`), and visibility propagation (`HandlePropagateVisibility`).
- A custom editor for a domain value type is one factory row registered into the factory collection — replacing the grid or forking per-screen editors is rejected.
- The inspector's gate-and-receipt pair (`CommandExecuting`/`CommandExecuted`) wraps every property operation — undo journaling and mutation receipts attach there once, not per editor.
- `CustomPropertyDescriptorFilter` is the admission point deciding which properties surface at all — the reflection layer is implementation, the filter event is the public shape.
- Inspector posture (`LayoutStyle`, `IsCategoryVisible`, quick-filter visibility, operation visibility, cell-edit alignment, expansion state) is declared properties, not subclassing.
- The inspector also exposes name rendering (`CustomNameBlock`), operation-menu shaping, and focus receipts (`PropertyGotFocus`/`PropertyLostFocus`) — focus receipts feed the same fact stream as command receipts, giving inspector telemetry without per-editor instrumentation.
- List-shaped properties edit through command surfaces (`ListEdit` new/clear, list-model insert/remove commands) — list mutation is command-driven with receipts, mirroring the suite mutation law instead of direct collection pokes.
- Bounded-choice editing has one editor per selection shape — checked list (multi-select), radio list (single-select), toggle-button group (mode-select) — so a bounded vocabulary renders by its selection arity, never by a hand-built panel of checkboxes.
- Scrub-versus-commit is a dual-channel contract: the previewable color picker (`PreviewColorChanged` vs `ColorChanged`) and previewable slider (`PreviewValueChanged` vs `RealValueChanged`) separate continuous preview from committed value.
- Preview drives cheap visual feedback; commit drives the actual mutation with receipt — binding the mutation to the preview channel floods the admission rail at pointer-move rate.
- Color editing is a typed model row-set: `ColorView`/`ColorPicker` with `Color`/`HsvColor` duals and `ColorChanged`; the `Hsv`/`Rgb` primitives convert both directions (`ToRgb`/`ToHsv`/`ToColor`/`ToHsvColor`), so color math never round-trips through strings.
- Editor posture is declared properties — alpha (`IsAlphaEnabled`, hex alpha placement), palette visibility and column count, spectrum shape and component axes — not subclasses.
- Palettes are an `IColorPalette` contract (`GetColor(index, shade)`) selected via `Palette` — a domain palette is one row implementing the contract; palette content is data, the control is fixed.
- `ToDisplayName` gives a human color label from a value — the display channel for swatch tooltips without a hand-rolled name table.
- Code editing binds a document model, never text properties: `TextEditor.Document` (`TextDocument` with lines, anchors, segments, `UndoStack`) is the state owner.
- `Text` round-trips whole content and resets editing state — incremental edits go through the document; `Load`/`Save` stream the round-trip.
- Caret and selection are model objects (`Caret`, `Selection` on the text area), so cursor position and selection restore are state assignments, not synthesized input.
- Durable positions are anchors, not offsets: `TextAnchor` tracks its position across document mutation with declared `MovementType` and `SurviveDeletion`, projects to `Offset`/`Line`/`Column`, and flags `IsDeleted`.
- Bookmarks, diagnostics markers, and fold anchors store anchors and check deletion before use; storing raw offsets across edits is the corruption form.
- Editing posture is options data: `TextEditorOptions`, `WordWrap`, `IsReadOnly`, `ShowLineNumbers` — posture is per-row configuration in an editor catalog, not editor subclasses; `ScrollTo` is the caret-reveal verb.
- Grammar binding is one installation session: `InstallTextMate(registryOptions)` returns the installation; `SetGrammar(scope)` selects by scope, `SetGrammarFile` loads ad-hoc, `SetTheme` swaps token colors, `TryGetThemeColor` exposes theme values for surrounding chrome, `AppliedTheme` signals, `Dispose` tears the session.
- The grammar registry resolves scope from file identity — `GetScopeByExtension`/`GetScopeByLanguageId`, with local-directory and local-file loading for custom grammars — so language support is registry rows, and a domain language is one grammar row plus one extension mapping.
- The cross-feature law: editor theme follows the application's resolved variant by calling `SetTheme` on the variant-changed edge — one fold, no per-editor theme state.
- The tokenizer reads through a snapshot pipeline (the editor projection and document snapshot types) and custom token-adjacent line transforms extend the generic line-transformer base.
- Per-keystroke manual recolorization outside the installation's transformers is rejected because it races the snapshot.
- Editor capabilities are managed installations scoped to editor lifetime: `FoldingManager.Install`/`UpdateFoldings`/`Uninstall` (manual `CreateFolding` for domain folds), completion via `CompletionWindow` over `ICompletionData` rows, the built-in `SearchPanel` overlay, and custom decoration via `DocumentColorizingTransformer`/`IBackgroundRenderer` registrations.
- Each capability is an install/uninstall pair — never subclass surgery.

## dialog and notification gating

- Modal state is host-addressable: `DialogHost` keyed by `Identifier`; static `Show(content, identifier, openedHandler, closingHandler)` returns `Task<object?>`; static `Close(identifier, parameter)` completes it.
- The awaited task is the dialog's typed receipt and the close parameter its result value.
- The overload matrix encodes addressing: content-only resolves the single host, identifier overloads select among hosts, instance overloads bypass lookup — multi-window suites address dialogs by host key, never by walking windows.
- Free-floating modal windows for in-app decisions are rejected; the host renders content through `DialogContentTemplate` against the same view locator as screens.
- Sessions are first-class: `DialogSession.UpdateContent` morphs an open dialog (progress to result without close/reopen); session `Close` ends it directly.
- `CurrentSession`/`CurrentSessions` plus `IsMultipleDialogsEnabled` and `Pop` give an explicit dialog stack; `GetDialogSession(identifier)`/`IsDialogOpen(identifier)` are the queries.
- `DialogClosingEventArgs.Cancel` is the veto point (unsaved input, in-flight work); `CloseOnClickAway` is per-host policy, not a per-dialog argument.
- Declarative open/close exists beside the static surface: `OpenDialogCommand`/`CloseDialogCommand` on the host bind from markup — the markup path and the awaited path are the same host, so a markup-opened dialog still lands in the session stack and the gating fold.
- Overlay presentation is host policy rows: `OverlayBackground`, `DialogMargin`, `BlurBackground`, and popup positioning through the positioner contract (centered and aligned implementations, constrainable variant for bounds-aware placement).
- Per-dialog presentation overrides are rejected; dialogs inherit the host's overlay law, and host chrome (corner radius, border brush) rides the style surface admitted once with the host's style resources.
- The notification suppression fold is one total function over (runtime-phase × degradation-level): terminal phases drop (nothing presents during teardown), capture phases queue (boot, drain, modal-exclusive moments), normal phases show.
- Resume edges flush the queue in arrival order with stale entries aged out by a declared horizon.
- The fold consumes settled phase and degradation vocabularies — the dialog lane contributes only the present/queue/drop verbs and the flush trigger; a notification call site never inspects phase itself.
- Dialog-versus-notification is a severity split in one presentation law: interaction-blocking decisions are dialog sessions (awaited, receipted); ambient facts are non-blocking presentations routed through the same suppression fold.
- Two presentation arms, one gating law — nothing user-facing escapes phase gating.

## gestures, key tables, pan-zoom

- Input adaptation is behavior composition, not code-behind: the behavior collection attaches to any element; trigger rows (key down/up, pointer entered/exited/moved/capture-lost, double-tap, got/lost focus, drag-enter/leave/over, drop) pair with action rows (`InvokeCommandAction` with `PassEventArgsToCommand`, throttle by `Interval`, debounce by `Delay`, async action groups).
- A gesture-to-intent mapping is one trigger+action row in markup; per-control event handlers are the deleted form.
- `DataTriggerBehavior`/`MultiDataTriggerBehavior` express state-conditional rows; observable-stream, timer, task-completed, and network triggers cover non-input causes through the same row grammar.
- System actions stay command-bounded: picker actions (open/save/folder with `FileTypeFilter`/`AllowMultiple` against the storage provider), clipboard read/write actions, file write/watch, and HTTP actions are behavior rows invoked through the command boundary.
- A behavior never becomes a hidden side-effect channel that bypasses receipts.
- Drag interactions are behavior rows too: context drag/drop pairs carry a typed payload handler, list-reorder is a dedicated behavior (orientation-declared), canvas drag covers free placement.
- Drop targets declare acceptance, mirroring the capability-row pattern rather than inspecting payloads in handlers.
- Responsive adaptation is behavior rows as well (adaptive and aspect-ratio behaviors) — size-conditional layout changes are declared conditions, keeping breakpoint logic out of measure overrides.
- The key table per surface: `InputElement.KeyBindings` is an ordered list of `KeyBinding { Gesture, Command, CommandParameter }`; dispatch is first-claimant-wins in list order, making conflict resolution a deterministic fold.
- One key table per surface scope (shell, screen, editor), built from the command-table gesture column, with the conflict fold rejecting duplicate gestures at composition time instead of shadowing silently at runtime.
- `HotKeyManager.SetHotKey` is the attached single-control variant for chrome buttons.
- Chord normalization happens at exactly one transform: `KeyGesture.Parse` admits chord strings; `PlatformHotkeyConfiguration` (from platform settings) supplies `CommandModifiers` plus canonical per-action gesture lists (copy/cut/paste/undo/redo/select-all and the text-caret families).
- Rows declare gestures abstractly ("platform-primary + key") and the transform substitutes the platform modifier.
- Display and persistence formats never cross: user-facing chord text renders through the platform format; the invariant form is the storage and comparison format — a stored platform-formatted string breaks round-trip on the next platform.
- Scope precedence is structural: key bindings fire within focus scope before text input consumes the key — editor-focused screens order tables so editing keys are claimed by the editor and only chord-modified keys escape to surface tables.
- The conflict fold encodes scope precedence as data, not runtime checks.
- One viewport owner per canvas: `ZoomBorder` owns the pan/zoom transform exclusively — verbs (`ZoomTo`, `ZoomDeltaTo`, `ZoomIn`/`ZoomOut`, `ZoomToLevel` with `GetNextDiscreteZoomLevel` as the discrete ladder, `ZoomToRectangle`, `CenterOn`, `Pan`/`PanDelta`, `BeginPanTo`/`ContinuePanTo`, `AutoFit`, stretch applications, `Rotate`/`RotateAt`/`SnapRotation`).
- Input policy is enum rows, not handlers: pan button, wheel behavior, double-click zoom, content bounds, resize behavior; gates (`EnablePan`/`EnableZoom`/`EnableGestures`) and constraint clamps (`EnableConstrains` with min/max zoom and offset bounds) are declared properties.
- Transition feel (`EnableAnimations`/`AnimationDuration`) and the zoom indicator (`ShowZoomIndicator`/`ZoomIndicatorPosition`) are likewise declared rows.
- Camera changes are events (zoom, pan, matrix, gesture, stretch-mode change) — the view-model observes camera state as a stream, the same shape as observing grid selection.
- A second transform source (a render transform set elsewhere on the same content) silently fights the owner — hand-rolled matrix manipulation on canvas controls is rejected.
- Viewport state is exportable and historied: `SaveView`/`RestoreView` named views with a deletable registry, `NavigateBack`/`NavigateForward` view history, `ZoomBorderState` + `ImportState` for typed persistence round-trip, `IsPointVisible`/`IsRectangleVisible` for reveal decisions.
- Camera persistence is a typed state row handed to the shell's workspace law, never scraped transform fields.

## divergent

- Projection-union collapse (binding-tree-flatten): the maximal form is one keyed cache per collection concern with the projection union as a declared mode value — flat folds sort, tree folds pivot+flatten with the expansion predicate stream, grouped folds group+interleave, paged folds page requests, windowed folds virtualise requests — all five terminating in the identical sorted-bind seam and one bound collection.
- Mode switches swap the operator chain, not the screen: the bound collection instance, selection model, and templates are invariant across modes — "view as tree / view as list / group by X" is one enum on screen state.
- Foreclosed: per-mode collections, per-mode list controls, selection loss on mode switch.
- Indent-row vocabulary as the universal hierarchy shape: because the flatten emits depth, root-ness, has-children, and expanded-ness alongside the item, every hierarchical surface — outline panels, grouped grids, nested inspectors — renders from the same row contract with different templates.
- Tree-ness is four fields on a row union; keyboard expand/collapse verbs are command-table rows mutating the expanded-key set — tree interaction gains availability gating and receipts for free, and expansion state persists through the workspace law as plain data.
- Editor registry law (editor-factory-rows): the three editor families share one registry shape — priority-ordered accept-matched factories (inspector cells), key-resolved registries (grammar scope by extension or language id), and contract rows (palettes).
- "An editor for value X" resolves through one dispatch: factory rows for structured values, document+grammar session for text values, with the preview/commit dual channel as the uniform mutation contract.
- A new editable domain type lands as one factory row plus optionally one palette or grammar row; no screen gains editor-construction code.
- Session-morph dialogs as workflow surfaces (dialogs-gestures): `UpdateContent` on a live session plus the awaited `Task<object?>` makes multi-step modal flows one session — step transitions are content swaps carrying accumulated state, the final close parameter is the whole flow's receipt, and the closing veto guards mid-flow abandonment.
- A wizard is a fold over step states inside one session, never a chain of dialogs with stitched intermediate results.
- Gesture-conflict fold as build-time proof: because key tables derive from command-table rows, the conflict fold runs at composition time over (scope × gesture) pairs and emits typed conflict receipts.
- Duplicate gestures within a scope, scope-shadowing of editor-reserved keys, and platform-modifier collisions (a chord normalizing onto a canonical text-action gesture from the hotkey configuration) are all detectable before a window opens — "why doesn't my shortcut work" reduces to reading the fold's receipts.
- Throttle/debounce placement law: rate-shaping belongs on the trigger row (throttled pointer-move action, debounced text-change action) so the command's execute effect stays rate-agnostic.
- Putting rate logic inside execute bodies couples intent rows to input physics and breaks reuse across surfaces with different physics.
- Pan-zoom composition with row selection: `ZoomToRectangle`/`CenterOn` paired with visibility queries give reveal-on-selection for canvas surfaces the same shape as `ScrollIntoView` for lists — one reveal verb per surface kind behind one intent row.
- "Go to item" works identically on grids, trees, and canvases, and the availability input is selection non-emptiness in all three.
