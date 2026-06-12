# shell-navigation — bedrock

## shell composition

- One `Application` root composes the entire suite of windows, tray presence, native menus, and resources; `AppBuilder.Configure<TApp>()` is the single boot fold.
- Backend selection is rows on the boot fold — `UsePlatformDetect` (probing native/Win32/X11), explicit `UseAvaloniaNative`/`UseWin32`/`UseX11`, `UseSkia` — and `StartWithClassicDesktopLifetime(args, shutdownMode)` is the one windowed entry.
- A per-host boot fork is rejected: every modality must land as a builder row, not a second entry path.
- `IClassicDesktopStyleApplicationLifetime` is the owned-window lifetime owner: `MainWindow`, the live `Windows` list, `Args`, and `ShutdownMode`.
- `MainWindow` is settable across the lifetime — primary-window swap (a bootstrap window yielding to the shell) is a lifetime write, not window juggling, and main-window-close semantics follow the current assignment.
- `ShutdownMode` is a closed three-value axis — `OnLastWindowClose`, `OnMainWindowClose`, `OnExplicitShutdown` — and defaults to last-window-close.
- A tray-resident or background-capable shell must flip to `OnExplicitShutdown` at boot or closing the last window silently terminates the process — the tray icon outliving its windows is the canonical trap.
- Main-window-close mode encodes "companions die with the primary" as lifetime policy — hand-wiring companion close handlers to the main window re-implements a mode that already exists.
- The controlled-lifetime contract brackets the process: `Startup` and `Exit` events plus `Shutdown(exitCode = 0)` — the exit code is the process's outermost receipt, so shutdown verbs carry a typed code derived from drain outcome rather than defaulting silently.
- `Startup` is the earliest point where `MainWindow` exists to assign; window construction before `Startup` races the lifetime — the composition root assigns `MainWindow` in the startup edge, not in the application constructor.
- `ShutdownRequested` is the first-chance OS-shutdown veto (session end, app-menu quit); cancelling the args is the only pre-drain interception point.
- Window `Closing` handlers fire too late to coordinate a suite-wide drain — the shell folds `ShutdownRequested` into the settled drain choreography; running teardown inline in the event handler is the rejected form.
- `TopLevel` is a per-surface service capsule, not a global: `Clipboard`, `StorageProvider` (lazily resolved with a no-op fallback), `FocusManager`, `Screens`, `InsetsManager`, and `RendererDiagnostics` hang off each `TopLevel`.
- `TopLevel.GetTopLevel(visual)` is the one resolution hop from any visual to its capsule.
- Caching capsule services statically is the defect: a floating dock window and the main window resolve different clipboards and focus managers — service access always re-derives from the active surface.
- `TrayIcon` attaches at application level through the `TrayIcon.IconsProperty` attached collection (`SetIcons(Application, TrayIcons)`).
- Each tray icon carries `Command`/`CommandParameter`, a `NativeMenu`, `ToolTipText`, `IsVisible`, and a `Clicked` event — the tray is a derived projection of the command vocabulary, never an independently wired surface.
- `TrayIcon` is `IDisposable`; disposal rides the lifetime exit, not finalization.
- `NativeMenu.SetMenu(AvaloniaObject, NativeMenu)` exports a menu model to the OS menu bar per `TopLevel`; `NativeMenu.GetIsNativeMenuExported(TopLevel)` reports whether the platform took the export.
- The export flag selects between two render targets of one menu model — OS bar or in-window managed menu; maintaining parallel native and managed menu definitions is the rejected form.
- One menu model type serves both exports: the same native-menu graph attaches to the OS bar and to tray icons — bar and tray menus are two placements of one model, never two menu definitions.
- Menu item `Click` events exist beside `Command` (`HasClickHandlers` discriminates) — the command path is law; click handlers are reserved for the export bridge's own wiring, and a hand-wired click beside a command double-fires.
- `AutoSuspendHelper` is the application-state suspension seat wired into the lifetime; shell-level state capture rides it or the drain band, never both for the same state.
- `Application` is also the global resource root: `Resources`, `Styles`, and `DataTemplates` are the suite-wide surfaces every window inherits — per-window copies of shared resources are the rejected form.
- `Application.ApplicationLifetime` is nullable and typed by modality — shell code pattern-matches the lifetime to the desktop contract once at boot; sprinkled lifetime casts at call sites are the rejected form.
- `Application.PlatformSettings` is feature-probed and nullable — platform-settings consumers fold absence, mirroring the capability grammar of every other platform feature.
- `Application.Name` is the one OS-facing application identity surface — about panels, tray tooltips, and platform integration read it; duplicating the product name as literals across surfaces is the rejected form.

## dock layout law

- Layout state lives exclusively in the factory-created model graph: `Factory.CreateLayout/CreateRootDock/CreateProportionalDock/CreateDocumentDock/CreateToolDock/CreateDocument/CreateTool/CreateDockWindow`.
- `DockControl.Layout` binds the graph; `InitializeLayout`/`InitializeFactory` gate auto-init; `DefaultContext` supplies the fallback context object.
- Every structural verb is a factory method — `AddDockable`, `InsertDockable`, `MoveDockable`, `SwapDockable`, `PinDockable`/`UnpinDockable`, `FloatDockable`/`FloatAllDockables`, `SplitToDock`, `SplitToWindow`, `DockAsDocument`, `CloseDockable`/`CloseOtherDockables`/`CloseAllDockables`/`CloseLeftDockables`/`CloseRightDockables`, `HideDockable`/`RestoreDockable` (both also addressable by string id).
- The factory is therefore the single mutation surface a command table targets; view-layer mutation of dock structure is rejected by the model contract itself.
- `IRootDock` carries the complete arrangement axis in one owner: `HiddenDockables`, four directional pinned lists, `PinnedDock`, `PinnedDockDisplayMode`, the floating `Windows` list, `FloatingWindowHostMode`, a per-root `DockCapabilityPolicy`, and `EnableAdaptiveGlobalDockTargets`.
- Persisting the root persists the whole arrangement — no side ledger of "which panel is where" may exist beside it.
- `IDockable` is a capability row: `CanClose/CanPin/CanFloat/CanDrag/CanDrop/CanDockAsDocument` plus `DockCapabilityOverrides` declare per-dockable policy.
- `DockGroup` is a string compatibility class restricting which targets accept a drop; `IsModified` is the dirty marker tab chrome renders; `Context` carries the screen view-model.
- Capability is data on the row — an `if`-ladder in drag handlers re-deriving what may dock where is the rejected form.
- Layout hygiene is model verbs too: `IsEmpty`/`IsCollapsable` on the dockable plus `CollapseDock` fold away emptied containers — a layout never accumulates dead splitter husks, and the collapse is part of the same mutation that emptied the dock.
- Pin preview is a verb family, not custom chrome: `PreviewPinnedDockable`/`TogglePreviewPinnedDockable`/`HidePreviewingDockables` plus per-dockable `KeepPinnedDockableVisible` and `PinnedDockDisplayModeOverride` cover flyover behavior of pinned tools.
- Sizing is model state too: `Proportion`/`CollapsedProportion`, min/max width and height bounds, grid placement (`Column`/`Row`/spans), and `IsSharedSizeScope` live on `IDockable` — splitter positions persist with the graph, so a restored layout restores proportions without measuring.
- Document presentation modes are factory verbs, not control swaps: `SetDocumentDockTabsLayout` (left/top/right tab strips) and `SetDocumentDockLayoutMode` (tabbed versus MDI) retarget one document dock — MDI is a layout-mode value on the same model, never a second windowing system.
- `IRootDock.IsFocusableRoot` plus the root-level `ShowWindows`/`ExitWindows` commands make multi-window presentation part of the model's command surface — float-all-restore choreography is command invocation, not window enumeration.
- Document docks can be items-source driven: the factory tracks dockables generated from items (`TrackItemsSourceDockable`/`UntrackItemsSourceDockable`, `GetContainerFromItem`), so a document set can derive from a collection — documents-as-rows over a live collection instead of imperative `CreateDocument` calls.
- `DockSettings.UpdateItemsSourceOnUnregister` governs the reverse edge from dock closure back into the source collection.
- Persistence is a workspace choreography: `DockWorkspaceManager` (constructed over an `IDockSerializer`) owns named snapshots — `Capture(id, layout, includeState)` serializes the layout to a string and optionally captures a `DockState`; `Restore(workspace)`/`TryRestore(id, out layout)` rebuild.
- `TrackFactory`/`TrackLayout` plus the `WorkspaceDirtyChanged` event give edge-triggered dirty detection — persistence writes on the dirty edge, not on a timer; `IsDirty` aggregates the active workspace.
- `Capture(includeState: false)` takes structural-only snapshots — presets that arrange panels without freezing their content are a parameter choice, not a second snapshot kind.
- `DockWorkspace` carries preset metadata (`Id`, `Name`, `Description`, serialized layout, optional state, dirty flag) — a workspace picker is a binding over `Workspaces`; `Remove`/`Clear` are the preset-management verbs.
- Hide is soft-close: `HideDockable` moves to `HiddenDockables` preserving the instance for `RestoreDockable`; `CloseDockable` destroys — the choice is the row's capability declaration, and "closable" panels that should survive reopen are hide-rows.
- Structure and content persist on two rails: the serialized layout string captures the dock graph; `DockState.Save(dock)`/`Restore(dock)` stash and rebind `Context` objects, document templates, and tool templates keyed by dockable `Id`.
- Restore order is structure-deserialize first, then content rebind by `Id` — dockable `Id`s are the durable identity contract.
- An `Id` the screen catalog cannot resolve folds to a default screen row rather than failing the restore.
- Capture sequences both rails internally (serialize the graph, then snapshot state) and `TryRestore` returns false on an unknown id rather than throwing — restore probes are total, and hand-sequencing the two rails outside the manager invites id drift between them.
- The workspace layer stores layouts as serialized strings and imposes no file format — workspace storage rides whatever settled store the suite chooses, so layout persistence gains atomicity and retention from that store's law for free.
- `IDockSerializer` (`Serialize<T>`/`Deserialize<T>`/stream `Save`/`Load`) is a seat, not an implementation — no serializer package is admitted; the suite's settled codec law supplies the implementation behind the seat.
- Hand-rolling a second layout format forecloses workspace capture, dirty tracking, and named-workspace switching in one stroke.
- `DockSettings` is a static policy row-set applied once at boot: drag thresholds, window magnetism (`EnableWindowMagnetism`, `WindowMagnetDistance`), floating-window ownership (`UseOwnerForFloatingWindows`, `FloatingWindowOwnerPolicy`, `FloatingWindowHostMode`, `CloseFloatingWindowsOnMainWindowClose`), drag preview (`ShowDockablePreviewOnDrag`, `DragPreviewOpacity`), global docking (`GlobalDockingProportion`, `GlobalDockingPreset`), selector overlay (`SelectorEnabled`, `DocumentSelectorKeyGesture`, `ToolSelectorKeyGesture`), and a `DiagnosticsLogHandler` hook routing dock diagnostics into the settled telemetry spine.
- These are process-wide mutable statics — setting them after windows exist is undefined-order behavior; they belong in the composition root only.
- Factory lifecycle hooks are a complete fact stream: dockable added/removed/moved/docked/undocked/pinned/unpinned/hidden/restored/activated/deactivated, window opened/closed/activated/move-drag begin/end.
- `OnDockableClosing` and `OnWindowClosing` return `bool` — the only legal places to block a close with unsaved-work policy.
- The shell folds all hooks into one layout-fact stream (telemetry, dirty edges, close gating) instead of subscribing per-concern.
- Floating windows are model rows: `Factory.CreateWindowFrom(dockable, DockWindowOptions)` plus the live `Factory.HostWindows`/`DockControls` registries mean multi-window layouts persist and restore through the same workspace capture.
- A floating window is never a separately managed window; `DockControl.HostWindowFactory` and `EnableManagedWindowLayer` select the floating-host kind as policy.
- Window ownership is a first-class verb — `Show(owner)` and `ShowDialog(owner)` parent secondary windows, and `WindowStartupLocation` declares placement — so float and companion windows declare ownership instead of z-order hacks; the dock settings ownership policy rides the same mechanism.
- Window-level `Closing` (with typed close-reason args) vetoes window-shaped concerns only; content-shaped vetoes belong to the dockable closing hook — the two veto altitudes never substitute for each other.
- `DockControl.IsDockingEnabled` is the drag gate; `RegisterExternalDockSurface` admits an additional drop surface, and `ShowSelector`/`HideSelector` drive the keyboard selector overlay whose gestures `DockSettings` declares — keyboard panel switching is a command-table row invoking these verbs.
- Dock-owned service seats exist for busy, confirmation, dialog, and window-lifecycle concerns — they are contracts the suite satisfies from its own settled rails, never a second dialog or busy system grown inside the dock layer.
- The dock manager is itself policy-carrying (`DockManagerOptions.IsDockingEnabled`) — the manager gate and the control gate are two altitudes of the same drag admission: manager-level for suite policy, control-level for per-surface state.
- Capability resolution is a three-altitude fold: global settings rows, the root's `DockCapabilityPolicy`, and per-dockable `DockCapabilityOverrides` — evaluation composes them in that order, so a capability question has exactly one answer path and no scattered flag checks.
- Group validity is owned by the model's group validator — `DockGroup` compatibility is enforced where moves execute, so an invalid drop is refused by the model, not by drag-handler vigilance.
- Drop-target taxonomy is two declared layers: local targets on each dock region and a global target overlay for edge docking — `EnableAdaptiveGlobalDockTargets` tunes the global layer; both are model-driven, and drawing custom drop adorners beside them is rejected.
- The dock control family (root, proportional, document, tool controls, tab strips, chrome, pinned controls, drag preview) is a pure view derivation of the model graph — restyling docking is control-theme work on these controls, never structural code.
- A dock theme contract seat exists (`IDockThemeManager`) — dock visual theming binds to the suite's variant application rather than carrying its own theme state.
- MDI placement has dedicated owners (the MDI layout manager seat with layout defaults and helper math) — cascade/tile arrangements are layout-manager verbs over the same document models, not window choreography.

## screen catalog and routing spine

- `IScreen` exposes one `RoutingState Router`; `RoutingState` is the entire navigation owner: `NavigationStack` (observable collection), `Navigate`, `NavigateAndReset`, `NavigateBack` as `ReactiveCommand<IRoutableViewModel, IRoutableViewModel>` values, `CurrentViewModel` as the projection stream, `NavigationChanged` as a change-set stream over the stack.
- Navigation verbs being commands means availability composes for free — `NavigateBack.CanExecute` already encodes stack depth; a hand-written "can go back" flag is the rejected form.
- `RoutingState` takes an optional scheduler at construction — navigation lands on the UI scheduler seam by default; supplying a scheduler is reserved for the headless row.
- `NavigationStack` is a settable, directly editable observable collection — session restore rebuilds navigation by stack manipulation (materialize the saved route keys, set the stack, let `CurrentViewModel` project the top) instead of replaying navigation commands with their side effects.
- The navigation-verb union is closed and small — push (`Navigate`), reset (`NavigateAndReset`), pop (`NavigateBack`), replace (pop+push composed), modal (routed to the dialog rail).
- Every cause of navigation (menu, palette, deep link, tray, restored workspace) folds to one of these verbs.
- A second router framework or per-surface navigation path is rejected: it forks the stack that `CurrentViewModel` and back-availability derive from.
- `RoutedViewHost` renders `Router` with `DefaultContent` fallback and `ViewContract` variants; `ViewModelViewHost` renders a direct view-model through the same locator contract.
- The empty navigation stack renders `DefaultContent` — the empty state is a declared row of the host, never an if-empty branch in screen code.
- `ViewContract` is the per-host view-variant axis: one view-model resolves to different views by contract string (a compact sidecar variant beside the full screen), so view variance is a contract row, not a second view-model.
- View resolution registers once — `RegisterReactiveUIViews`/`RegisterReactiveUIViewsFromAssemblyOf<T>`/`FromEntryAssembly` on the builder, or explicit `RegisterView<TView, TModel>`/`RegisterSingletonView` — producing one frozen route index from view-model type to view.
- A dockable is a screen: `Document`/`Tool` `Context` holds the same routable view-model the router would push, and `ViewModelViewHost` inside the dockable template resolves it through the same frozen index.
- "Panel screens" versus "routed screens" never split into two registries.
- The screen-catalog row is the absorption point: route key, view-model factory, dock role (document/tool/none), and activation policy live as one row.
- Adding a screen is adding a row — router, dock factory, deep-link grammar, and workspace restore all read it.
- The `DockState` content-rebind path and the deep-link path both resolve through the row's key — the row key and the dockable `Id` must be the same vocabulary.
- Typed view hosts two-way-sync `ViewModel` and `DataContext` via property-change interception with current-value writes, and the sync is type-guarded: a `DataContext` that is not `TViewModel` leaves `ViewModel` null silently.
- Consequence: template-level `DataContext` drift produces an empty screen with no error — the screen catalog binds the typed `ViewModel` property directly, sidestepping the trap.
- Deep links arrive through the activatable lifetime, not argv parsing: `IActivatableLifetime.Activated` with `ActivationKind.OpenUri` delivers `ProtocolActivatedEventArgs.Uri`.
- The activation-kind vocabulary is closed — `File`, `OpenUri`, `Reopen`, `Background` — and `Deactivated` mirrors it; `TryEnterBackground`/`TryLeaveBackground` are the explicit background transitions.
- The deep-link grammar is one parse from `Uri` into a navigation-verb row: `Reopen` folds to surfacing `MainWindow`, `File` folds to a document-open intent — an OS activation is one more trigger arm on the navigation union.
- The grammar's verb set includes workspace verbs — a URI can select a named workspace preset the same way it selects a screen, because both resolve through catalog and workspace keys the grammar already speaks.
- First-launch arguments and activation unify: `Args` on the desktop lifetime and `Activated(OpenUri)` later in life hit the same grammar; parsing argv in the entry point and URIs in an event handler is the same-law-twice defect.

## theme application

- `ThemeVariant` is a sealed record of `Key` plus `InheritVariant`: `Default`, `Light`, `Dark` are the canonical statics; explicit conversions bridge the platform light/dark vocabulary in both directions.
- The explicit conversion from the platform variant is the only platform-variant ingress — the platform signal converts at the application fold; nothing else writes variant values from platform state.
- A custom variant (`new ThemeVariant(key, inherit)`) resolves resources through its inheritance chain — a brand variant inheriting `Dark` falls back per-resource, so custom variants are one declaration, not a copied resource tree.
- A custom variant without `InheritVariant` resolves nothing outside its own dictionary — inheritance is mandatory, not stylistic.
- Variant binding is a three-level scope chain with one property: `RequestedThemeVariant` (nullable) on `Application`, `TopLevel`, and `ThemeVariantScope`; `ActualThemeVariant` plus `ActualThemeVariantChanged` are the resolved read side.
- Null `Requested` means inherit — application-null follows the platform, window-null follows the application, scope-null follows the window.
- Per-region theming (a dark canvas strip inside a light shell) is a `ThemeVariantScope` decorator row, never a parallel resource-dictionary swap.
- Per-window variant pinning rides the same property at the `TopLevel` level — an always-dark canvas window is a window-level `RequestedThemeVariant` row, and it must be set on floating dock windows explicitly because they inherit from the application, not from the window they detached from.
- Guarded versus throwing resource lookup is a declared pair (`TryFindResource` versus `FindResource`) — optional tokens probe, mandatory tokens throw at composition time; probing for a mandatory token hides theme defects.
- Theme-aware resource resolution is a first-class lookup: `TryGetResource(key, ThemeVariant, out value)` resolves against a specific variant and `ResourcesChanged` signals invalidation.
- Code-side token consumption goes through this seam at the consumer's `ActualThemeVariant`, never through a captured brush.
- `ActualThemeVariantChanged` at the application is the one subscription edge for code-side variant consumers — per-window subscriptions duplicate the edge and drift when floats pin their own variants.
- `FluentTheme` consumes the settled token algebra through two declared knobs: `Palettes` and `DensityStyle` (standard versus compact spacing).
- `Palettes` is typed `IDictionary<ThemeVariant, ColorPaletteResources>` (`Accent`, `BaseHigh/Medium/Low`, `AltHigh/Medium`, `ChromeHigh/Medium/Low`, `ErrorText`, `ListLow`, `RegionColor`) — custom variants get palette rows in the same dictionary, so brand theming is dictionary entries plus the variant declaration, with inheritance covering unlisted resources.
- Density switching is one property swap rebuilding control themes — a second spacing system or per-control margin overrides for "compact mode" is the rejected form.
- Follow-system is a fold, not an event-handler pile: `IPlatformSettings.GetColorValues()` plus `ColorValuesChanged` deliver `PlatformColorValues` (platform variant, contrast preference, three accent colors).
- One application-level fold maps (platform values × user override × density choice) to (`RequestedThemeVariant`, palette accent mapping, `DensityStyle`) atomically.
- User-override-wins is encoded by the override short-circuiting the fold, not by unsubscribing from the platform signal.
- The fold is idempotent on identical `PlatformColorValues` (record equality), so signal bursts during OS transitions need no debounce machinery.
- Control-template token consumption stays at the template seam: `ControlTheme` rows keyed by control type, `TemplateBinding` for per-instance values, theme-dictionary lookups for variant-dependent values.
- A literal paint inside a template forecloses variant switching for that control silently — the failure shows only when the second variant ships.

## divergent

- Maximal persistence law (shell-dock-persistence): one workspace owner persists shell geometry end-to-end — dock workspace capture on `WorkspaceDirtyChanged` edges, window placement read from the lifetime's `Windows` list, and per-canvas exportable state, all serialized through the one codec seat behind `IDockSerializer`.
- Restore is receipted choreography: deserialize structure, rebind contents by `Id` against the screen catalog, fold unresolvable `Id`s to default rows, re-create floating windows from the model, then apply variant and density — every step yields a typed receipt; best-effort partial restore without receipts is the rejected form.
- The persistence growth axis: a new persisted surface is one more capture row in the workspace, never a new file format; the serializer seat means codec evolution (compression, hashing, atomic write) arrives from the settled durability law without touching dock code.
- Restore-versus-catalog version skew: a restored workspace older than the current screen catalog is the dock equivalent of a schema mismatch — the fold drops rows whose `Id`s vanished (with a receipt), materializes catalog rows marked required that the workspace lacks, and never lets a serializer throw decide the outcome.
- The skew fold collapses first-run, restore, and upgrade into one total fold over (workspace rows × catalog rows).
- Workspace switching as a verb: because workspaces are named (`Capture(id)`/`TryRestore(id)`), layout presets (authoring, review, diagnostics) are command-table rows over the workspace manager — switching captures the active workspace, restores the target, and emits both receipts; a preset is data, not a window-arranging procedure.
- Dock facts feed the spine once: the single layout-fact stream from the factory hooks carries dirty edges to persistence, activation edges to the screen catalog's activation policy, and close vetoes to unsaved-work flows — three consumers, one stream, no per-concern factory subclassing.
- Routing spine absorption (screen-catalog-routing): the catalog row is the single growth point for five consumers simultaneously — router push, dock placement, deep-link resolution, workspace restore, and command-table screen-activation verbs; a sixth consumer (a remote "open screen" verb) lands as a reader of the same rows.
- The test for a correctly shaped row is that adding a screen touches exactly one declaration; a consumer needing a field the row lacks extends the row, never grows a side table keyed by the same key.
- Modal-as-verb boundary: the routing union's modal arm carries the request payload and awaits a typed result, delegating presentation to the dialog rail — the spine owns sequencing (a modal blocks navigation verbs behind it via its command's executing state), the dialog rail owns presentation.
- Putting modal presentation in the router or navigation sequencing in the dialog host both violate the split.
- Items-source documents close the loop with live data: a document dock bound to a collection makes "open documents" a keyed set that the same workspace capture persists and the same close-veto hooks gate — open-document lifecycle becomes collection membership, and "reopen last session's documents" is workspace restore, not a bespoke MRU mechanism.
- Theme-application failure taxonomy (theme-application): captured-brush staleness (fixed at the consumption seam by theme-aware lookup); variant-scope leakage (popups and floating dock windows are separate `TopLevel`s outside any scoping decorator — suite-wide variant must ride the application-level fold or floats silently diverge); platform-signal flap (idempotent fold); custom-variant orphaning (mandatory inheritance).
- Two-clock shutdown composition: `ShutdownRequested` (cancelable, OS-initiated) and explicit shutdown verbs feed the same drain state machine, with dock workspace capture registered as a drain participant.
- Layout persistence on the drain band rather than in window `Closing` handlers is what makes quit-during-drag safe: the factory's model graph is always coherent mid-gesture while visual state is not.
- Placement capture normalizes transient window states — minimized or mid-drag geometry folds to the last settled bounds before capture, so a workspace never restores into a minimized ghost.
- Boot choreography is ordered by ownership: lifetime `Startup` assigns `MainWindow`, then workspace restore populates the dock graph, then deep-link or argv intents replay through the grammar — intents arriving before restore queue behind it, because they may target screens the restore is about to materialize.
