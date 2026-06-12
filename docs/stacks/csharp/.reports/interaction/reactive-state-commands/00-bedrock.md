# reactive-state-commands — bedrock

## reactive view-model law

- `ReactiveObject` plus the `RaiseAndSetIfChanged` mixin is the mutable-property floor; everything derived rides `WhenAnyValue` into `ObservableAsPropertyHelper<T>` via `ToProperty`.
- The derived-property knob triad is load-bearing: `initialValue`/`getInitialValue` prevents a `default(T)` flash before the first emission.
- `deferSubscription: true` delays the upstream subscription until the property is first read, turning expensive derivations into pay-on-read.
- The `scheduler` parameter defaults to the installed main-thread scheduler; a derivation already produced on the UI thread re-dispatches through it — passing an immediate scheduler is the lever when profiling shows double-scheduling, never a reflex.
- Derived state is a fold over property streams, never imperative recomputation: `WhenAnyValue` for property tuples, `CombineLatest` for cross-object joins.
- Property streams replay the current value at subscription — the structural reason property-derived inputs satisfy seeded-availability contracts everywhere downstream.
- A raw event-projected observable does not replay and must be explicitly seeded at its admission.
- Deep paths ride expression-chain subscription (`SubscribeToExpressionChain`): an `a.B.C` chain re-resolves when intermediate objects are replaced — the chain observable survives intermediate swaps where a hand-wired `PropertyChanged` chain silently goes stale.
- Activation is ref-counted scope, not construction: `IActivatableViewModel.Activator` with `WhenActivated(disposables)` opens a subscription scope that closes on deactivation; views compose the same hook through the view-side `WhenActivated` mixins.
- The view drives activation: for controls it derives from loaded/unloaded events merged and made distinct; for bare visuals from visual-tree attach/detach.
- View-model activation is a consequence of view activation — the fetcher activates the view-model when its view loads, so a view-model never self-activates and activation order is view-first by construction.
- Consequence — a docked screen re-fires activation on every tab switch, float, and pin.
- The split: long-lived state (caches, derived properties) belongs in the constructor; everything touching external streams, timers, or services belongs inside `WhenActivated` or it leaks one subscription per tab switch.
- Activation composes hierarchically by construction: a parent screen activating child panels happens automatically when child views load.
- Explicit child-activation forwarding is rejected — it double-activates when the child view also loads.
- Error flow is one rail: `ThrownExceptions` on every command and reactive object routes pipeline faults to a single observer wired at the composition root.
- The error rail is itself scheduled — fault delivery rides the output scheduler, so the root observer reads faults UI-safely without its own marshaling.
- Fault receipts from the error rail join the same fact stream as execution receipts — one operational view covers successes and failures keyed by intent.
- A default exception handler backs unobserved `ThrownExceptions` streams; once observed, the observer owns the failures — observing at the root is therefore mandatory, not optional hardening.
- Per-command `try/catch` in execute bodies for non-recoverable faults is rejected; recoverable outcomes are values in `TResult`, not exceptions.
- Command-level `ThrownExceptions` carries execution faults; object-level `ThrownExceptions` carries pipeline faults from derived state — both join the one root observer, but the split tells the observer which class of failure it is reading.
- The reactive stack admits at the composition root through builder rows — view and view-model registration, standard converter admission, and the exception-handler row — so error policy and binding converters are declared once beside the other root policies.

## view-model-to-view interaction rail

- View-model-to-view requests ride `Interaction<TInput, TOutput>`: `RegisterHandler` on the view side (inside the activation scope), `Handle(input)` on the view-model side awaiting `SetOutput` — the view-model asks, the view answers, and neither references the other.
- Handler dispatch is verified precedence-by-recency: `Handle` walks the handler list in reverse registration order on the handler scheduler and stops at the first handler that sets output.
- An unhandled walk throws the typed `UnhandledInteractionException<TInput, TOutput>` — a missing handler is loud, never a silent no-op.
- `SetOutput` is what marks the context handled — a handler that inspects and declines simply does not call it, and the walk continues to the next-older handler.
- Registration disposal restores the earlier handler — overrides are scoped, stack-shaped, and lifetime-bound.
- Confirmation flows therefore override per-surface: a headless row registers an auto-answer handler over the dialog-presenting one without touching the requesting view-model.
- `BindInteraction` ties handler lifetime to view activation, so a deactivated view cannot leave a stale handler claiming requests.
- Interaction-versus-dialog altitude: the interaction is the contract and a dialog session is one possible handler — routing a confirmation directly to the dialog rail from a view-model couples it to presentation and forecloses headless auto-answer.
- Interaction input and output types are the contract surface: a small standard set of question shapes (confirm, choose-one, supply-value) lets handlers compose across screens — per-screen bespoke interaction types multiply handlers without adding questions.

## command-intent table

- The single command vocabulary is one row table — intent key, execute effect, availability inputs, gesture slot, header, icon key, toggle type, surface placements — and every interactive surface is a fold over it.
- In-window menu carrier: `MenuItem.Command/CommandParameter/InputGesture/HotKey/Icon/IsChecked/ToggleType/GroupName` — gesture display and hotkey claim are separate columns on the same row.
- OS-exported menu carrier: `NativeMenuItem.Command/Gesture/Icon/IsChecked/ToggleType/IsEnabled/IsVisible` plus nested `Menu` for submenu rows.
- Tray carrier: `TrayIcon.Command` plus its `NativeMenu` — the tray menu is the same menu fold applied at the tray placement tag.
- Dock chrome carrier: `DockCommandBarItem { Header, Icon, Command, CommandParameter, Items, GroupId, Order, IsSeparator }` — bar grouping and ordering are row fields, separators are rows.
- Key tables, the palette, and remote verbs consume the same rows through their own folds — no carrier owns row data.
- Per-surface command registries are deleted: each surface derivation is a projection of the same rows, so a new command is one row and N surfaces update by construction.
- The intent key is triple-duty by law: localization key (headers and tooltips resolve per-locale from it), icon key (assets resolve from it), and the automation identity other lanes derive from.
- A literal header or icon reference at a surface is the smell that a row field was bypassed.
- Visibility and enablement are distinct row projections: enablement derives from the availability stream, visibility from placement policy (`IsVisible` on the native item) — hiding an unavailable verb versus disabling it is a per-placement policy bit, never two different rows.
- Command construction is a closed factory family: `Create` (sync), `CreateFromTask` with `CancellationToken` overloads, `CreateFromObservable`, `CreateRunInBackground` (explicit background scheduler parameter), `CreateCombined`.
- The factory matrix spans the parameter/result quadrants (`Unit`-to-`Unit` through `TParam`-to-`TResult`) — modality lives in the value types, so verb arity never produces sibling rows.
- Cancellation derives from the carrier: task commands receive the token (disposal and window close flow into it); observable commands cancel by unsubscription.
- `Create` executes its body inline on the invoking thread — long work in a plain `Create` blocks the UI; the background factory exists precisely for that case, so the choice between the two is a latency declaration, not style.
- Stream-into-command is an operator, not a subscription body: `InvokeCommand` pipes an observable's emissions into a command, gated by current availability — emissions arriving while unavailable are dropped, not queued.
- The typed `InvokeCommand` overloads bind the emission as the parameter; the expression overloads late-bind the command off a target property so the pipe survives command replacement.
- Hand-subscribing a stream to call execute bypasses the availability gate and is the rejected form.
- The row stores the effect; the factory choice derives from the effect's carrier — bespoke per-row wiring is the rejected form.
- `CreateRunInBackground` moves the execute body off the UI thread with results marshaled back — the lever for compute-shaped commands; marshaling inside the execute body is rejected.
- `CreateCombined` is the batch-verb absorber over command-shaped children: combined availability is the all-true fold of child `CanExecute` AND its own supplied gate; execution is the combined latest of child executions.
- Batch verbs ("save all", "close others") are one combined row over existing rows, never re-implemented loops.
- Combined children must share parameter and result types — heterogeneous batches compose at the effect level (one row whose effect sequences other rows' effects through their result streams) rather than forcing the combined factory.
- The command parameter binds at the surface row from placement-local context (the selected item, the focused document) — the parameter is data the trigger carries; the row's identity is always the intent key, never the parameter.
- Command binding outside markup rides the binding surface (`BindCommand`, with the command-binding provider behind it) — code-composed surfaces bind rows through the same contract markup uses; there is no second binding idiom to audit.
- The binder is affinity-selected: the command-binding provider scores targets (`GetAffinityForObject`) and binds command-bearing properties or events accordingly — binder selection is the framework's dispatch, never a per-control switch in shell code.
- Property observation is provider-backed the same way (`GetNotificationForProperty` behind the registered provider), and two-way property subjects (`GetSubject`/`GetBindingSubject`) are the reactive seam onto retained properties — hand-wired property-changed listeners against retained objects are the deleted form.
- OS-exported menu items track enablement through weak `CanExecuteChanged` subscribers — availability flows to the native menu without the menu retaining the command, so table rows never leak through OS menu exports.
- `CanExecuteChanged` is raised only on distinct availability transitions (the pipeline is distinct-until-changed before the bridge) — surfaces re-query exactly when the value flips, never on input noise.
- One truth, two read protocols: reactive consumers subscribe the `CanExecute` observable; retained controls read through the bridge — both observe the same replayed pipeline, so availability can never disagree between a menu and a reactive gate.
- Rows are dependency-free among themselves: a row references typed inputs, never another row — verb sequencing ("export then reveal") composes at the effect level through result streams, keeping the table a flat set rather than a dependency graph.
- Execution evidence is the command's output stream: each row's `TResult` is a typed receipt observed once at the spine (telemetry, undo journal, palette recent-ranking).
- Surfaces never subscribe to results individually; `Execute` values fold into one fact stream keyed by intent.
- The command itself implements `IObservable<TResult>` — subscribing to the command observes all execution results regardless of trigger, which is the spine's subscription point.
- The observable returned by `Execute(parameter)` is the per-invocation result and is cold — execution starts at subscription, so composing an execution into a larger pipeline defers it correctly; a parameterless `Execute()` covers `Unit`-param rows.
- Commands are disposable owners: disposal releases the availability subscription and the `CanExecuteChanged` wiring — command lifetime equals row lifetime, and the frozen table disposing at process exit is the only disposal site; per-screen disposal would orphan surfaces still bound.

## availability algebra

- The verified availability pipeline inside every command: `(supplied ?? alwaysTrue).Catch(ex => { thrownExceptions.OnNext(ex); return false; }).StartWith(false).CombineLatest(IsExecuting, (can, busy) => can && !busy).DistinctUntilChanged().Replay(1).RefCount()`.
- Seeded-total law: the pipeline starts `false` until the supplied observable emits — an availability stream that does not emit synchronously at subscription leaves its command disabled.
- A bare subject with no current value disables forever; the contract for inputs is emit-on-subscribe, no terminal completion, total selector.
- Even default-available rows tick false-then-true at subscription (the seed precedes the always-true default) — surfaces bound before first paint absorb the tick invisibly; surfaces bound late render a momentary disabled flash, which is a binding-order symptom, not a row defect.
- The bridge is push-updated, not pull-evaluated: availability transitions write the cache and raise the change event — surfaces polling between transitions read coherent state, and no surface ever forces re-evaluation.
- Busy-exclusion is built in: `IsExecuting` is already combined — adding `!IsExecuting` to a supplied gate double-counts and can deadlock re-enablement.
- `IsExecuting` is a scan-counter over execution demarcations (begin increments, end decrements, floored at zero), seeded false, distinct, replayed — it correctly tracks overlapping executions.
- `IsExecuting` is also the canonical progress input for busy indicators — a separate "is busy" flag per screen is rejected.
- Cancel verbs are sibling rows gated on the target's `IsExecuting`: the cancel row is available exactly while its sibling executes, and cancellation flows through the sibling's carrier (token or unsubscription) — a cancel button never reaches into execution internals.
- Observable-bodied commands stream progress: an execution may emit intermediate `TResult` values before completing, so progress reporting is result emissions on the existing rail — a separate progress channel beside the command is rejected.
- A throwing availability stream latches the command disabled permanently and routes the exception to `ThrownExceptions` — availability folds must be total over their inputs; the latch is unrecoverable without rebuilding the command.
- `Replay(1).RefCount()` makes the pipeline cold until observed and reset when the last observer leaves — `CanExecute` is a pure value stream; side-effects observed through it re-trigger on re-subscription.
- All command outputs — results, thrown exceptions, `IsExecuting`, `CanExecute` — surface on the output scheduler, which defaults to the installed main-thread scheduler value: command-derived state is UI-safe by default.
- Overriding the output scheduler is a headless-row decision, not a per-command one — per-command scheduler choices fragment the seam the scheduler law installs once.
- The `ICommand` bridge is verified two-sided: `CanExecute(parameter)` returns the cached latest value ignoring the parameter entirely.
- `Execute(parameter)` on the bridge maps null to `default(TParam)` — a value-typed parameter row invoked from a parameterless surface receives the default value, silently.
- The bridge throws `InvalidOperationException` on a wrong-typed parameter — a wrong-typed `CommandParameter` fails at invoke time, not bind time.
- The bridge swallows the execution observable's errors (catch-to-empty) — ICommand-initiated failures surface only through `ThrownExceptions`; receipts come from the spine's result subscription, never from the control.
- Parameter-dependent availability is structurally inexpressible at the control seam; it must enter the availability stream as a typed input.
- The availability fold per row is one `CombineLatest` over the typed input set — level admits, validity gates, selection shapes, busy excludes implicitly — with a row-declared selector.
- Inputs arrive as settled seeded streams from their owning laws — the table never derives degradation or validity itself, it consumes them.
- A per-command `if` chain re-deriving inputs is the rejected form; the growth axis is one more element in the combine tuple plus a selector edit — zero new surface.
- Group gating composes above rows: a degradation level that disables a verb family is one predicate row applied across the family's intents in the fold, never N edits.
- Every cause of an interactive action is a case in one trigger union carrying typed evidence — pointer (position, modifiers), key chord (gesture, claiming scope), menu/tray (placement), palette (query text), deep link (URI payload), remote verb (caller identity), automation invoke.
- The union dispatches totally to intent rows: a new cause is a new case, and the total dispatch breaks at compile time until every consumer handles it.
- An external or scripted command verb is one more trigger arm on the existing union, never a parallel execution pipeline — entering through the arm structure is what grants it availability gating, busy exclusion, and receipts.
- Parameterized intents stay one row: the trigger arm's typed evidence supplies the parameter, so per-target verb variants ("open in left dock", "open in right dock") are evidence values, not sibling rows.

## user-facing validation projection

- `IValidatableViewModel.ValidationContext` is the single projection owner: `Valid` (seeded observable), `IsValid` (current), `Text` (typed `IValidationText`), `ValidationStatusChange`, and `Validations` as an observable component list.
- The settled boundary-validation law produces typed outcomes; this lane converts those outcomes into observable validation state for screens — re-running validation logic in the view-model is the rejected form.
- The `ValidationRule` overload family is the bridge taxonomy: property + predicate + message for local input checks; `IObservable<bool>` + message for externally computed validity; `IObservable<IValidationState>` (and the `TValue : IValidationState` generic form) for projecting settled rich outcomes.
- The state-observable overloads are the canonical seam from the typed fault rail: they carry validity and message together in one value, so the fault-to-state projection happens once at the seam, not per rule.
- `IValidationState` is a small contract (validity plus typed text) — domain projections implement it directly when a richer state must travel (a fault code beside the message), so the projection seam widens by implementing the contract, never by a parallel state type.
- Each rule returns a `ValidationHelper` handle; `ClearValidationRules` retires rule sets — rule membership is data with a lifecycle, not construction-time wiring.
- The helper is itself a reactive value: `IsValid`, typed `Message`, and the `ValidationChanged` state stream live on the handle, and disposing the handle retires the rule — per-rule observation binds to the helper, never to a re-query of the whole context.
- The context activates lazily (`Activate` starts its composition) and `Validations` is an observable list — rule membership changes flow as change-sets, so a summary panel listing active rules is a binding over `Validations`, not a parallel registry.
- The context is a disposable owner (`IsDisposed` guards reuse) — it disposes its component subscriptions with the screen, so validation never outlives the view-model that declared it.
- Multi-property rules ride the observable-validation components (property-typed variants tie state to specific properties for adorner routing) — a cross-field rule is one component observing a combined stream, registered like any other, never a hand-rolled aggregate flag.
- `ValidationContext.Valid` is itself a canonical availability input: form-submit rows gate on it through the same availability fold.
- Validation reaching commands through any channel other than the availability algebra (disabling buttons in code-behind) splits the gate vocabulary.
- Message projection is typed end-to-end: `IValidationText` with `BuildText`/`ToSingleLine` and the formatter contract (`IValidationTextFormatter<T>`, single-line default) renders; `ObserveFor` scopes observation to one property.
- `BindValidation` ties context or per-property state to view text targets; freeform string side-channels for error display are rejected — the formatter is the one place presentation varies.
- Control-native error adorners ride the error-info bridge (`ReactiveValidationObject` with `GetErrors`/`ErrorsChanged`) when a control template renders its own error state; `BindValidation` serves screens that own the rendering.
- Both adorner paths read the same context — the choice is presentation policy, not a second validation source.
- Per-property versus whole-form is a projection axis, not two systems: property-scoped overloads feed field adorners, context-level `Valid`/`Text` feeds the submit gate and summary — one context, two read altitudes.
- Projection latency is zero by construction: validation state is already observable when the settled outcome lands, so there is no "validate on submit" pass — submit availability was true the moment the last rule turned valid, and a submit-time re-check is the rejected form.
- Async validity (a uniqueness probe) enters as an `IObservable<IValidationState>` rule whose pending state is invalid-with-message — pending-as-invalid keeps the submit gate conservative without a tri-state flag.

## divergent

- One-fold command table (command-intent-availability): the maximal form is a frozen table built at composition time — each row constructed once with its availability `CombineLatest` already composed from the typed input set, then every surface materialized as a pure projection: menu trees fold rows by placement path, the key table folds rows by gesture slot, the palette folds rows by searchable header, tray and dock bars fold by surface tag ordered by row order/group fields.
- The table is the only stateful object; surfaces are derived views rebuildable at any time (locale change, density change) from rows alone.
- Foreclosed by construction: stale per-surface enablement (all surfaces observe the same replayed stream), command duplication (one execute effect per intent), gesture drift (the gesture lives on the row, surfaces render it).
- Availability input admission: each typed input is normalized to a seeded, distinct, replayed stream at its admission into the table, and a composition-time probe (first-value timeout against the emit-on-subscribe contract) converts the silent forever-disabled trap into a loud construction failure naming the offending input.
- Placement as key-derived structure: hierarchical intent keys carry the menu path, so menu trees, palette categories, and dock-bar grouping all derive from key segments — menu structure is data in the key namespace, and reorganizing menus is a key edit, never surface rewiring.
- Generated row families: per-screen verbs (show, focus, close) are rows generated by folding the screen catalog into the table — the table admits generated row sets beside hand-declared rows, so a new screen contributes its verb rows without anyone writing them.
- Verb families collapse through parameter unions: when a family of related verbs differs only in a typed discriminant, the row's parameter is a union case and the family is one row — the execute effect dispatches totally on the case, so family growth is a case addition, not a row addition.
- Toggle and radio semantics inside the row: `ToggleType` plus `IsChecked`/`GroupName` on both managed and native menu items mean stateful verbs (show-panel, mode-select) are rows whose checked state binds to a state stream.
- Mutually exclusive modes are a group fold whose exactly-one-checked invariant lives in the state owner; surfaces merely render it.
- Palette ranking as a receipt fold: because every execution lands in the one result fact stream keyed by intent, recency and frequency ranking for the palette is a fold over receipts — the palette consumes the table plus the receipt stream and owns no state of its own.
- Palette search reads localized headers, so the palette index rebuilds on locale edges — the index is a derived view of (rows × locale), and a stale index after locale switch is the symptom of indexing literals instead of keys.
- The receipt fold's ranking output is exportable typed state — "recent commands" persists through the suite's workspace state like any other typed row, and restores as fold input, not as UI state.
- Trigger-evidence provenance: each receipt embeds the trigger arm's typed evidence (which surface, which gesture, which caller), so per-surface usage analytics, scripted-versus-manual attribution, and abuse gating on remote verbs are all folds over receipts — no surface instruments itself.
- Receipt-driven undo composition: rows whose effects are reversible declare the inverse intent as row data, and the undo journal is a fold over receipts pairing each with its inverse — undo becomes replay of inverse intents through the same trigger union, inheriting gating and receipts, rather than a parallel mutation system.
- Activation-scoped command subscription (viewmodel-law): screen-owned rows are constructed in the constructor but their input subscriptions (selection observers, document listeners) open inside `WhenActivated` — the row survives deactivation with availability frozen at its last replayed value, and re-activation re-binds inputs.
- This split makes dock-tab switching cheap (no command rebuild) and leak-free (no orphan subscriptions); shell-owned global rows never deactivate and must therefore consume only process-lifetime inputs.
- Validation-projection mode shifts (validation-projection): a screen with modes (create/edit/read-only) swaps rule sets through `ValidationHelper` retirement and re-registration on the mode edge — each mode is a declared rule-row set and rule membership is data.
- The read-only mode registering zero rules makes `Valid` trivially true, composing correctly with availability because the submit row is gated off by mode level, not by fake validation.
- Failure-mode taxonomy for the algebra: forever-disabled (non-seeded input), latched-disabled (throwing input), double-busy deadlock (manual `!IsExecuting`), zombie-enabled (a surface bound to a stale command instance after a row rebuild — foreclosed by the frozen-table law: rows never rebuild, their inputs swap), parameter-gated illusion (logic in `CanExecute(parameter)` the bridge never honors), invoke-typed crash (wrong-typed `CommandParameter` surfacing at the menu seam), and null-collapse (a value-typed parameter silently receiving its default from a parameterless surface).
