# options-config — bedrock

## ranked chain

- The default app-configuration chain, weakest to strongest: `appsettings.json` → `appsettings.{environment}.json` → `{applicationName}.settings.json` → `{applicationName}.settings.{environment}.json` → user secrets (development only) → unprefixed environment variables → command line (mounted only when args are non-empty).
- Path separators in the application name normalize to `_` for the per-application settings file pair.
  - Derived file names stay deterministic for nested application names.
- The per-application settings pair is a second JSON axis most chains ignore: machine-shared `appsettings` versus app-owned `{applicationName}.settings` lets a multi-process suite split shared posture from per-process policy with zero custom sources.
- User-secrets mounting resolves the secrets id by loading the application-name assembly's secrets-id attribute; a missing assembly is swallowed silently.
  - Secrets vanish without error when the application name is not a loadable assembly name.
- Suites that rename application identity must therefore verify the secrets layer explicitly — the failure mode is silent absence, not a throw.
- Beneath the app chain sits the host layer: content root, `DOTNET_`-prefixed environment variables, and args.
  - Args participate in host identity before they participate in app configuration, so an identity key passed as an argument wins at both layers consistently.
- All default-mounted JSON files share one reload knob: `hostBuilder:reloadConfigOnChange` (default true), read from the already-mounted host layer.
  - Reload posture is itself a ranked configuration value, settable per environment through the `DOTNET_` layer.
- Per-file source policy (optional, reload-on-change, file provider) is settable through the source-configurator overloads when a file must deviate from the chain default.
  - Source policy is data on the source row, never call-site handling.
- Precedence is per-key last-source-wins, never per-file: a stronger source overrides only the keys it states.
- The chain is a key-level merge fold, and removing a key from a stronger source is impossible.
  - Absence semantics must live in the policy record as explicit off-values, never as key deletion upstream.
- `ConfigurationManager` is simultaneously builder and root: each source `Add` materializes its provider immediately, so a read between mounts sees a partial chain.
- Law: complete the rank before the first typed read; mid-composition reads are admitted only for chain-shaping knobs that by design live in already-mounted layers (the reload knob, identity keys).
- Environment-variable hierarchy uses `__` as the section separator; prefix-scoped mounts strip the prefix before key normalization.
  - A process-scoped layer costs one prefix value.
- Command-line switch mappings translate external argument spellings (`-v`, `--verbosity`) to canonical keys as source policy on the source row.
  - Boundary translation at the seam, canonical keys everywhere inward.
- Key paths are colon-delimited and compared by the configuration key comparer, which orders numeric segments numerically.
  - Array-shaped sections (`items:0`, `items:10`) enumerate in declaration order, not lexicographic order.
- `GetSection` never returns null — an absent section is an empty section; existence checks read child emptiness, and binding an absent section produces an unmodified record, which is why fail-closed binding matters.
- `GetDebugView()` on the root renders every effective key with its winning provider; the overload taking a debug-view-context value processor exists to redact secrets in the rendered view.
- The redacted debug view is the chain's conformance receipt primitive: emit it into the boot receipt and chain disputes become diffable text.
- The flattening enumeration (`AsEnumerable`) is the programmatic twin of the debug view.
  - Key-level chain snapshots for receipts and diffing are one projection call.
- Stream sources mount parsed payloads without file-system coupling — the embedded and test intake for configuration produced by another system; stream sources load once and never reload, so reload-dependent law applies only to file-backed sources.
- `AddConfiguration` mounts an existing configuration tree as a chained source that keeps its own reload propagation.
  - The mechanism for handing a parent process's verified configuration to a child composition without re-ranking, and for composing one suite-shared tree under per-process overrides.
- Reload tokens propagate root-ward: a provider's reload signal fires the root's token once per provider load; the root token is one-shot and re-armed.
  - Consumers re-acquire after every fire, and the re-registering change-token helper is the standard consumption shape.

## binder and source generation

- Binding has exactly three modes — allocate (`Get<T>`/`Get(Type)`), fill (`Bind`, including the named-section form), scalar (`GetValue<T>`/`GetValue(Type, key)`) — and one policy record: `BinderOptions`.
- `ErrorOnUnknownConfiguration` is the fail-closed admission law for policy records: an unrecognized key under the section throws at bind time instead of drifting silently.
  - The configuration-plane analogue of closed-family dispatch, on for every policy bind.
- `BindNonPublicProperties` stays rejected by default — a policy record whose members must be non-public to bind is mis-shaped; the record's public surface is its admission contract.
- The source-generated binder activates through the `EnableConfigurationBindingGenerator` build switch and intercepts binder call sites at compile time.
  - The reflection path never executes, which is what makes bound policy records trim- and AOT-safe.
- The generator's reach defines the admissible policy-record grammar: scalars, nullables, enums, arrays, collections, dictionaries, object graphs, constructor-bound objects; dynamic and reflection-only shapes are structurally rejected.
- Constructor-bound immutable records are first-class — policy records need no settable properties, so the frozen-record shape costs nothing at the binder.
- Generated binding is per-call-site interception: a binder call hidden behind an unconstrained generic indirection escapes interception and silently reverts to reflection.
  - Bind at concrete, visible call sites in the composition assembly.
- Scalar conversion is invariant-culture: numbers and dates in configuration text never vary by machine locale; locale-sensitive parsing belongs to boundary admission with explicit culture values, never the configuration plane.
- Section-shape polymorphism is not a binder feature: a section that varies by case binds as a discriminator key plus per-case sub-sections, admitted into a closed vocabulary after binding.
  - Modeling variance inside one record with nullable clusters is the rejected shape.

## options pipeline

- The materialization pipeline per (type, name) is configure → post-configure → validate, executed lazily by the factory on first access and cached per name.
- The factory runs every configure row in registration order, then every post-configure row, then every validation row.
  - Phase order is absolute; registration order matters only within a phase.
- Unnamed configure rows apply ONLY to the default name: a named options consumer silently misses every bare configure registration.
  - The named-variant family must use named rows or the configure-all wildcard — mixing bare configures with named consumption is the silent-miss trap.
- Validation is a complete pass: every validator runs and failures accumulate across validators into one validation exception per (type, name).
  - The validator layer reports inventories even before the startup sweep aggregates across records.
- The factory constructs a fresh instance per materialization; caching policy lives entirely in the access mode.
  - Singleton caches once, snapshot per scope, monitor per name until invalidated.
- Post-configure is the derivation slot — values computed from other admitted values live there, never in consumers.
- Named options make the name a policy dimension: default name, explicit names, configure-all and post-configure-all wildcards.
  - A bounded variant family is one options type with named registrations, not sibling option types.
- Access modes are a three-row axis: `IOptions<T>` (root singleton, never reloads), `IOptionsSnapshot<T>` (per-scope stable read, named access), `IOptionsMonitor<T>` (live, named, change-notifying).
- The frozen-publish posture below supersedes snapshot and monitor consumption in domain code; the monitor remains the plumbing feeding the publish cell.
- `OptionsBuilder<T>` is the one fluent owner per record: bind, configure (with up to five injected dependencies), validate, validate-on-start in a single chain; `AddOptionsWithValidateOnStart` is the fused registration spelling.
- Scattering `Configure<T>` calls for one record across modules is the rejected form — one record, one builder chain, one residence.
- Dependency-shaped pipeline rows (configure/post-configure/validate forms taking up to five services) make cross-policy derivation a declared dependency row.
  - One record's value computed from another's never reads the provider inside the delegate.
- Validate-on-start mechanics: it registers the startup-validator service idempotently plus a validator-options row keyed by (options type, name).
  - Re-registering the same pair overwrites, so the sweep is naturally deduplicated.
- The sweep materializes each registered pair through the monitor's named get, which also pre-warms the monitor cache.
  - After a successful sweep, first-traffic reads are cache hits; the sweep is both proof and warm-up.
- Sweep failure shape: only the options-validation exception type is collected — one failure rethrows on its original stack, several aggregate; any other exception from a configure delegate escapes immediately as a composition defect, not a validation failure.
- The two failure classes stay distinguishable at the boot seam — validation inventories versus pipeline crashes never blur.
- The sweep executes inside the host's start path ahead of any service start — an invalid policy record means the process never reaches traffic; the precise slot in the ordered sequence is lifecycle-owned and composed here as a consequence.
- Generated validators: the options-validator attribute on a partial class emits a validation implementation from data-annotation constraints with zero reflection.
- `[ValidateObjectMembers]` recurses into nested records — without it nested members are silently unvalidated, the single most common validation hole; `[ValidateEnumeratedItems]` validates per collection element.
- Hand validation rows coexist with the generated validator: the factory runs every registered validator and aggregates.
  - The generated validator owns shape constraints, hand rows own relational and cross-field law, neither restates the other.
- `ValidateOptionsResult` is a three-state value — success, skip, fail (single message or inventory).
  - Skipped is distinct from succeeded: a name-filtered validator returns skip for foreign names, so multi-name registrations compose without false passes.
- `ValidateOptionsResultBuilder` accumulates many failures into one result — validators report inventories, never first-failure.
- The literal-wrapper surface (`OptionsWrapper<T>`, the static create factory) wraps a fixed value as `IOptions<T>`.
  - The test and boundary shim for options-shaped contracts without the pipeline; never a production registration.
- Monitor cache verbs (get-or-add, try-remove, clear) are the manual invalidation surface: a typed "re-read now" transition through the same pipeline.
- The type/instance configure-registration hook registers every options contract a class implements in one call — a class carrying configure, post-configure, and validation for one record is one registration, the dense spelling when pipeline rows share state.
- The secrets store path is deterministically derivable from the secrets id through the path projection — suite tooling locates and seeds developer stores without running the application.
  - Operational re-reads ride these verbs, never a parallel reload mechanism.

## reload and frozen publish

- Change plumbing: provider reload fires the configuration reload token → the options change-token source invalidates the monitor cache for the name → `OnChange` callbacks receive the freshly built value.
- The `OnChange` registration returns a disposable detach token — subscription lifetime is owned, never leaked.
- Nothing in the chain dedupes physical events, and file watchers routinely fire more than once per logical save.
  - Transitions must be coalesced and idempotent, keyed by content identity (hash of the bound record), never by event count.
- The rejected-write hazard: monitor cache invalidation precedes re-validation, so after a bad edit the next get runs the pipeline and throws — the last-good value is already evicted.
- Naive monitor consumption therefore converts a bad config edit into read-path exceptions at every consumer — the structural argument for frozen publish: consumers never read the monitor.
- Frozen-policy publish (the master idiom instantiated): the root owns one atomic cell holding the current frozen policy record; the change callback builds a candidate through the full pipeline.
- The transition is a two-outcome fold — candidate valid → swap the cell and emit an accepted-transition receipt (content hash before/after, changed-key set); candidate invalid → retain the incumbent and emit a rejected-write receipt carrying the validation failure inventory.
- The running system is never degraded by a bad write; rejection is visible only in the receipt stream — exactly where it belongs.
- Reload-receipt content law: receipts carry derived facts — hash identity, key diff computed against the debug view, validation inventory.
  - Never raw configuration values; secrets stay out of the receipt stream by construction, not by filtering.
- Scoped consistency under frozen publish: a unit of work captures the cell's record once at entry and threads it; mid-flight swaps affect the next unit.
  - Snapshot semantics return without the snapshot service, with the consistency boundary explicit in code shape.
- Multi-record coherence (record A's change must land with record B's) is solved by composing one umbrella record over both sections and publishing that.
  - Coherence wider than one cell is the signal the records were one concept wrongly split.

## divergent — sourcegen-binding-validation

- The full AOT-clean admission stack is generator-on-generator: source-generated binding (interceptor) feeds a constructor-bound record validated by a source-generated validator (validator attribute + object-members + enumerated-items recursion) registered with the startup sweep.
  - Four declarations, zero reflection, the sweep proving the whole chain before any service starts.
- Anything the stack cannot express is not a binding problem: model the section as a closed discriminated vocabulary and fold after admission.
  - The binder admits shape, the domain owner admits meaning, and the two-stage admission keeps the binder grammar small forever.
- The unknown-key throw and the startup sweep are complementary closures: the sweep proves stated values satisfy the record's law; the unknown-key throw proves the section states nothing the record cannot carry.
- Both on is the closed-surface posture; either alone leaks one drift class — stale keys survive without the throw, invalid values survive without the sweep.
- Sweep failure shape is worth depending on: one bad record → a typed exception naming type, name, and failure inventory; several → an aggregate carrying all.
  - Boot diagnostics render the complete configuration defect list from one catch, never fix-one-reboot-discover-next.
- Trim/AOT posture is binary per record: a record either rides the generated stack end-to-end or it is a boundary record admitted by hand.
  - A record half-on the generated path (generated bind, reflection validate) has the worst of both and is the rejected hybrid.

## divergent — reload-receipts-law

- Reload classes resolve through one transition fold, not per-class handlers: (accepted, rejected, no-op) × (file edit, manual invalidation, programmatic source mutation).
- The fold's input is always candidate-record-plus-incumbent, so the trigger class is receipt metadata, not control flow.
- A reload that binds to a hash-identical record is the no-op arm and emits nothing — watcher double-fires erase themselves inside the fold, with no debounce timer or event bookkeeping anywhere.
- Republish atomicity: the cell swap is the only mutation, so readers see either the complete old record or the complete new one — partial-update tearing is unrepresentable.
- Receipts double as the staleness guard: a rejected-write receipt followed by no accepted receipt within an operational window is the running-on-stale-but-valid-policy state.
  - Detectable from the receipt stream alone, no extra state machine.
- The publish cell generalizes beyond configuration: any externally-mutated policy surface — feature posture, schedule catalogs, capability grants.
  - Is the same fold: candidate, validate, swap-or-retain, receipt. The reload law is one shape with N sources, which is why it lives here once.

## divergent — symbolic-key-law

- Zero free key literals: every section key derives from the policy type through one pure derivation — `nameof`-rooted, the record type name with a canonical suffix trimmed.
- Member keys are the property names the binder already matches; composite paths build through the configuration path combinator, never string concatenation with `:` literals.
- The derivation function is declared once; every binder call, options-builder bind, debug-view assertion, and receipt key calls it.
- A grep for quoted section names returning anything outside the derivation's own definition is the mechanical defect test — key discipline is auditable as an absence.
- The policy record carries its own key: a static abstract section-key member on the policy-record contract makes "bind this record" a generic one-liner over the record family and surfaces key collisions as a compile-visible review item.
  - Section naming stops being a per-call-site decision.
- External spellings — environment prefixes, command-line switches, foreign trees mounted via chained roots — map to canonical keys exclusively through source policy values at the seam.
- The inward key space stays derivation-pure, so renaming a policy record is one symbol rename plus the seam rows, with the compiler and the debug-view receipt finding every site.
- Reserved-key hygiene rides the same algebra: host identity keys and the reload knob are the only literal-keyed reads in a process, and both live inside platform code.
  - A derivation-pure codebase has exactly zero application-authored key literals.
