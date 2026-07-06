# [APPHOST_COMPOSITION_AND_MODULES]

One composition root per process folds a frozen module table into the service graph and freezes it. Composition owns three axes: the `ModuleContribution` row — assembly, scan, descriptor, contributor, registrar, and decoration columns — the one-pass receipted composition fold, and admission-boundary activation carrying availability probing, async-scope ownership, keyed decoration introspection, and validator discovery. One descriptor algebra serves every seam: `ServiceDescriptor.Describe` and `DescribeKeyed` rows carry registrations, and `TryAddEnumerable` ordered sets carry every fan-in family. The package spine is `Microsoft.Extensions.DependencyInjection` with `Scrutor` scanning and decoration, `FluentValidation.DependencyInjectionExtensions` validator discovery at the root, and `System.CommandLine` as the app-root verb boundary — one `ParseResult`-driven projection onto the existing owners, never a second dispatcher.

## [01]-[INDEX]

- [01]-[MODULE_TABLE]: Frozen contribution rows with one descriptor algebra for every fan-in seam.
- [02]-[SCAN_AND_DECORATE]: One-pass scan, decoration, and keyed registration fold with receipted freeze.
- [03]-[BOUNDARY_ACTIVATION]: Activation plans, availability probes, async scopes, keyed decoration, and validators.
- [04]-[APP_ROOT_VERBS]: The `System.CommandLine` verb table — seed DATA projecting `ParseResult` onto existing owners.

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

- Owner: `CompositionSurface` — one fold composes scan, descriptor admission, decoration, and freeze in one pass over the table.
- Entry: `Fin<Seq<ContributionReceipt>> Compose(params ReadOnlySpan<ModuleContribution> modules)` — `Fin` aborts on the first rejected module with module provenance in the failure.
- Auto: `MakeReadOnly` freezes the collection after the fold; `BuildServiceProvider` under `ServiceProviderOptions` with `ValidateOnBuild` and `ValidateScopes` proves the frozen graph on the test row.
- Packages: Scrutor, Microsoft.Extensions.DependencyInjection
- Growth: one scan filter row or one registrar row per cross-cutting concern; zero new surface — the fold absorbs it.
- Boundary: `Applied` is the composition-root boundary capsule — `Scrutor` scan, descriptor admission, and registrar application are host-owned statement seams, and the statement carve-out names this fence.

Pass law:
- Scan sources are `FromAssemblies` over the row's explicit `Assembly`. `FromApplicationDependencies` and `FromDependencyContext` walk the default dependency closure and are the deleted sources: plugin load contexts never appear in that closure, so closure-walking scans silently miss every plugin assembly.
- Selection composes `AddClasses`, then `AssignableTo`, `WithAttribute`, and `InNamespaces` filters, then mapping: `UsingAttributes` maps `ServiceDescriptorAttribute`-annotated classes, `AsImplementedInterfaces` and `AsSelfWithInterfaces` map the rest, and `WithLifetime` plus `WithServiceKey` bind lifetime and key inside the same pass.
- Duplicate registrations resolve under `UsingRegistrationStrategy(RegistrationStrategy.Throw)` bound inside the same `Scan` pass; the thrown rejection captures into the rail as conflict evidence carrying the module key — never a silent append, never a silent replace. `RegistrationStrategy.Replace` survives only as an explicit row-level policy on a row that names the contract it overrides.
- The `Decorations` column applies before registrars through `BoundaryActivation.Decorate`, wrapping contributor ports with telemetry and receipt decoration; the decorated contract stays the public contract, and the `Conditional` flag selects `TryDecorate` on a profile-conditional target. Decoration owns this cluster's keyed-decoration pass-law; `BOUNDARY_ACTIVATION` owns the decoration introspection and the `ContributionReceipt.Decorated` count.
- Registration is bootstrap-only: after `MakeReadOnly`, descriptor mutation throws, so every late registration attempt surfaces at the root instead of drifting into runtime state.

```csharp signature
public static class CompositionSurface {
    extension(ServiceCollection services) {
        public Fin<Seq<ContributionReceipt>> Compose(params ReadOnlySpan<ModuleContribution> modules) =>
            Iterable<ModuleContribution>.FromSpan(modules)
                .TraverseM(module => Try.lift(() => Applied(services, module)).Run()
                    .MapFail(error => Error.New($"<module-rejected:{module.Module}:{error.Message}>")))
                .As()
                .Map(receipts => (fun(services.MakeReadOnly)(), receipts.ToSeq()).Item2);
    }

    private static ContributionReceipt Applied(IServiceCollection services, ModuleContribution module) {
        int admitted = services.Count;
        module.Scan.IfSome(select => services.Scan(source => select(source.FromAssemblies(module.Assembly))));
        int scanned = services.Count - admitted;
        module.Services.Iter(services.Add);
        module.Keyed.Iter(services.Add);
        module.Defaults.Iter(row => Default(services, row));
        module.Contributors.Iter(services.TryAddEnumerable);
        module.Decorations.Iter(decoration => BoundaryActivation.Decorate(services, decoration));
        ignore(module.Registrars.Fold(services, static (current, registrar) => registrar(current)));
        return new ContributionReceipt(
            Module: module.Module,
            Scanned: scanned,
            Singletons: Lifetimes(module, ServiceLifetime.Singleton),
            Scoped: Lifetimes(module, ServiceLifetime.Scoped),
            Transients: Lifetimes(module, ServiceLifetime.Transient),
            Keyed: module.Keyed.Count,
            Defaults: module.Defaults.Count,
            Contributors: module.Contributors.Count,
            Registrars: module.Registrars.Count,
            Decorated: module.Decorations.Count(decoration => services.IsDecorated(decoration.Service)));
    }

    private static void Default(IServiceCollection services, ServiceDescriptor row) =>
        ignore(row.IsKeyedService
            ? row.Lifetime switch {
                ServiceLifetime.Singleton => (services.TryAddKeyedSingleton(row.ServiceType, row.ServiceKey, row.KeyedImplementationType!), services).Item2,
                ServiceLifetime.Scoped => (services.TryAddKeyedScoped(row.ServiceType, row.ServiceKey, row.KeyedImplementationType!), services).Item2,
                _ => (services.TryAddKeyedTransient(row.ServiceType, row.ServiceKey, row.KeyedImplementationType!), services).Item2,
            }
            : (fun(() => services.TryAdd(row))(), services).Item2);

    private static int Lifetimes(ModuleContribution module, ServiceLifetime lifetime) =>
        (module.Services + module.Keyed + module.Defaults).Filter(row => row.Lifetime == lifetime).Count;
}
```

The fold is the only writer of the collection: scan first inside each module so the scanned count derives from the collection delta, descriptor rows next, decorations after the descriptors so every `DecorationRow` finds its target contract within the module or in an earlier row, registrars last.

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
- Boundary: the verb table is a BOUNDARY ADAPTER — every row's body is one projection into a composed owner (`CommandDispatch.Run`, the determinism port, the capture trigger) and a verb carrying domain logic of its own is the deleted form; `AppRootVerbs.Mount` is the named boundary capsule for the statement carve-out (the `RootCommand` mutation seam); parse failures surface as `ParseResult.Errors` projected to exit-code evidence, never a thrown parse; the ControlService verbs stay the service-modality wire route — the CLI row and the control verb project onto the SAME owner so an operator at a terminal and an operator over the control hop invoke one semantics; removal of this table is legal only on proof every verb rides ControlService for every host modality.

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

    public static VerbRow CaptureSupport(CorrelationId correlation, Func<SupportTrigger, IO<Unit>> capture) {
        var reason = new Option<string>("--reason");
        var command = new Command("capture-support", "admit one external-command support capture") { reason };
        return new(command, (parse, token) =>
            capture(new SupportTrigger.ExternalCommand(correlation, parse.GetValue(reason) ?? string.Empty))
                .RunAsync().AsTask().ContinueWith(static run => run.IsCompletedSuccessfully ? 0 : 1, token));
    }
}
```
