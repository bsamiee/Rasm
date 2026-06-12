# surface-substrates-a11y-l10n — bedrock

## host-surface axis and mount law

- The host-surface axis is closed at three rows — owned window (desktop lifetime), embedded foreign view, headless — and one mount law covers all of them: every surface terminates in a `TopLevel`.
- The per-`TopLevel` service capsule (`Clipboard`, `StorageProvider`, `FocusManager`, `Screens`, `InsetsManager`, `TryGetPlatformHandle`, `RequestAnimationFrame`) resolves identically across rows, so code written against the capsule is surface-row-agnostic by construction.
- A surface-specific service lookup path is the rejected form.
- Capsule resolution from a visual returns null until the visual attaches — capsule access defers to attachment edges, never constructors, which is why activation scopes are the natural home for capsule-consuming subscriptions.
- Boot is one builder fold regardless of row: `AppBuilder.Configure<TApp>()`, then backend rows — `UsePlatformDetect` for windowed rows, `UseHeadless(options)` for the headless row — then framework admissions, then exactly one lifetime start.
- Backend rows are mutually exclusive per process — one windowing substrate per boot; mixed-substrate needs (a headless export worker beside a windowed shell) are separate processes composed by the suite, never one process with two backends.
- Reactive admission registers through `AfterPlatformServicesSetup`, executing after backend selection but before the lifetime runs — admission order is structural, not stylistic.
- A built-app guard makes a double build a silent no-op rather than an error, which masks a misordered second configure — the composition root must own the single boot expression.
- Embedding runs both directions through dedicated owners; both directions preserve the mount law because the embedded content still sees a real `TopLevel` capsule.
- Outbound (foreign native view inside the shell): a `NativeControlHost` subclass overrides `CreateNativeControlCore(IPlatformHandle parent)`/`DestroyNativeControlCore`, with `TryUpdateNativeControlPosition` for manual sync.
- The foreign handle is created against the parent handle and destroyed symmetrically — the override pair is the entire foreign-view lifecycle contract.
- Inbound (shell content inside a foreign host window): `EmbeddableControlRoot` is the `TopLevel` that mounts into a host-provided surface; offscreen top-level variants back render-only mounts.
- The embeddable root is a focus scope by contract — keyboard focus is contained at the embedding seam, so tab cycles inside the embedded content and hand-off to the foreign host is an explicit edge, not a focus leak.
- Offscreen top-levels are the preview substrate: rendering a screen for a thumbnail or gallery is a render-only mount of the same catalog row — never a hidden window or a screenshot of a live surface.
- The inbound mount has an explicit protocol: `Prepare`, `StartRendering`/`StopRendering`, `Dispose` — render gating is per-mount, so an embedded view hidden by its host stops rendering by verb, not by detached-tree side effects.
- Disposal is symmetric with the host's surface teardown; an undisposed embedded root outlives its host surface and renders into freed memory territory.
- Foreign-view coexistence constraints are structural, not stylistic: a native control region is an airspace hole — popups, overlay adorners, and the dialog overlay must avoid or own that region.
- Screens hosting foreign views declare the hole in their catalog row as a placement fact, letting dialog and overlay placement fold around it instead of discovering clipping at runtime.
- The hole is also input-opaque: the foreign view owns its own input pipeline, so the hosting row marks the region as outside the trigger union — gestures over the hole are the foreign view's business, and bridging specific foreign events back is an explicit trigger arm, never ambient leakage.
- Window-handle access is capability-probed: `TryGetPlatformHandle()` is nullable by design — headless and embedded rows may return nothing.
- Code requiring a raw handle is by definition row-specific and lives behind the axis row, never behind null checks at call sites.
- The capsule's own fallbacks prove the degradation grammar: the storage provider resolves to a no-op implementation when the platform lacks one, and the headless platform ships explicit stubs (clipboard, cursor, typeface) — absence is a capability value, not an exception.
- The insets manager is nullable on most desktop rows — another absence-as-capability example: edge-inset-aware layout declares the capability and folds absence to zero insets, never branches on platform names.
- Multi-monitor awareness is a capsule capability: `Screens` enumerates displays per `TopLevel` — placement restore clamps saved window geometry against the current screen set, so a vanished monitor folds to the nearest valid placement instead of restoring a window off-screen.
- Platform input metrics are capability rows, not constants: platform settings expose hold duration, tap size, double-tap size, and double-tap time per pointer type — gesture thresholds anywhere in the suite read these rows, so touch-versus-mouse tuning arrives from the platform, never from magic numbers.

## UI-scheduler seam

- Thread-marshaling mechanics arrive settled from the boundary law; this lane owns the seam composition: one main-thread scheduler value, installed once, consumed everywhere.
- The framework wiring (`WithAvalonia`) installs `AvaloniaScheduler.Instance` as the main-thread scheduler and the default task-pool scheduler beside it.
- The same admission registers the four binding providers — activation fetcher, data-template binding hook, command binding, property observation — and closes with the suspension-host registration.
- A second scheduler source or a hand-registered binding provider duplicates the seam; the admission is one expression.
- `AvaloniaScheduler` semantics are verified and consequential: work scheduled from the UI thread with zero due time executes inline up to a reentrancy bound.
- Everything else posts to the dispatcher at background priority, and timed work rides a one-shot dispatcher timer.
- Background priority means scheduler-marshaled work lands behind input and render — correct for state projection, wrong for anything frame-coupled.
- Frame-coupled work uses `TopLevel.RequestAnimationFrame(Action<TimeSpan>)` — the one frame-aligned hook, and the reason no render-loop timer exists anywhere in the doctrine.
- The seam law for streams: every pipeline crossing from a non-UI producer terminates with exactly one observe-on hop onto the installed main-thread scheduler at the binding edge.
- Earlier hops marshal too early (serializing background work); later hops do not exist (the binding edge is last).
- The inline-execution fast path means an already-on-UI-thread emission pays no dispatch — the single observe-on is safe to apply unconditionally rather than guarded by thread checks.
- Scheduler injection is total: derived-property, command-output, and timed operators all take the scheduler as a value, so the headless row and proof seam substitute schedulers without touching pipeline code.
- Pipelines reaching for an ambient static scheduler instead of the installed seam value are the form that breaks under headless time control.

## accessibility

- The automation surface is an attached-property row-set on any styled element, in three families.
- Identity rows: `AutomationProperties.Name`, `AutomationId`, `HelpText`, `AcceleratorKey`/`AccessKey`, `ControlTypeOverride`/`ClassNameOverride`, `ItemStatus`/`ItemType`.
- Relationship rows: `LabeledBy` (control-reference labeling), `IsRequiredForForm`, `IsColumnHeader`/`IsRowHeader`, `PositionInSet`/`SizeOfSet`.
- Structure and visibility rows: `AccessibilityView` (subtree visibility to assistive tech), `LiveSetting` (announcement politeness), `HeadingLevel`, `LandmarkType`, `IsOffscreenBehavior`.
- These are declarative rows — runtime peer manipulation for ordinary controls is rejected.
- Structure navigation is declared, not inferred: document-like screens set `LandmarkType` and `HeadingLevel` rows so assistive navigation by region and heading works without content heuristics.
- Virtualization breaks assistive set counting: realized rows in virtualized lists declare `PositionInSet`/`SizeOfSet` from the upstream projection's totals — the projection knows the true cardinality the panel never materializes.
- The peer system has a sharp default: `Control.OnCreateAutomationPeer()` returns a none-peer, so a custom-drawn control is invisible to assistive technology until it overrides the method.
- Every owner-drawn surface (canvases, custom chrome) either overrides with a typed peer or sets `ControlTypeOverride` plus name rows on a focusable wrapper.
- The built-in peer family (button, list-item, menu-item, content-control, items-control and kin) covers composed controls automatically — composition over owner-drawing is also the accessibility-cheap choice.
- The embedded root carries its own automation peer, so the accessibility tree continues across the inbound-embedding seam — embedding does not exempt a surface from the automation law.
- Automation identity derives, never duplicates: `Name` derives from the same key that yields the localized header, `AcceleratorKey` renders the same gesture the key table claims, `AutomationId` is the stable intent or screen key.
- The command vocabulary is the single source and the automation rows are a projection of it — automation-driven invocation is just another trigger arm with typed evidence rather than a parallel naming scheme.
- Keyboard reachability is attached rows, not handler code: `KeyboardNavigation.TabIndex` (defaulting to max, so declaration order wins until a row sets an index), `TabNavigation` mode per container, `IsTabStop`, and `TabOnceActiveElement`.
- The tab-navigation mode vocabulary is closed — `Continue`, `Cycle`, `Contained`, `Once`, `None`, `Local` — so focus topology per container is one enum row: cycle for dialogs, contained for panels that must not leak focus, once for composite controls behaving as one stop.
- Tab order is declared data on the layout, auditable by tree walk — a screen's focus cycle is a container-mode row, never key handling.
- Mnemonic rendering is the access-text primitive paired with the `AccessKey` row — the underscore-marked header renders and registers the key in one declaration.
- Since headers derive from the localization key, mnemonics localize with the header rather than being re-declared per locale.
- Contrast is a gated fold with two verified inputs: relative luminance from the color-helper surface supplies the ratio inputs for token pairs, and `PlatformColorValues.ContrastPreference` (none/high) from platform settings signals the OS preference with the color-values-changed edge.
- The gate runs where variant application runs — token pairs failing the declared ratio under a candidate variant are a composition-time rejection (a theme defect).
- The high-contrast preference selects a variant row through the same application fold — a separate high-contrast code path is the rejected form.
- Live regions replace focus theft: status surfaces announce through `LiveSetting` politeness levels; moving keyboard focus to announce a change is the rejected form — focus moves only on user intent.
- Focus is per-surface state: each `TopLevel`'s `FocusManager` owns its focus; element-level focus requests and manager-level focus ownership are two altitudes of the same verb, and cross-window focus reasoning must go through the owning capsule.
- Decorative subtrees prune themselves from assistive output via the `AccessibilityView` row — visual ornamentation never costs screen-reader noise, and the prune is declarative, not peer surgery.

## localization and RTL

- The localization spine is the intent and screen key vocabulary: every user-visible string resolves from a key the command table or screen catalog already owns, through locale resource dictionaries swapped atomically on the application's resource surface.
- The theme-aware resource lookup doubles as the locale lookup, so locale switch is the same atomic-swap shape as variant switch and requires no per-control rebind beyond the resource invalidation edge.
- String literals at surfaces are the defect — the key is the contract.
- Gesture display localizes through the format channel: the invariant gesture form is the persistence and table format; the platform format (via the gesture format-info provider) renders user-facing chord text.
- Storing or comparing the platform-formatted string breaks round-trip on the next platform; the two formats never cross.
- The inspector surface carries its own localization seat (a localization service with an asset-backed implementation) — domain property display names route through it with the same key discipline, so inspectors localize without per-property attributes diverging from the suite vocabulary.
- RTL is one inherited property: `FlowDirection` set on the top-level flows the entire visual tree (an inheriting attached property registered at the visual layer).
- Mirroring is structural for layout; owner-drawn content reads its own `FlowDirection` to mirror geometry — owner-drawn surfaces that ignore it ship un-mirrored in RTL locales with no diagnostic.
- Per-control direction overrides exist only at genuine direction islands (a left-to-right code editor inside a right-to-left shell), declared as rows, not sprinkled.
- Locale and direction are one switch: the locale row carries its direction, so applying a locale folds (resource dictionary swap × `FlowDirection` × gesture display refresh) in one place.
- Three separate "change language" code paths is the rejected form.

## production headless surface and proof seam

- Headless is a production surface row, not test scaffolding: `UseHeadless(new AvaloniaHeadlessPlatformOptions { UseHeadlessDrawing, FrameBufferFormat })` boots the full retained UI without a display.
- `UseHeadlessDrawing` defaults to true (stub drawing, fastest, no pixels); setting it false and admitting the real render backend produces pixel-true frames.
- `FrameBufferFormat` declares the pixel format of captured frames — capture consumers read the declared format instead of assuming one.
- The same screens, command table, navigation spine, and binding pipelines run unmodified under headless — that invariance is the entire value of the mount law.
- Frame production is explicit under headless: `CaptureRenderedFrame` renders then grabs; `GetLastRenderedFrame` returns the existing frame without forcing a render — the two verbs differ in whether they advance the frame.
- `SetRenderScaling` controls DPI for capture — high-density rendering proofs are a scaling row, not a separate environment.
- `AvaloniaHeadlessPlatform.ForceRenderTimerTick` advances the frame clock manually — the tick verb is the headless frame authority.
- Headless rendering is pull-based — capture verbs and ticks are the only frame triggers and nothing renders "eventually"; this is the substrate for server-side render and export modalities as well as proof.
- Pixel verbs are meaningful only on the pixel row: the surface-row capability table marks frame production as stub or pixel, so capture requested against the stub row is a mount-time capability rejection, never an empty-bitmap surprise downstream.
- Synthetic input carries provenance in its trigger evidence — receipts distinguish device-driven from synthetic invocations, so audit folds can separate user action from scripted driving without a second pipeline.
- Synthetic input drives the real pipeline: window-level key press/release, physical-key pressing (layout-positional) beside logical key press, text-input, mouse down/up/move/wheel, and drag-drop verbs inject through the same input path real devices use.
- The logical-versus-physical key split matters: chord verification exercises logical gestures; layout-dependent behavior (mnemonic reachability across keyboard layouts) exercises the physical-key verb.
- The trigger union, key tables, and behaviors execute identically under synthetic input — headless interaction is real interaction.
- The interactive-capability gate is owned once at the surface row: each mounted surface declares display-only or interactive, and input admission (real or synthetic) folds through that declaration before reaching the trigger union.
- A display-only surface ignores synthetic input exactly as it ignores real input; headless driving of an interactive surface is legitimate interaction, not a bypass.
- Scattering "is interactive" checks through handlers is the rejected form; the gate is one predicate at the mount.
- Synthetic drag-drop closes the input matrix: drop-acceptance law and payload handling are provable headless because the drag verbs ride the same routed pipeline — drag behavior is not a windowed-only concern.
- The proof seam composes, never re-teaches: headless sessions dispatch UI work onto a session-owned UI thread (session start, per-assembly shared session, dispatch, async dispose), and the proof rails consuming them are settled elsewhere.
- Session dispatch marshals values out: UI reads execute inside the dispatch and return as plain results — proof code never touches UI objects outside the session thread, mirroring the seam law the scheduler section installs.
- The per-assembly shared session keys on the assembly — parallel proof hosts in one process share one platform boot, which is the amortization that makes structural audits cheap enough to run wholesale.
- Session economics: the per-assembly shared session amortizes platform boot across proofs while per-proof sessions buy isolation — the choice is an isolation-versus-cost row, declared once, never per-proof improvisation.
- This lane's law is only that the production headless surface and the proof seam are the same platform row with the same options vocabulary — proof runs exercise the shipping mount path.

## debug-gated dev loop

- The dev loop is a build-asset gate, not a runtime flag: the hot-reload package ships no runtime library — build props/targets gate everything behind a master knob defaulting to debug configuration.
- An injected source file contributes the `UseHotReload` builder and application extensions; release builds strip the runtime, extension, and weaver references entirely.
- The knob family is complete policy: injection mode and initial-patching gates, resource recompilation, reference processing and exclusion for the release strip.
- Production wiring never mentions hot reload — the builder call exists only in the injected debug source path, and a hand-written hot-reload bootstrap beside the injection is rejected.
- Runtime control inside the gated build: enable, disable, and trigger verbs plus a configurable hotkey and timeout; a lite-mode knob trades reload fidelity for speed as a declared row.
- The trigger verb makes "reload now" a command-table row in debug builds, entering the same command vocabulary instead of a bespoke key hook.
- Remote dev loop for non-desktop rows: the file-server channel (address with fallback, port, shared-secret or certificate transport trust, search-depth and lifetime bounds) serves markup to targets where the source tree is not local.
- The trust knobs mean the dev loop has a real security boundary — defaults-off and debug-only is the law, and the secret or certificate knobs are mandatory the moment the channel leaves loopback.
- Render diagnostics ride the capsule (`RendererDiagnostics` per `TopLevel`) — debug overlays are a per-surface toggle in the dev loop, not a build flavor.
- The dev-loop hotkey enters the gesture-conflict fold as a debug-build row — a reload chord shadowing a product gesture is a composition-time conflict receipt in debug builds, not a mystery dead key.
- Debug-row injection generalizes: every dev-loop surface (reload verb, diagnostics overlay toggle) enters the same command vocabulary as conditional rows, so the debug build differs from release by row set, never by code paths inside screens.
- Dev-loop versus proof-seam separation: hot reload patches markup and view code paths and never runs under the headless proof row (a different builder fold).
- A behavior that only reproduces under hot reload is by definition a dev-loop artifact — the proof seam is the arbiter, which is exactly why both must share the production mount law.

## divergent

- One mount law, N rows (host-surface-mount): the maximal form is a closed surface-row table — owned-window, embedded-inbound, embedded-outbound-hosting, headless-interactive, headless-display-only — where each row declares its builder fold fragment, lifetime owner, capability set (handle availability, frame-production mode, input admission), and interactive gate.
- Mounting any screen is one fold over (catalog row × surface row); foreclosed: per-surface boot forks, handle null-checks at call sites, "test mode" flags inside screens, and display-only surfaces that accidentally accept input.
- The growth axis is explicit: a new modality (a remote-rendered surface, a render-only export worker) is one row whose capability set the existing folds already consume — zero screen edits.
- Capability-probed services as the embedding contract: because the capsule resolves services per-surface with nullable or no-op fallbacks, embedded and headless rows degrade by capability rather than by exception.
- Generalized: any code consuming the capsule declares required capabilities and the mount fold verifies row-capability coverage at composition time, converting "clipboard is null in the embedded panel" from a runtime surprise into a mount-time rejection receipt.
- Automation as a driving channel: because automation invocation is a trigger arm, the automation tree doubles as the remote-control protocol for interactive surfaces — external drivers address stable `AutomationId`s and invoke through the same gated union, so scripted driving needs no parallel command API.
- A11y as derivation audit (a11y-l10n): since automation name, accelerator, and id all derive from the command and catalog vocabulary, an automation-tree walk under the headless row is a complete derivation audit.
- The walk checks: every focusable surface exposes a non-none peer with a derived name; every intent-bound surface exposes the gesture it claims; tab-order rows produce a connected cycle — and the walk's diff against the command table is a typed receipt.
- Accessibility regression checking thereby collapses into vocabulary-coverage checking for the structural layer — no assistive-technology-in-the-loop required to prove derivation completeness.
- Contrast-gated variant admission: composing the contrast fold with theme application yields admission semantics — a variant (including custom brand variants) is admitted only when its token pairs pass the declared ratios for both contrast-preference states.
- The high-contrast OS edge becomes a pure row selection over pre-verified variants, never a runtime recolor — the entire contrast surface moves to composition time and "contrast mode" is zero code.
- Locale-key totality audit under headless: the same headless walk that audits automation derivation audits localization — every rendered string traceable to a key, every key resolvable in every admitted locale.
- The missing-key set per locale is a typed receipt, making translation completeness a fold over (key set × locale rows) instead of visual inspection.
- Headless parity as the law of the lane (headless-devloop): the deepest property is bidirectional parity — anything provable headless holds windowed because the mount law is shared, and anything that diverges windowed-versus-headless is a platform-row capability by definition, locatable in the row table rather than in screen logic.
- The practical fold: capability-conditional behavior is enumerable by diffing row capability sets, so the complete set of "works differently headless" facts is the row table itself.
- A divergence found elsewhere means the row table is incomplete, and the fix is a row edit, not a screen patch.
- Frame-clock ownership under headless: because headless frames advance only on capture and tick verbs, time-coupled UI behavior (animations, throttled gestures, debounced actions) must take time from the injected scheduler and clock seam rather than wall time.
- The headless row is the structural enforcement of the clock-seam law for the entire interaction layer: any animation or debounce that fails to advance under forced ticks has smuggled wall time, and the failure is diagnostic, not flaky.
- Pixel-evidence economics: stub drawing proves structure (tree, layout, automation, bindings) at near-zero cost; pixel-true capture proves rendering — the two headless drawing modes are evidence classes, and choosing per proof keeps the expensive class rare; running everything pixel-true is the rejected default.
- Pointer-type-polymorphic interaction: because hold, tap, and double-tap thresholds are platform rows keyed by pointer type, gesture-consuming surfaces parameterize on the metrics rows and become touch-correct without a touch mode — a "touch mode" flag is the rejected form, and the metrics rows are the only place input physics may vary.
