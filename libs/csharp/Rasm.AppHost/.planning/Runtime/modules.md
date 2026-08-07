# [APPHOST_COMPOSITION_AND_MODULES]

One composition root per process folds a frozen module table into the service graph, arms every seam the corpus declares must bind, and freezes it. Composition owns four axes: the `ModuleContribution` row — assembly, scan, descriptor, contributor, registrar, and decoration columns — the one-pass receipted composition fold with its port-cardinality and decoration admissions, admission-boundary activation carrying availability probing, async-scope ownership, keyed decoration introspection, and validator discovery, and the MUST-BIND ledger whose rows are the root's own call sites for the seams no module row reaches. One descriptor algebra serves every seam: `ServiceDescriptor.Describe` and `DescribeKeyed` rows carry registrations, and `TryAddEnumerable` ordered sets carry every fan-in family. The package spine is `Microsoft.Extensions.DependencyInjection` with `Scrutor` scanning and decoration, `FluentValidation.DependencyInjectionExtensions` validator discovery at the root, and `System.CommandLine` as the app-root verb boundary — one `ParseResult`-driven projection onto the existing owners, never a second dispatcher.

## [01]-[INDEX]

- [02]-[MODULE_TABLE]: Frozen contribution rows with one descriptor algebra for every fan-in seam.
- [03]-[SCAN_AND_DECORATE]: One-pass scan, decoration, and keyed registration fold with receipted freeze.
- [04]-[BOUNDARY_ACTIVATION]: Activation plans, availability probes, async scopes, keyed decoration, and validators.
- [05]-[APP_ROOT_VERBS]: The `System.CommandLine` verb table — seed DATA projecting `ParseResult` onto existing owners.
- [06]-[BINDING_LEDGER]: The must-bind seam roster and the two-altitude fold that is every row's one call site.

## [02]-[MODULE_TABLE]

- Owner: `ModuleContribution` — the frozen per-process module-table row; modules contribute registrations and never resolve services.
- Auto: `Contributors` rows apply through `TryAddEnumerable` — one ordered descriptor algebra carries every multi-implementation fan-in family.
- Receipt: `ContributionReceipt` — per-module scan, lifetime, keyed, default, contributor, registrar, and decoration counts, materialized at the fold edge.
- Packages: Microsoft.Extensions.DependencyInjection
- Growth: one module row per contributing package, one descriptor row per service; zero new surface.
- Boundary: descriptor construction spells `ServiceDescriptor.Describe` and `DescribeKeyed` only — the `AddSingleton`/`AddScoped`/`AddTransient` and `AddKeyedSingleton`/`AddKeyedScoped`/`AddKeyedTransient` overload families are the deleted spellings.

Row law:
- One composition root per process folds the table; packages ship rows into it. A per-package registration extension, a module interface with configure members, and an event-style registration hook are the deleted patterns — the row is the whole module contract.
- Table order is semantic: a registrar that wraps a sibling module's contract sits in a later row than the contract it wraps, and the fold preserves declaration order end to end.
- `Services` carries unkeyed `Describe` rows; `Keyed` carries `DescribeKeyed` rows whose keys are smart-enum policy values from the owning vocabulary pages; `Contributors` carries the ordered fan-in sets — health, support, drain, and telemetry contributor families register here, never through a bespoke aggregator contract.
- `Defaults` carries idempotent fallback rows: a package-shipped default whose contract a host or later module may pre-empt applies through `TryAdd` for unkeyed rows and `TryAddKeyedSingleton`/`TryAddKeyedScoped`/`TryAddKeyedTransient` for keyed rows, so the first registration of a contract wins and a duplicate default is a no-op — never a silent second descriptor competing at resolution. A default that must override an earlier registration stays a `Services` `Describe` row; `Defaults` is the additive-only floor.
- `FromKeyedServicesAttribute` binds keyed constructor parameters, `ServiceKeyAttribute` injects the resolved key into the implementation, and `KeyedService.AnyKey` selects keyed enumerables and never resolves a single service.
- `Registrars` carries collection-shaped package registrations that no descriptor spelling expresses — the validator-discovery row and other collection-shaped admissions — each a `Func<IServiceCollection, IServiceCollection>` applied after the module's descriptor rows.
- `Decorations` carries the typed decoration column: each entry is one `DecorationRow` application naming the inner service contract and the wrapping decorator, so the decoration topology is data the fold reads and the receipt counts, never an opaque registrar `Func`. A profile that drops a contributor port carries the entry with `Conditional: true`, so the same column decorates on the service profile and skips on the plugin profile by `TryDecorate` row presence.
- The `Scan` column is `Option`-typed: a row constructed with `Scan: default` composes through explicit descriptor rows alone. The web and AOT module tables construct every row that way — the same table, zero parallel composition system, and the column flip is the growth proof.

```csharp signature
public sealed record ModuleContribution(
    string Module,
    Assembly Assembly,
    Option<Action<IImplementationTypeSelector>> Scan,
    Seq<ServiceDescriptor> Services,
    Seq<ServiceDescriptor> Keyed,
    Seq<ServiceDescriptor> Defaults,
    Seq<ServiceDescriptor> Contributors,
    Seq<Func<IServiceCollection, IServiceCollection>> Registrars,
    Seq<DecorationRow> Decorations);

public readonly record struct DecorationRow(Type Service, Type Decorator, bool Conditional);

public readonly record struct ContributionReceipt(
    string Module,
    int Scanned,
    int Singletons,
    int Scoped,
    int Transients,
    int Keyed,
    int Defaults,
    int Contributors,
    int Registrars,
    int Decorated);
```

Module keys are `nameof`-derived assembly symbols, never free literals; the receipt's `Module` field repeats the row key so receipt streams group by module without positional reconstruction.

## [03]-[SCAN_AND_DECORATE]

- Owner: `CompositionSurface` — one fold composes scan, descriptor admission, port-cardinality admission, decoration, and freeze in one pass over the table.
- Entry: `Fin<Seq<ContributionReceipt>> Compose(params ReadOnlySpan<ModuleContribution> modules)` — `Fin` aborts on the first rejected module with module provenance in the failure, whether the rejection was thrown by the scan or railed by an admission.
- Auto: `MakeReadOnly` freezes the collection after the fold; `BuildServiceProvider` under `ServiceProviderOptions` with `ValidateOnBuild` and `ValidateScopes` proves the frozen graph on the test row.
- Packages: Scrutor, Microsoft.Extensions.DependencyInjection
- Growth: one scan filter row or one registrar row per cross-cutting concern; zero new surface — the fold absorbs it.
- Boundary: `Applied` is the composition-root boundary capsule — `Scrutor` scan, descriptor admission, and registrar application are host-owned statement seams, and the statement carve-out names this fence; the `Runtime/ports#PORT_RECORDS` eighth-port refusal EXECUTES here and nowhere else, so the mandate and its enforcement are one seat and a contributor descriptor naming no cardinality row refuses while the collection is still editable rather than surfacing later as a leaked inward dependency.

Pass law:
- Scan sources are `FromAssemblies` over the row's explicit `Assembly`. `FromApplicationDependencies` and `FromDependencyContext` walk the default dependency closure and are the deleted sources: plugin load contexts never appear in that closure, so closure-walking scans silently miss every plugin assembly.
- Selection composes `AddClasses`, then `AssignableTo`, `WithAttribute`, and `InNamespaces` filters, then mapping: `UsingAttributes` maps `ServiceDescriptorAttribute`-annotated classes, `AsImplementedInterfaces` and `AsSelfWithInterfaces` map the rest, and `WithLifetime` and `WithServiceKey` bind lifetime and key inside the same pass.
- Duplicate registrations resolve under `UsingRegistrationStrategy(RegistrationStrategy.Throw)` bound inside the same `Scan` pass; the thrown rejection captures into the rail as conflict evidence carrying the module key — never a silent append, never a silent replace. `RegistrationStrategy.Replace` survives only as an explicit row-level policy on a row that names the contract it overrides.
- `Defaults` apply through the package's own roster overload: `TryAdd` compares `(ServiceType, ServiceKey)` and never reads an implementation type, so the keyed and unkeyed arms are one call and the deleted lifetime switch was both redundant and narrower — it dereferenced `KeyedImplementationType`, which is null on every keyed factory and keyed instance descriptor the Row law admits.
- `Contributors` enter through `PortCardinality.Of` before `TryAddEnumerable`, accumulating so one boot names every unrostered service type rather than the first; the admission runs per module because the port set is a package-wide invariant a module row is the thing that can violate.
- The `Decorations` column applies before registrars through `BoundaryActivation.Decorate`, wrapping contributor ports with telemetry and receipt decoration; the decorated contract stays the public contract, and the `Conditional` flag selects `TryDecorate` on a profile-conditional target. A `Conditional: false` entry whose target reports undecorated refuses on the rail — a count alone leaves the composition defect for a reader to notice — and the surviving count is the receipt's `Decorated` column. Decoration owns this cluster's keyed-decoration pass-law; `BOUNDARY_ACTIVATION` owns the decoration introspection.
- Registration is bootstrap-only: after `MakeReadOnly`, descriptor mutation throws, so every late registration attempt surfaces at the root instead of drifting into runtime state.

```csharp signature
public static class CompositionSurface {
    extension(ServiceCollection services) {
        public Fin<Seq<ContributionReceipt>> Compose(params ReadOnlySpan<ModuleContribution> modules) =>
            Iterable<ModuleContribution>.FromSpan(modules)
                // The self-flattening bind collapses the capture rail into the module's own admission rail, so
                // a thrown scan conflict and a railed cardinality refusal both leave carrying the module key.
                .TraverseM(module => Try.lift(() => Applied(services, module)).Run()
                    .Bind(static admitted => admitted)
                    .MapFail(error => Error.New($"<module-rejected:{module.Module}:{error.Message}>")))
                .As()
                .Map(receipts => (fun(services.MakeReadOnly)(), receipts.ToSeq()).Item2);
    }

    private static Fin<ContributionReceipt> Applied(IServiceCollection services, ModuleContribution module) {
        int admitted = services.Count;
        module.Scan.IfSome(select => services.Scan(source => select(source.FromAssemblies(module.Assembly))));
        int scanned = services.Count - admitted;
        module.Services.Iter(services.Add);
        module.Keyed.Iter(services.Add);
        services.TryAdd(module.Defaults);
        return Contributed(services, module.Contributors).Bind(_ => {
            module.Decorations.Iter(decoration => BoundaryActivation.Decorate(services, decoration));
            ignore(module.Registrars.Fold(services, static (current, registrar) => registrar(current)));
            return Decorated(services, module.Decorations);
        }).Map(decorated => new ContributionReceipt(
            Module: module.Module,
            Scanned: scanned,
            Singletons: Lifetimes(module, ServiceLifetime.Singleton),
            Scoped: Lifetimes(module, ServiceLifetime.Scoped),
            Transients: Lifetimes(module, ServiceLifetime.Transient),
            Keyed: module.Keyed.Count,
            Defaults: module.Defaults.Count,
            Contributors: module.Contributors.Count,
            Registrars: module.Registrars.Count,
            Decorated: decorated));
    }

    // The eighth-port refusal's one execution site: every contributor descriptor resolves a `PortCardinality`
    // row by its service-type name before it joins the ordered set, so the invariant `Runtime/ports` declares
    // is proven where the seam is still editable. Accumulating, so a module adding three foreign ports names
    // three rather than one per boot attempt.
    private static Fin<Unit> Contributed(IServiceCollection services, Seq<ServiceDescriptor> rows) =>
        rows.Traverse(row => PortCardinality.Of(row.ServiceType.Name).ToValidation())
            .As()
            .ToFin()
            .Map(_ => rows.Iter(services.TryAddEnumerable));

    // The decoration proof: an unconditional row whose contract the frozen collection does not wrap is a
    // composition defect, so the pass yields a refusal and the confirmed count in one fold rather than a
    // number a reader compares against the declaration by hand.
    private static Fin<int> Decorated(IServiceCollection services, Seq<DecorationRow> rows) =>
        rows.Traverse(row => row.Conditional || services.IsDecorated(row.Service)
                ? Validation<Error, Unit>.Success(unit)
                : new Fault.InvalidValue(Label: row.Service.Name, Requirement: "<a decorated contract>"))
            .As()
            .Map(_ => rows.Filter(row => services.IsDecorated(row.Service)).Count)
            .ToFin();

    private static int Lifetimes(ModuleContribution module, ServiceLifetime lifetime) =>
        (module.Services + module.Keyed + module.Defaults).Filter(row => row.Lifetime == lifetime).Count;
}
```

The fold is the only writer of the collection: scan first inside each module so the scanned count derives from the collection delta, descriptor rows next, contributor admission before the ordered set it guards, decorations after the descriptors so every `DecorationRow` finds its target contract within the module or in an earlier row, registrars last.

## [04]-[BOUNDARY_ACTIVATION]

- Owner: `BoundaryActivation` — admission-edge activation, availability probing, async-scope ownership, keyed decoration, and validator discovery over the frozen graph.
- Entry: `Fin<T> Activate<T>(params object[] dependencies)` — empty arity resolves the registered contract, supplied arity invokes the cached constructor plan.
- Packages: Microsoft.Extensions.DependencyInjection, Scrutor, FluentValidation.DependencyInjectionExtensions
- Growth: one validator assembly row per discovering package, one cached plan per boundary-constructed type, one `TryDecorate` row per profile-conditional contributor port; zero new surface.
- Boundary: activation sits at admission boundaries only — interior code receives constructor dependencies and frozen policy records, and a provider lookup inside domain flow is the deleted service-location pattern; `Available` probes through `IServiceProviderIsService`/`IServiceProviderIsKeyedService` instead of a resolve-and-catch, and the deleted form is the `GetService<T>()` null check; `Scoped` opens an `AsyncServiceScope` through `CreateAsyncScope`, and a synchronous `CreateScope` at a drain boundary is the deleted form because scoped disposables there `DisposeAsync` under the conductor token; a multi-constructor boundary type pins its activation constructor with `[ActivatorUtilitiesConstructor]`, so the cached `CreateFactory` plan binds the declared signature rather than constructor-greediness inference.

Activation law:
- Empty arity routes through `GetServiceOrCreateInstance` — registered contract first, constructed instance second — so optional host contracts admit without a parallel probe entrypoint.
- Supplied arity routes through the `ActivatorUtilities.CreateFactory(Type, Type[])` plan cached per boundary type — the returned `ObjectFactory` delegate invokes as `(IServiceProvider, object?[]?) -> object`; the plan's argument vector derives from the first admission, so a boundary-constructed type owns exactly one explicit-dependency shape, and a second shape for the same type is a row on a new type, never an overload. A boundary type carrying more than one constructor pins the activation constructor with `[ActivatorUtilitiesConstructor]` so the factory plan binds the intended signature deterministically rather than the greediest-resolvable one; a boundary type with one constructor needs no marker.
- `Available` answers admission questions before construction: `IServiceProviderIsService.IsService` for unkeyed contracts and `IServiceProviderIsKeyedService.IsKeyedService` for smart-enum-keyed ports, so an optional host contract admits through one probe instead of a resolve-then-rescue pair; the probe never resolves, so it is legal in admission flow where a resolve is not.
- `Scoped` owns the async drain-scope shape: `CreateAsyncScope` returns the `AsyncServiceScope` whose `ServiceProvider` resolves the boundary graph and whose `DisposeAsync` runs under the supplied conductor token, so scoped disposables flush inside the drain band instead of on a finalizer thread.
- Every activation failure converts at this seam: the capture funnel projects construction rejections into the rail with the target type name, and no raw activation exception crosses inward.
- `AddValidatorsFromAssemblies` discovers validators with an explicit `ServiceLifetime` and a deterministic `AssemblyScanner.AssemblyScanResult` filter; `includeInternalTypes` stays `false`, so public validators are the admitted set. The produced delegate enters the module table as one `Registrars` row — validator discovery owns no second registration path.

Decoration pass-law:
- `Decorate` applies one `DecorationRow` column entry over the collection: a `Conditional: false` entry spells `Decorate(serviceType, decoratorType)` on a contract guaranteed present, and a `Conditional: true` entry spells `TryDecorate(serviceType, decoratorType)` so a profile where the inner port is absent decorates nothing rather than failing. The same module table decorates a contributor port on the service profile and skips it on the plugin profile by entry presence and the `Conditional` flag, never by a runtime branch at a call site.
- The decorated contract stays the public contract: a decorated port resolves to the decorator, and the decorator resolves the inner registration through the generated `DecoratedService<TService>` handle, so a third decoration wraps the second with no registration rewrite. Keyed contributor ports decorate by their smart-enum service key, so decoration composes per key without a parallel keyed-decoration path.
- `Decorated` folds the pass into the receipt without a hand-kept tally: `IsDecorated(serviceType)` confirms the frozen collection wraps each `DecorationRow.Service`, so `ContributionReceipt.Decorated` counts confirmed targets from the graph, and `Decorated<TService>` over `GetDecoratedServices<TService>` enumerates the decorated descriptors for graph introspection; a `Conditional: false` entry whose `Service` reports undecorated is the composition defect this fold surfaces.

```csharp signature
public static class BoundaryActivation {
    private static readonly ConcurrentDictionary<Type, ObjectFactory> Plans = new();

    extension(IServiceProvider provider) {
        public Fin<T> Activate<T>(params object[] dependencies) where T : notnull =>
            Try.lift(() => dependencies.Length == 0
                    ? ActivatorUtilities.GetServiceOrCreateInstance<T>(provider)
                    : (T)Plans.GetOrAdd(
                            typeof(T),
                            static (_, supplied) => ActivatorUtilities.CreateFactory(
                                typeof(T),
                                [.. supplied.Select(static value => value.GetType())]),
                            dependencies)
                        .Invoke(provider, dependencies)!)
                .Run()
                .MapFail(error => Error.New($"<activation-rejected:{typeof(T).Name}:{error.Message}>"));

        public bool Available<T>(Option<object> key = default) where T : notnull =>
            key.Case is object serviceKey
                ? provider.GetRequiredService<IServiceProviderIsKeyedService>().IsKeyedService(typeof(T), serviceKey)
                : provider.GetRequiredService<IServiceProviderIsService>().IsService(typeof(T));

        public AsyncServiceScope Scoped() => provider.CreateAsyncScope();
    }

    extension(IServiceCollection services) {
        public int Decorated<T>() where T : notnull =>
            services.GetDecoratedServices<T>().Count();
    }

    public static IServiceCollection Decorate(IServiceCollection services, DecorationRow row) =>
        row.Conditional
            ? (services.TryDecorate(row.Service, row.Decorator), services).Item2
            : (services.Decorate(row.Service, row.Decorator), services).Item2;

    public static Func<IServiceCollection, IServiceCollection> ValidatorRow(
        Assembly assembly,
        ServiceLifetime lifetime,
        Func<AssemblyScanner.AssemblyScanResult, bool> filter) =>
        collection => collection.AddValidatorsFromAssemblies(
            [assembly],
            lifetime,
            filter,
            includeInternalTypes: false);
}
```

## [05]-[APP_ROOT_VERBS]

- Owner: `VerbRow` the seed-DATA verb table row; `AppRootVerbs` the one CLI boundary adapter mounting the table onto a `RootCommand`.
- Cases: canonical rows — `dispatch` projects a descriptor + serialized arguments onto `Agent/runtime#DISPATCH_FRONT_DOOR` `CommandDispatch.Run`; `replay` and `bisect` are the `Runtime/determinism` ingress (the `ChangefeedPort.Load` windowed read feeding `ReplayVerify.Replay`/`AdversarialProbe.Bisect`); `capture-support` admits one `SupportTrigger.ExternalCommand` onto the `Observability/bundles` capture fan — every host modality that also carries the ControlService verbs shares these exact owners, so the CLI is a projection, never a parallel verb semantics.
- Entry: `Mount(string description, Seq<VerbRow> rows)` returns `RootCommand` — the table mounts once at the app root; each row's `Command.SetAction(Func<ParseResult, CancellationToken, Task<int>>)` binds the projection; `ParseResult.GetValue<T>(Option<T>)`/`GetValue<T>(Argument<T>)` are the only argument reads.
- Packages: System.CommandLine, LanguageExt.Core, BCL inbox
- Growth: a new operator verb is one `VerbRow` in the table projecting onto an existing owner; a verb whose owner does not exist yet is a missing case on the owning page, never a CLI-local body; zero new surface.
- Boundary: the verb table is a BOUNDARY ADAPTER — every row's body is one projection into a composed owner (`CommandDispatch.Run`, the determinism port, the capture trigger) and a verb carrying domain logic of its own is the deleted form; `AppRootVerbs.Mount` is the named boundary capsule for the statement carve-out (the `RootCommand` mutation seam); a rejected parse never reaches a row's action because a non-empty `ParseResult.Errors` blocks invocation by the package's own contract, so parse failure is DATA the host entry projects to an exit code and a thrown parse has no spelling here; the ControlService verbs stay the service-modality wire route — the CLI row and the control verb project onto the SAME owner so an operator at a terminal and an operator over the control hop invoke one semantics; removal of this table is legal only on proof every verb rides ControlService for every host modality.

```csharp signature
// Seed DATA: one verb row per operator concern, each a projection onto an existing owner.
public sealed record VerbRow(Command Command, Func<ParseResult, CancellationToken, Task<int>> Project);

public static class AppRootVerbs {
    // Named boundary capsule: RootCommand mutation is the host-owned statement seam.
    public static RootCommand Mount(string description, Seq<VerbRow> rows) {
        var root = new RootCommand(description);
        rows.Iter(row => { row.Command.SetAction(row.Project); root.Add(row.Command); });
        return root;
    }

    public static VerbRow Dispatch(DispatchRuntime runtime, Func<string, string, Fin<CommandIntent>> intentOf) {
        var descriptor = new Argument<string>("descriptor");
        var arguments = new Option<string>("--arguments", "-a");
        var command = new Command("dispatch", "run one capability through the command front door") { descriptor, arguments };
        return new(command, (parse, token) =>
            intentOf(parse.GetValue(descriptor)!, parse.GetValue(arguments) ?? "{}")
                .Match(
                    Succ: intent => CommandDispatch.Run(runtime, intent).RunAsync().AsTask()
                        .ContinueWith(static run => run.IsCompletedSuccessfully && run.Result.Txn is CommandTxn.Committed ? 0 : 1, token),
                    Fail: error => Task.FromResult((int)error.Code)));
    }

    public static VerbRow Replay(ReplayRuntime runtime, ChangefeedPort port, DeterminismContext live) {
        var origin = new Option<Guid>("--origin");
        var from = new Option<long>("--from");
        var to = new Option<long>("--to");
        var command = new Command("replay", "rehydrate a recorded chain from the durable store and replay-verify it") { origin, from, to };
        return new(command, (parse, token) =>
            port.Load(new ChangefeedWindow(parse.GetValue(origin), parse.GetValue(from), parse.GetValue(to)))
                .Match(
                    Succ: log => ReplayVerify.Replay(runtime, log, live).RunAsync().AsTask()
                        .ContinueWith(static run => run.Result.Exists(static o => o is not ReplayOutcome.Matched) ? 1 : 0, token),
                    Fail: error => Task.FromResult((int)error.Code)));
    }

    // The bisect row rides the SAME windowed read the replay row does — one durable ingress, two probes — so
    // an operator narrowing a divergence never re-records a run to find it. The clean verdict is the HASH
    // EQUALITY, never the sequence: a clean chain yields the genesis-pair sentinel whose halves agree, while a
    // real divergence carries two differing hashes by construction, so sequence zero stays a legal finding.
    public static VerbRow Bisect(ChangefeedPort port, Func<LogEntry, ChainHash> rederive) {
        var origin = new Option<Guid>("--origin");
        var from = new Option<long>("--from");
        var to = new Option<long>("--to");
        var command = new Command("bisect", "binary-search a recorded chain for its first divergent step") { origin, from, to };
        return new(command, (parse, _) =>
            port.Load(new ChangefeedWindow(parse.GetValue(origin), parse.GetValue(from), parse.GetValue(to)))
                .Map(log => AdversarialProbe.Bisect(log, rederive))
                .Match(
                    Succ: divergence => Task.FromResult(divergence.Recorded == divergence.Rederived ? 0 : 1),
                    Fail: error => Task.FromResult((int)error.Code)));
    }

    public static VerbRow CaptureSupport(CorrelationId correlation, Func<SupportTrigger, IO<Unit>> capture) {
        var reason = new Option<string>("--reason");
        var command = new Command("capture-support", "admit one external-command support capture") { reason };
        return new(command, (parse, token) =>
            capture(new SupportTrigger.ExternalCommand(correlation, parse.GetValue(reason) ?? string.Empty))
                .RunAsync().AsTask().ContinueWith(static run => run.IsCompletedSuccessfully ? 0 : 1, token));
    }
}
```

## [06]-[BINDING_LEDGER]

- Owner: `RootBinding` `[Union]` the two-altitude must-bind seam row; `RootInputs` the composed values every row reads; `CompositionRoot` the static ledger, the measured `CapsulePins` load-context roster, and the one fold that is each row's call site.
- Cases: `Seated` binds while the collection is still editable; `Proven` runs against the BUILT provider because the fact it needs — a materialized pipeline, an issued token — does not exist before the build.
- Entry: `Arm(ServiceCollection services, RootInputs inputs, ServiceProviderOptions options, params ReadOnlySpan<ModuleContribution> modules)` returns `Fin<(IServiceProvider Provider, Seq<ContributionReceipt> Receipts)>` — folds every `Seated` row, folds the module table, freezes, builds, then folds every `Proven` row, so a boot either yields a provider whose declared seams are all armed or names the seam that refused.
- Auto: the ledger is DATA, so a page declaring a new must-bind seam adds one row and no fold body changes; the root's own seams seat AHEAD of the module table so a module row decorating a platform contract finds it already registered, and `Compose`'s `MakeReadOnly` stays the one freeze — a ledger row after it throws at the append rather than registers; both folds accumulate per row on the rail, so one boot names every unbound seam instead of one per attempt; `redaction-and-sampling` reaches the log chain through `SignalGovernance.GovernLogs`, whose `RedactionRegistration.Bind` carries every redactor row — sealing the chain without it leaves the erasing fallback as the ONLY resolution and every classified tag erases, including the operational dimensions the pass rows exist to spare; `latency-context` seats the pooled provider, issuer, and the ONE `Func<ILatencyContext>` factory the three threading folds read, and `latency-names` folds this root's roster with every contributed `LatencyRoster` into the single registration under `ThrowOnUnregisteredNames`, because an unregistered name resolves to a positionless token whose writes drop with nothing raised; `lane-guard` registers every lane pipeline and hands the `LanePermits` cell that makes the adaptive resize a live seam rather than a permit column frozen at build; `changefeed` constructs the `ChangefeedPort` from the Persistence changefeed delegates and seats it so `EventLog.Append` mints and publishes in one motion; `child-residual` seats the `Func<CompanionPeer, IO<int>>` census delegate `SandboxRuntime.ChildResidual` reads, so a process eviction's `Residual` count is an independent measurement and an unbound census reads as an unproven kill rather than a silent zero; `drain-thread` seats the `Func<DrainThread>` mint so every drain-gated rollover receives the conductor's whole telemetry tail — a context minted per drain through `LatencySpine.Open`, its resolved checkpoint token, the mounted instrument set, the two deadline rows, and the `ILatencyDataExporter` the terminal `Seal` feeds — from the one root, and an unbound tail refuses at boot instead of compiling on a trailing default; `lifecycle` constructs the phase capsule over the composed `HookRail`'s own `Phase` point, so the one shielded fan-out exists before the capsule that fires it and an observer seating real I/O can never unwind past the transition rail; `verdict-fallback` binds `FlagVerdict.Inert` as the whole verdict function so an absent features rail and an unready provider answer ONE shape at every consumer; `lane-closure` runs the roster probe on the built provider, where a pipeline first exists; `hop-checkpoint` issues the hop token from the built issuer so a name the folded roster never carried refuses at boot instead of resolving the positionless token whose writes drop; `capsule-unload` proves the measured ALC pin set is container-owned, under the `InHost` topology alone because every other topology unloads at process exit; `membership` seats the cluster view over the resolver, the per-authority `UriHealthCheck`, and the three decoded membership arrows, and `peer-roster` seats the local attach set whose `contribute` closure is the two-tier edge `Wire/companion` and `Wire/coordination` both declare — the two rows reference each other only inside lambdas the fold never runs, so the mutual read resolves after both are registered rather than against a graph still being written, and the peer endpoint projects from the manifest's own `SocketPath` through `UnixDomainSocketEndPoint` so the UDS contract carries one address rather than two encodings obliged to agree; `control-inbound` completes `ControlRuntime` with the four columns the control hop adds — the command front door, the descriptor-classification lookup, the redactor provider, and the composition's minted source — over the `ControlSeed` its config, bundles, telemetry, and ports owners fill, because composing their halves here absorbs three pages' laws into one ledger row.
- Receipt: `Arm` yields the module `ContributionReceipt` sequence beside the provider; a ledger row mints no receipt of its own — its evidence is the boot that either produced a provider or named the refusing seam.
- Packages: Microsoft.Extensions.DependencyInjection, Polly.Extensions, LanguageExt.Core, BCL inbox
- Growth: a new must-bind seam is one `RootBinding` row at its altitude; a new composed value the rows read is one `RootInputs` column, and a value whose halves belong to other owners is one seed record beside it — `CoordinationArrows` and `ControlSeed` are that shape, so an owner's law reaches this fold as a filled column rather than a re-implementation; a member measured to pin a collectible load context is one `CapsulePins` row the unload proof already folds; zero new surface.
- Boundary: this ledger is the ONE composition-scoped call site for every seam a page declares and no ordinary consumer reaches, so a declared-and-unbound seam is a missing row rather than a sentence a reader audits against the corpus — the class the ledger exists to close is a `Bind`, `Register`, or `Of` member with a page-long law behind it and zero callers. Runtime-scoped PRODUCERS never appear here and the two altitudes never trade: an instrument write, a dispatched tool, a continued trace binds at its own producing arm, because hoisting one into this fold fires it once at boot where the law wants it per event, and sinking a composition binding into a producing arm re-registers on a frozen collection; the `PortCardinality.Of` contributor admission stays inside `SCAN_AND_DECORATE`'s per-module fold for the same reason — the module row is the thing that can violate the port invariant, so the admission belongs at the row and not at the root; a row's delegate composes an owner and never re-implements one, so the ledger holds no logic of its own and a body doing work past one composed call is the deleted form; the `ILatencyContext` factory is the composition's single mint — `DrainConductor.Drain`, `OutboundSurface.Run`, and `SupportCapture.Capture` each take the context as a parameter, so a fold minting its own context, or timing a phase off a `Stopwatch`, is the deleted form; `LatencySpine.Register` owns the NAME table alone while this ledger owns the provider registration, so the option that gates the issuer sets once at this seat; capsule unload is a MEMBERSHIP proof and never a sweep — the roster carries only members a live collectible host measured as pinning, so a blanket dispose-everything row claims a guarantee the runtime does not give while a type proven not to pin (an undisposed `ActivitySource`, an instrument-free `Meter`) buys an unload nothing.

```csharp signature
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record RootBinding {
    private RootBinding() { }

    public sealed record Seated(string Seam, Func<IServiceCollection, RootInputs, Fin<IServiceCollection>> Apply) : RootBinding;

    public sealed record Proven(string Seam, Func<IServiceProvider, RootInputs, Fin<Unit>> Apply) : RootBinding;
}

public sealed record RootInputs(
    TelemetryComposition Telemetry,
    LaneGuard.Composition Lanes,
    Seq<LanePolicy> LaneRows,
    ChangefeedPort Changefeed,
    Func<CompanionPeer, IO<int>> ChildResidual,
    Func<DrainThread> DrainThread,
    // The resolved posture and the boot identity every seam stamps, plus the hook rail — the rail is a
    // composed VALUE rather than a resolved service because the capsule that fires its phase point is
    // constructed by a row in this same fold, and a row resolving what a later row registers reads a graph
    // that is still being written.
    ConsumptionProfile Profile,
    ClockPolicy Clocks,
    CorrelationId Correlation,
    DeploymentTopology Topology,
    // This node's own cluster identity — the `Services` config key the endpoint resolver keys on and the
    // membership group its durable rows live under. Both are deploy-declared values no service can produce,
    // which is why they seat here rather than resolving; a companion attaching over the control hop
    // contributes under THIS role, because it is a local process of this role and not a peer of its own.
    RoleName Role,
    string Group,
    CoordinationArrows Coordination,
    ControlSeed Control,
    HookRail Rail);

// The membership half of the Persistence coordination port, decoded to WIRE-STABLE PRIMITIVES at this root
// exactly as `LeaseElection.Runtime`'s four arrows are — group and member cross as their string keys and the
// scan answers keys beside deadlines, so no store record crosses upward into the cluster view.
public sealed record CoordinationArrows(
    Func<string, string, Duration, Fin<Unit>> MemberUpsert,
    Func<string, string, Fin<Unit>> MemberRelease,
    Func<string, Fin<Seq<(string Member, Instant Until)>>> MemberScan);

// The half of `ControlRuntime` this ledger does NOT own: the config page fills the invalidation, active-root,
// and revalidation arrows beside the reload section and class, the bundles page the support runtime, the
// telemetry composition its own minted source, and the ports page the merged wire options. The ledger row
// completes the record with the four columns the control hop itself adds.
public sealed record ControlSeed(
    DegradationCell Degradation,
    Func<Option<string>, Unit> InvalidateOptions,
    Func<IConfigurationRoot> ActiveConfig,
    string ReloadSection,
    ReloadClass ReloadClass,
    Func<string, Func<JsonObject, Validation<ConfigError, Unit>>> Revalidate,
    ActivitySource Source,
    SupportRuntime Support,
    JsonSerializerOptions Wire);

public static class CompositionRoot {
    // The must-bind roster. Every entry is one composed call, so the row names the seam and the delegate is
    // its whole body — a row growing logic of its own is the composition root absorbing a page's law.
    public static readonly Seq<RootBinding> Ledger =
    [
        new RootBinding.Seated("redaction-and-sampling", static (services, inputs) =>
            Fin.Succ(services.AddLogging(logging => ignore(SignalGovernance.GovernLogs(logging, inputs.Telemetry))))),

        new RootBinding.Seated("latency-context", static (services, _) =>
            Fin.Succ(services
                .AddLatencyContext(static options => options.ThrowOnUnregisteredNames = true)
                .Add(ServiceDescriptor.Describe(
                    typeof(Func<ILatencyContext>),
                    static provider => (Func<ILatencyContext>)provider.GetRequiredService<ILatencyContextProvider>().CreateContext,
                    ServiceLifetime.Singleton)))),

        new RootBinding.Seated("latency-names", static (services, inputs) =>
            Fin.Succ(LatencySpine.Register(services, [.. inputs.Telemetry.Latency]))),

        new RootBinding.Seated("lane-guard", static (services, inputs) =>
            LaneGuard.Register(services, inputs.Lanes, [.. inputs.LaneRows])),

        new RootBinding.Seated("changefeed", static (services, inputs) =>
            Fin.Succ(services.Add(ServiceDescriptor.Describe(
                typeof(ChangefeedPort), _ => inputs.Changefeed, ServiceLifetime.Singleton)))),

        new RootBinding.Seated("child-residual", static (services, inputs) =>
            Fin.Succ(services.Add(ServiceDescriptor.Describe(
                typeof(Func<CompanionPeer, IO<int>>), _ => inputs.ChildResidual, ServiceLifetime.Singleton)))),

        new RootBinding.Seated("drain-thread", static (services, inputs) =>
            Fin.Succ(services.Add(ServiceDescriptor.Describe(
                typeof(Func<DrainThread>), _ => inputs.DrainThread, ServiceLifetime.Singleton)))),

        // The phase fan-out is the rail's point, so the rail is LIVE before the capsule that fires it exists —
        // constructing the capsule first would leave the one shielded seam unsubscribable at its own boot phase.
        new RootBinding.Seated("lifecycle", static (services, inputs) =>
            Fin.Succ(services.Add(ServiceDescriptor.Describe(
                typeof(Lifecycle),
                _ => new Lifecycle(
                    inputs.Profile, inputs.Clocks.Clock, inputs.Clocks.Time, inputs.Correlation, inputs.Rail.Phase),
                ServiceLifetime.Singleton)))),

        // The no-provider arm of the features rail. `Evaluate` re-keys the same constant when a seated provider
        // reports itself unready, so absence and unreadiness answer ONE shape and a consumer never carries a
        // fallback verdict of its own — the deleted form is a per-consumer default that drifts from this one.
        // `TryAdd`, so a host that DOES seat a provider registers its own verdict function ahead of the table
        // and this row is the additive floor rather than the value that displaces it.
        new RootBinding.Seated("verdict-fallback", static (services, _) =>
            Fin.Succ((services.TryAdd(ServiceDescriptor.Describe(
                typeof(Func<EvaluationContext, FlagVerdict>),
                static _ => (Func<EvaluationContext, FlagVerdict>)(static _ => FlagVerdict.Inert),
                ServiceLifetime.Singleton)), services).Item2)),

        // The cluster view. Its `Attached` arrow resolves the roster and the roster's `contribute` resolves
        // this runtime — both reads sit INSIDE lambdas the fold never runs, so the mutual reference resolves
        // after both rows are registered and neither factory reads a graph still being written. `Remote` is
        // the admitted `UriHealthCheck` constructed per probed authority, because the check grades the URI
        // set fixed at ITS construction and a shared instance would accumulate every peer ever probed.
        new RootBinding.Seated("membership", static (services, inputs) =>
            Fin.Succ(services.Add(ServiceDescriptor.Describe(
                typeof(Membership.Runtime),
                provider => new Membership.Runtime(
                    NodeId: Environment.ProcessId,
                    Role: inputs.Role,
                    Group: inputs.Group,
                    Resolver: provider.GetRequiredService<ServiceEndpointResolver>(),
                    Health: provider.GetRequiredService<HealthCheckService>(),
                    Local: provider.GetRequiredService<WireHealthRow>(),
                    Remote: async (authority, token) =>
                        (await new UriHealthCheck(
                                new UriHealthCheckOptions().UseGet().AddUri(authority),
                                () => provider.GetRequiredService<IHttpClientFactory>().CreateClient(nameof(Membership)))
                            .CheckHealthAsync(new HealthCheckContext(), token)).Status,
                    Attached: pid => provider.GetRequiredService<PeerRoster>().Attached.Exists(entry => entry.Pid == pid),
                    MemberUpsert: inputs.Coordination.MemberUpsert,
                    MemberRelease: inputs.Coordination.MemberRelease,
                    MemberScan: inputs.Coordination.MemberScan,
                    View: Atom(new MembershipView(HashMap<int, MemberRecord>.Empty)),
                    Cursor: Atom(0UL),
                    Clocks: inputs.Clocks,
                    Staleness: LeasePolicy.Maintenance.CrashStaleness,
                    Sink: provider.GetRequiredService<ReceiptSinkPort>()),
                ServiceLifetime.Singleton)))),

        // The local kernel-credentialed attach set. `contribute` is the two-tier edge as a BOUND delegate:
        // every admitted peer enters the cluster view as a Joining row the sweep then grades, so the law that
        // was prose becomes a call site. The endpoint projects from the manifest's own `SocketPath` through
        // `UnixDomainSocketEndPoint` rather than a second column on `DiscoveryManifest` — the manifest is the
        // UDS contract under a 104-byte `sun_path` cap behind a checksum gate, so a parallel endpoint encoding
        // would put one address on the wire twice and oblige both copies to agree, where the BCL's own
        // projection of exactly that string obliges nothing. The pid is the KERNEL-reported one the credential
        // read produced, never the manifest's self-asserted field.
        new RootBinding.Seated("peer-roster", static (services, inputs) =>
            Fin.Succ(services.Add(ServiceDescriptor.Describe(
                typeof(PeerRoster),
                provider => PeerRoster.Boot(
                    service: inputs.Profile.Key,
                    contribute: (credential, manifest) => ignore(Membership.Contribute(
                        provider.GetRequiredService<Membership.Runtime>(),
                        credential.Pid,
                        inputs.Role,
                        new UnixDomainSocketEndPoint(manifest.SocketPath))),
                    sink: provider.GetRequiredService<ReceiptSinkPort>(),
                    clock: inputs.Clocks.Clock,
                    tenant: TenantContext.Root),
                ServiceLifetime.Singleton)))),

        // The control hop's dependency frame. `Classify` reads the descriptor's own
        // `PermissionShape.Classification` and falls to `Unknown` for a tool the registry cannot resolve,
        // whose erase treatment keeps the audit record fail-closed — an unresolvable tool must not leak the
        // argument text nobody graded. `Source` is the composition's minted source, never a process-static
        // one, so a continued verb span carries this composition's identity.
        // This row binds the four columns the CONTROL HOP adds and takes the reload, support, and identity
        // half as the composed `ControlSeed` its owning pages fill — the config page owns the invalidation
        // and revalidation arrows, the bundles page the support runtime, the telemetry composition its own
        // minted source. Composing here what those owners declare would re-implement three laws inside a
        // ledger row, which is exactly the shape this ledger's own Boundary rules out.
        new RootBinding.Seated("control-inbound", static (services, inputs) =>
            Fin.Succ(services.Add(ServiceDescriptor.Describe(
                typeof(ControlRuntime),
                provider => new ControlRuntime(
                    Degradation: inputs.Control.Degradation,
                    InvalidateOptions: inputs.Control.InvalidateOptions,
                    ActiveConfig: inputs.Control.ActiveConfig,
                    ReloadSection: inputs.Control.ReloadSection,
                    ReloadClass: inputs.Control.ReloadClass,
                    Revalidate: inputs.Control.Revalidate,
                    Dispatch: intent => CommandDispatch.Run(provider.GetRequiredService<DispatchRuntime>(), intent),
                    // Option, not Fin: `CapabilityRegistry.Resolve` answers `Option<CapabilityDescriptor>`, and
                    // a tool it cannot resolve falls to `Unknown`, whose erase treatment keeps the audit record
                    // fail-closed — an ungraded tool must not leak the argument text nobody classified.
                    Classify: tool => provider.GetRequiredService<CapabilityRegistry>().Resolve(tool)
                        .Match(Some: static row => row.Permission.Classification, None: static () => DataClassification.Unknown),
                    Redactors: provider.GetRequiredService<IRedactorProvider>(),
                    Source: inputs.Control.Source,
                    Support: inputs.Control.Support,
                    Clock: inputs.Clocks.Clock,
                    Sink: provider.GetRequiredService<ReceiptSinkPort>(),
                    Wire: inputs.Control.Wire),
                ServiceLifetime.Singleton)))),

        new RootBinding.Proven("lane-closure", static (provider, _) =>
            LaneGuard.Proven(provider.GetRequiredService<ResiliencePipelineProvider<string>>())),

        // Proven, not seated: the issuer exists only after the build, and the token's POSITION is the fact worth
        // proving — an unfolded roster answers a positionless token whose writes drop with nothing raised, while
        // the name echoes back whatever string was handed in and proves nothing. The capture is here because
        // `ThrowOnUnregisteredNames` makes the same omission a throw, so both shapes of the one defect leave on
        // this rail naming this seam, and the outbound runtime then carries the resolved token rather than a
        // name it re-resolves per hop.
        new RootBinding.Proven("hop-checkpoint", static (provider, _) =>
            Try.lift(() => provider.GetRequiredService<ILatencyContextTokenIssuer>()
                    .GetCheckpointToken(LatencyCheckpoint.Hop.Key))
                .Run()
                .Bind(static token => token.Position >= 0
                    ? Fin.Succ(unit)
                    : Fin.Fail<Unit>(new Fault.InvalidValue(
                        Label: LatencyCheckpoint.Hop.Key,
                        Requirement: "<a checkpoint name the folded latency roster carries>")))),

        // Unload is a MEMBERSHIP question, never a disposal sweep. Measured on a live collectible host: a
        // process-global registrant pins the load context only when it holds a delegate or an object typed
        // inside that context — an undisposed `PosixSignalRegistration` pins across repeated collection, an
        // observable instrument whose observe callback closes over context-typed state pins, while an
        // instrument-free `Meter` collects undisposed and an undisposed `ActivitySource` does not pin at all.
        // So the roster below is the pin set and this row proves each member is container-OWNED, which is what
        // makes provider disposal the release; a blanket dispose-everything claim would report a guarantee the
        // runtime does not give, and a count would not name the member that failed to register.
        new RootBinding.Proven("capsule-unload", static (provider, inputs) =>
            inputs.Topology != DeploymentTopology.InHost
                ? Fin.Succ(unit)
                : CapsulePins
                    .Traverse(pin => provider.GetService(pin) is IDisposable or IAsyncDisposable
                        ? Validation<Error, Unit>.Success(unit)
                        : new Fault.InvalidValue(Label: pin.Name, Requirement: "<a container-owned disposable>"))
                    .As()
                    .Map(static _ => unit)
                    .ToFin()),
    ];

    // The measured pin set: the container-owned members that hold a context-typed delegate or object and so
    // keep a collectible load context alive until released — the utilization listener's observe callbacks and
    // the telemetry composition's transports and queue set. A type joins on a live-host measurement, never on
    // suspicion, so members proven not to pin stay off and an unload waits on no release that buys nothing. The
    // signal-trap `PhaseSubscription` is the third pin and is NOT here because its owner holds it directly: the
    // capsule disposes the composite `ArmTraps` returned, which is what releases the `PosixSignalRegistration`
    // this roster's probe would otherwise have to reach through a service the container never registered.
    public static readonly Seq<Type> CapsulePins = [typeof(UtilizationCell), typeof(TelemetryComposition)];

    public static Fin<(IServiceProvider Provider, Seq<ContributionReceipt> Receipts)> Arm(
        ServiceCollection services, RootInputs inputs, ServiceProviderOptions options,
        params ReadOnlySpan<ModuleContribution> modules) =>
        Seated(services, inputs)
            .Bind(seated => seated.Compose(modules))
            .Map(receipts => (Provider: (IServiceProvider)services.BuildServiceProvider(options), Receipts: receipts))
            .Bind(built => Built(built.Provider, inputs).Map(_ => built));

    // One ledger, two folds, and a row cannot run at the altitude its case forbids: the opposite arm is a
    // no-op rather than a filter, so the roster stays one ordered declaration and neither fold re-derives it.
    static Fin<ServiceCollection> Seated(ServiceCollection services, RootInputs inputs) =>
        Ledger.TraverseM(row => row.Switch(
                state: (Services: (IServiceCollection)services, Inputs: inputs),
                seated: static (state, row) => row.Apply(state.Services, state.Inputs).Map(static _ => unit),
                proven: static (_, _) => Fin.Succ(unit)))
            .As()
            .Map(_ => services);

    static Fin<Unit> Built(IServiceProvider provider, RootInputs inputs) =>
        Ledger.TraverseM(row => row.Switch(
                state: (Provider: provider, Inputs: inputs),
                seated: static (_, _) => Fin.Succ(unit),
                proven: static (state, row) => row.Apply(state.Provider, state.Inputs)))
            .As()
            .Map(static _ => unit);
}
```

## [07]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
