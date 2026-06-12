# host-composition — bedrock

## composition root construction

- `HostApplicationBuilder` construction is a fixed eager fold, not a staged callback pipeline: settings → `ConfigurationManager` adoption → default content root + `DOTNET_`-prefixed environment layer (unless `DisableDefaults`) → command-line layer into host configuration → identity keys → hosting-environment materialization → service-collection population → default app configuration, default services, default provider options.
- Everything the staged `IHostBuilder` deferred to delegates happens inside the constructor; when it returns, `Environment`, `Configuration`, `Services`, `Logging`, and `Metrics` are live mutable surfaces.
- Composition modules therefore read real environment and configuration values while contributing — no context-callback indirection, no deferred-availability ceremony.
- The staged `Host.CreateDefaultBuilder` route still exists (`ConfigureHostConfiguration`, `ConfigureAppConfiguration`, `ConfigureServices`, `ConfigureHostOptions`, `UseEnvironment`, `UseContentRoot`, `RunConsoleAsync`); it is the compatibility spelling.
  - The direct builder is the only authored form, and staged delegates appear only when a foreign library demands the staged contract.
- `Build()` is one-shot: a second call throws `InvalidOperationException`.
- `Build()` creates the provider and then calls `MakeReadOnly()` on the service collection — the sealed-after-fold root is structural, not disciplinary: any post-build `Add`/`Remove`/`Insert` on the collection throws.
- No custom freeze guard is ever needed; the platform ships the seal, and `MakeReadOnly` is also directly callable for pre-build staging freezes.
- Host identity is frozen at builder construction: changing the `applicationName`, `environment`, or `contentRoot` configuration keys after the builder exists throws `NotSupportedException`.
- Identity comparison detail: names compare ordinal-ignore-case; content root compares resolved physical paths — restating the same root through a different relative spelling is tolerated, an actual move is not.
- Identity therefore arrives only through `HostApplicationBuilderSettings { DisableDefaults, Args, Configuration, EnvironmentName, ApplicationName, ContentRootPath }`.
  - The settings record is the single identity admission point.
- Passing a pre-populated `ConfigurationManager` through `Settings.Configuration` is the sanctioned way to seed the host layer before defaults stack on it — pre-seeding beats post-mutation, which the freeze forbids.
- `IHostApplicationLifetime` is non-replaceable: the host's constructor downcasts to the internal lifetime implementation and throws `ArgumentException` when a substitute registration won the slot. Lifetime observation composes the contract; lifetime ownership is closed.
- Default content-root selection takes the current directory except when it equals the Windows system directory — the service-manager boot case where cwd is meaningless.
- Service modalities set content root explicitly (`AppContext.BaseDirectory`); relying on cwd in a service row is the rejected form.
- `Host.CreateEmptyApplicationBuilder(settings)` constructs the same surface with zero configuration providers, zero logging providers, no default services, and an unvalidated default provider.
- The empty builder is the explicit-everything boot for embedded and plugin modalities that must not inherit ambient machine state — default-versus-empty is the first modality-keyed decision.
- A staged-contract bridge exists for foreign libraries extending `IHostBuilder`: the adapter records staged delegates and replays them at `Build()`; calling `Build()` on the adapter itself throws.
- Host-configuration delegates replayed through the adapter still hit the identity freeze — the adapter re-checks all three identity keys after replay, so a foreign extension cannot smuggle an identity change.
- The provider-factory seam is replaceable per container: `ConfigureContainer(factory, configure)` swaps the provider-creation function wholesale.
  - A foreign container is one seam swap, and the default factory with explicit options is the same seam exercised with the stock container.
- `HostBuilderContext.Properties` is the only sanctioned cross-module scratch surface during composition; it dies with the builder and never becomes runtime state.
- `HostAbortedException` is the typed bootstrap-abort signal: composition-time tooling that must stop the boot path throws it; it is the one exception class the boot seam treats as intentional rather than defective.
- `ConsoleLifetimeOptions.SuppressStatusMessages` removes the started/stopping console lines — the console modality's only lifetime knob, set when the process owns its own boot reporting.

## descriptor algebra

- `ServiceDescriptor.Describe(serviceType, implementationType, lifetime)` and `DescribeKeyed(serviceType, serviceKey, implementationType, lifetime)` are the complete registration algebra.
- Every `AddSingleton`/`AddScoped`/`AddTransient`/`AddKeyed*`/`TryAdd*` method is sugar over descriptor rows — the add-lifetime overload family collapses to one descriptor constructor plus a lifetime value.
- A module that yields `IEnumerable<ServiceDescriptor>` rows instead of calling extension methods makes its whole contribution enumerable, diffable, and receipt-able before the root folds it.
- Descriptor shapes are a closed three-case family per keyedness: implementation type, factory, instance — keyed variants mirror them exactly.
- Keyed factories receive `(IServiceProvider, object? serviceKey)`: one factory row serves an entire key family because the key is a dispatch input, not a registration multiplier.
- `TryAdd` dedupes on service type alone; `TryAddEnumerable` dedupes on (service type, implementation identity) — the contract for multi-registration families.
- Hosted-service registration is itself a `TryAddEnumerable` singleton row — registering the same hosted type twice yields one instance, and the dedup keys on the implementation type.
- `TryAddEnumerable` boundary: implementation identity must be recoverable from the descriptor; a factory lambda whose delegate type erases to the service type defeats dedup and silently double-registers.
  - Factory rows in enumerable families keep their concrete return type in the delegate's runtime type.
- `Replace` swaps the first matching descriptor; `RemoveAll`/`RemoveAllKeyed` reset a contract.
  - Composition-assembly verbs only; their presence in module code signals a module fighting the fold order instead of declaring rows.
- `KeyedService.AnyKey` is a registration-side wildcard: an AnyKey row answers any key's single resolution and feeds keyed enumerables; it never wins over a specific-key row.
- `null` key bridges keyed registrations to unkeyed resolution — one axis, not two registration worlds.
- `[FromKeyedServices(key)]` selects per constructor parameter; `[ServiceKey]` injects the resolved key value itself.
  - A keyed family can share one implementation type that self-discriminates on the injected key, collapsing per-key subclasses.
- `ServiceKeyLookupMode` closes the key-inheritance axis (explicit key, inherited key, no inherited key): inheritance lets a keyed object graph resolve sub-dependencies under the resolving key without restating the key per registration.
- Scope law: the root provider owns singleton state and every disposable it materializes; transients resolved from the root are retained until process death — the leak class composition review hunts for.
- `CreateAsyncScope` returns a struct scope whose `await using` disposal handles async-only disposables; synchronous scope disposal throws when an async-only disposable is present.
  - Scope creation chooses the disposal world up front.
- `ActivatorUtilities.CreateFactory` compiles and caches a constructor plan for repeated boundary activation of non-registered payload types with partial DI input.
- `CreateInstance` is the one-shot activation form; `GetServiceOrCreateInstance` prefers a registration and falls back to activation — three verbs, one boundary concern.
- `[ActivatorUtilitiesConstructor]` pins constructor selection under overload ambiguity. Activation is boundary work; interior code never calls the activator.

## provider validation

- `ValidateOnBuild` walks every descriptor, constructs its call site, and aggregates one `InvalidOperationException` per defective row into a single `AggregateException`.
  - The complete failure inventory in one throw, never first-failure.
- The aggregate's inner-exception list is the composition root's conformance failure receipt — boot tooling renders the full defect list from one catch.
- `ValidateOnBuild` skips open-generic registrations entirely — only constructed service types validate.
- An open-generic row with an unsatisfiable dependency therefore surfaces at first resolution even under full validation; closed-generic smoke resolutions per open-generic family are the only build-time proof available.
- `ValidateScopes` installs a call-site validator with two firing points: every resolution checks scoped-from-root capture, and combined with `ValidateOnBuild` the build pass also walks call sites.
  - A singleton consuming a scoped dependency is caught at build, before any traffic.
- Default posture: both flags equal the development-environment predicate when defaults are enabled, and both are absent under the empty builder.
- Production roots silently skip validation unless forced; the law is to force both flags in every posture — the cost is one-time at build.
- `IServiceProviderIsService` / `IServiceProviderIsKeyedService` are auto-registered availability probes: the sanctioned way for composition code to branch on contract presence without resolving.
- Runtime probing through `GetService` null-checks is the rejected spelling of the same question — probes answer at composition; null-checks answer at traffic.
- The provider self-registers `IServiceProvider`, `IServiceScopeFactory`, and both availability probes as constant call sites — these four contracts are always resolvable and never need rows.
- AppContext switch `Microsoft.Extensions.DependencyInjection.DisableDynamicEngine` forces the reflection-only resolver.
  - Mandatory posture for hosts inside foreign processes where IL emission is hostile; AOT builds force it implicitly through dynamic-code detection.

## scrutor scan and decoration

- Scan grammar is source → filter → mapping → lifetime → strategy, all package-owned data.
- Sources: `FromApplicationDependencies`, `FromAssemblyDependencies`, `FromDependencyContext`, `FromAssembliesOf`, `FromAssemblies`, `FromTypes` — dependency-closure, marker-type, and explicit-set intake.
- Filters: `AssignableTo`/`AssignableToAny`, `WithAttribute`/`WithoutAttribute`, `InNamespaces`/`NotInNamespaces`, `Where` — assignability, attribute, namespace, and predicate axes.
- Mappings: `AsImplementedInterfaces`, `AsSelf`, `AsSelfWithInterfaces`, `AsMatchingInterface`, `As<T>`, `UsingAttributes`; lifetimes: the three canonical mappings plus `WithLifetime` and keyed `WithServiceKey`.
- Scan output is descriptor rows like any module contribution; scanning is bootstrap-only, the filters are deterministic data, and runtime reflection loops stay rejected.
- `RegistrationStrategy` closes the conflict axis — append, skip, throw, replace — with `ReplacementBehavior` refining what replace matches (service type, implementation type, or both).
- Throw is the sealed-root default strategy: a scan colliding with an explicit row is a composition defect, not tolerable shadowing.
- `ServiceDescriptorAttribute` plus `UsingAttributes` inverts mapping ownership: a type declares its own service contract and lifetime, and the scan honors the declaration.
  - The per-type row for plugin-shaped assemblies where the scanning root cannot know the mapping.
- Decoration mechanics: `Decorate` walks the collection backwards; each matching descriptor moves to a generated keyed registration, and its original slot is replaced in place by a factory descriptor that resolves the keyed original and wraps it via activator construction.
- Decoration consequences: the decorator inherits the original slot's lifetime and registration position exactly; decorator constructor dependencies resolve from the consuming scope; the inner instance remains a first-class keyed service.
- Decoration stacks outward — each call re-wraps the current slot, and keyed shadow rows carry a decorated marker exempting them from re-matching, so N calls produce a clean N-deep onion with last-call-outermost.
- Registration order is stacking order; the root owns it — decorator ordering disputes are resolved by reading the root, not the wrappers.
- `Decorate` throws `DecorationException` on zero matches; `TryDecorate` returns false. The throwing form is the sealed-root default — decorate-before-register is loud, never a silent no-op.
- Decoration is keyed-aware: a strategy carries an optional service key and matches only equal-key descriptors — keyed policy families are decoratable per key.
- Open-generic and closed strategies are distinct: open-generic decoration matches open-generic rows only; closed registrations of the same definition need their own pass.
  - Mixed-registration families require both or the closed rows silently escape.
- `DecoratedService<T>` handles plus `IsDecorated`/`GetDecoratedServices<T>` expose the decoration topology after the fold — decoration depth per contract is an enumerable, checkable conformance fact.
- Decoration interacts with enumerable families: the backwards walk decorates every matching row.
  - Correct for pipeline families, surprising for marker-interface sets; decorate single-contract seams, not enumerable extension sets.

## cli boot surface

- The command-line surface is a parse-then-invoke pipeline with no middleware layer: `RootCommand` → `Command` tree → `Parse` → `ParseResult.Invoke`/`InvokeAsync`.
- The root auto-mounts the help option, version option, and completion directive; `RootCommand.ExecutableName` derives the command name from the running executable.
- `Command.SetAction` overloads close the action axis: sync `Action<ParseResult>` and `Func<ParseResult, int>`, async `Func<ParseResult, CancellationToken, Task>` and `Task<int>`.
- The token-less async overloads exist but are editor-hidden — the API steers every async action to the token form, and the int return is the process exit code.
- `Command.Parse(IReadOnlyList<string>)` and `Parse(string commandLine)` are the two intake shapes; the string form tokenizes shell-style — the embedded and test intake, with the splitter also exposed for tooling.
- `ParserConfiguration` closes the grammar axis: `EnablePosixBundling` (on by default) and `ResponseFileTokenReplacer`.
  - Defaulting to `@file` expansion, null disabling response files; the replacer delegate is the seam for custom token macros.
- `InvocationConfiguration` closes the invocation axis: `EnableDefaultExceptionHandler` (on), `ProcessTerminationTimeout` (default two seconds), and rebindable `Output`/`Error` writers.
- Termination semantics: SIGINT/SIGTERM flow into the action's cancellation token, and termination is forced after the timeout window — CLI actions get one cooperative-then-forced pair natively.
- Headless capture is a writer swap, never console redirection; disabling output is the null writer, and assigning null throws.
- Value access is a typed rail: `GetValue<T>(option|argument|name)` returns default on absence, `GetRequiredValue<T>` throws, `GetResult(symbol|name)` exposes evidence-level results.
- `ParseResult.Errors` is the typed parse-fault list; `UnmatchedTokens` carries the residue when `TreatUnmatchedTokensAsErrors` is relaxed — both are values, never console text.
- Validation attaches as data, not code: `Validators` lists on commands, options, and arguments run against the bound result tree.
- The built-in constraint family — `AcceptOnlyFromAmong`, `AcceptExistingOnly` (file/directory/file-system-info collections), `AcceptLegalFilePathsOnly`, `AcceptLegalFileNamesOnly`.
  - Covers the common admission rows declaratively.
- Per-option admission policy is two delegates: `DefaultValueFactory` and `CustomParser` over the argument result.
  - An option row carries its own default derivation and parse, making option declarations self-contained policy values.
- Option posture knobs are all data on the row: `Required`, `Recursive` (visible to descendant commands), `AllowMultipleArgumentsPerToken`, `Arity`, `HelpName`, aliases.
- Directives are first-class root rows: environment-variable injection and parse-diagram directives are opt-in additions; completion ships by default.
  - A directive is a cross-cutting pre-action keyed by bracket syntax, the CLI's aspect slot.
- CLI-host fusion law: parse before boot. The command action is the only place the host is built and run, so help, version, completion, and parse errors never construct the runtime.
- Args flow into `HostApplicationBuilderSettings.Args` inside the action; the action's cancellation token is passed to the host run call so one signal chain serves both layers; boot failures map to exit codes at this seam and nowhere else.

## divergent — sealed-root-module-algebra

- The maximal root shape: one frozen module-contribution table `moduleKey → (descriptor rows, capability set)` where `moduleKey` is a serializable `nameof`-derived symbol, folded in declared order into one collection, then `Build()` + the native read-only seal.
- Modules never touch the collection; they yield rows. The root is the only caller of any registration spelling, and even there the spelling is the descriptor algebra plus a lifetime value, not the overload family.
- Designed-but-dormant growth case: a planned module is a table row with a real key and an empty descriptor set — declared inert, never stubbed with placeholder implementations.
- The dormant row's presence makes the capability inventory enumerable (receipts state "module X: dormant") and activation is a one-row diff — anticipatory collapse at the composition layer.
- Conformance receipt: because contributions are rows, the receipt is a pure fold over the table before sealing.
  - Counts per lifetime, keyed counts per key family, decoration depth per contract via the decorated-service handles, dormant-module inventory.
- The build-validation aggregate's inner list is the failure half of the same receipt; boot emits exactly one accepted-or-rejected composition receipt.
- The three mutation throws — read-only collection, identity-key freeze, lifetime-contract replacement.
  - Make post-seal drift structurally impossible; the only mutable surface surviving the seal is state the root deliberately registered as a cell.
- Ambient capabilities (clock authorities, culture policy, correlation root) are rows in this table threaded as constructor-visible values; any static read of them downstream is the named universal defect, and the table is where the defect is auditable.
  - The capability either has a row or the code lies.

## divergent — scrutor-depth-provider-validation

- Failure taxonomy with detection points, ordered by how early each class can be caught: unconstructible dependency → aggregated at build; singleton-over-scoped capture → build under both flags, else first resolution; open-generic defect → first resolution always, mitigated only by closed smoke resolution; decoration zero-match → throw at composition; scan collision → throw under the throw strategy at composition; post-seal mutation → read-only throw; identity mutation → not-supported throw at replay; lifetime-contract replacement → argument throw at host materialization.
- The lane's whole job is routing every failure class to its earliest detector; anything movable from first-resolution to build moves.
- Keyed decoration plus keyed factories compose into a policy plane: one keyed factory row constructs per-key variants, one keyed decoration strategy wraps a chosen key, consuming constructors select with the keyed-services attribute.
  - The entire variant family is three declarations regardless of key count.
- Scan-then-decorate ordering is a root invariant: all module and scan rows fold first, decoration passes run last, and the throwing forms turn ordering mistakes into boot failures.
  - The fold order is self-verifying, no review checklist required.
- The empty-builder + forced-validation + disabled-dynamic-engine triple is the embedded posture's provider row: explicit sources, full build proof, no IL emission.
  - Three values on one options record, not three mechanisms.

## divergent — modality-boot-variants

- One closed process-modality axis (service, console, embedded, plugin, companion, cli, headless) resolved by a posture-keyed default table.
- Each modality row carries: builder factory (default versus empty), lifetime registration row, configuration posture, validation posture, signal ownership.
  - One fold over the row replaces every per-modality bootstrap file, and a new modality is one row.
- Service-manager lifetimes are environment-gated registrations, not branches: the systemd and service-control registrations both no-op unless their probe fires, so both rows register unconditionally and at most one activates.
- Probe-folded composition is the zero-scaffolding mechanism the modality law requires — the modality is never tested in application code.
- The probes are one-time static evaluations: the systemd probe accepts PID-1 with a notify socket or listen-PID, otherwise compares the parent process name against the init daemon; the service-control probe detects the manager parent.
- Probe results are cached for process lifetime — modality is immutable per process, which is what licenses folding it into composition rather than runtime dispatch.
- Embedded/plugin rows: the variance point is the `IHostLifetime` contract itself.
  - `WaitForStartAsync` gates start and its stop hook runs last at shutdown, so the embedded row registers a pass-through lifetime (both members complete immediately) and the owning process drives start/stop directly.
- An embedded host then owns no process signals — mandatory when the surrounding process already owns them; the embedded row composes three values: empty builder, pass-through lifetime, dynamic-engine kill switch.
- The plugin row adds collection-isolation posture: no dependency-context scanning — explicit assembly rows only, because the dependency context describes the wrong application.
- The cli row composes the parse-then-invoke pipeline as the boot gate; the headless row is the cli row with swapped writers and no lifetime.
- Companion/sidecar rows are the service row with verification-first boot — peer presence verified at start, absence folding to the degradation rail rather than failing the boot.
- Every row resolves to values consumed by one fold — no modality ever introduces a new composition function, and the table's row count is the complete answer to "what process shapes exist".
