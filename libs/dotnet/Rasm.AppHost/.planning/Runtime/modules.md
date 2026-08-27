# [APPHOST_COMPOSITION_AND_MODULES]

One composition root per process folds a frozen module table into the service graph, arms every contract the corpus declares must bind, and freezes it. Composition owns four axes: the `ModuleContribution` row — assembly, scan, slot-keyed descriptor carrier, registrar, and decoration columns — the one-pass composition fold whose single ordinal pass over `DescriptorSlot.Items` carries every admission and apply, admission-boundary activation carrying availability probing, async-scope ownership, keyed decoration introspection, and validator discovery, and the MUST-BIND ledger whose module folds and root bindings are every declared owner's one call site. One descriptor algebra serves every contribution: `DescriptorSlot` rows name the admitting member and the admission their descriptors cross first. The package spine is `Microsoft.Extensions.DependencyInjection` with `Scrutor` scanning and decoration, `FluentValidation.DependencyInjectionExtensions` validator discovery at the root, and `System.CommandLine` as the app-root verb boundary.

## [01]-[INDEX]

- [02]-[MODULE_TABLE]: Frozen contribution rows over one slot roster carrying every descriptor admission.
- [03]-[SCAN_AND_DECORATE]: One-pass scan, slot fold, decoration, and freeze.
- [04]-[BOUNDARY_ACTIVATION]: Activation plans, availability probes, async scopes, keyed decoration, and validators.
- [05]-[COMMAND_SURFACE]: `System.CommandLine` verb table — seed DATA projecting `ParseResult` onto existing owners.
- [06]-[MODULE_LEDGER]: Module folds, the must-bind contract roster, and the two-altitude fold that is every row's one call site.

## [02]-[MODULE_TABLE]

- Owner: `DescriptorSlot` `[SmartEnum<string>]` — the descriptor algebra as ROW DATA, each row carrying the admission its descriptors cross and the collection member that admits them; `ModuleContribution` — the frozen per-process module-table row; modules contribute registrations and never resolve services.
- Cases: `Service` admits unkeyed descriptors and `Keyed` keyed ones — each row enforces its own regime against the descriptor's `IsKeyedService` — `Default` adds idempotently across both regimes, and `Contributor` joins the ordered fan-in set behind the port-cardinality admission.
- Auto: the composition fold walks `DescriptorSlot.Items` in `Rank` order, so slot ordering is the roster's own column and no fold body names a slot.
- Packages: Microsoft.Extensions.DependencyInjection, Thinktecture.Runtime.Extensions
- Growth: a new admission regime is ONE `DescriptorSlot` row carrying its `Admits` and `Admit` columns; one module row lands per contributing package and one descriptor row per service.
- Boundary: descriptor construction spells `ServiceDescriptor.Describe` and `DescribeKeyed` only; the ordinal slot fold applies every admission regime without stored counts or a parallel summary value.

Row law:
- One composition root per process folds the table; packages ship rows into it. A per-package registration extension, a module interface with configure members, and an event-style registration hook are the deleted patterns — the row is the whole module contract.
- Table order is semantic: a registrar that wraps a sibling module's contract sits in a later row than the contract it wraps, and the fold preserves declaration order end to end.
- `Descriptors` is the ONE slot-keyed carrier: `Service` holds unkeyed `Describe` rows, `Keyed` holds `DescribeKeyed` rows whose keys are smart-enum policy values from the owning vocabulary pages, `Contributor` holds the ordered fan-in sets — health, support, drain, and telemetry contributor families register there, never through a bespoke aggregator contract. A slot a module never fills reads empty rather than absent, so the fold is total over the roster and a module omits a slot by saying nothing.
- `Default` is the additive-only floor: a package-shipped default whose contract a host or later module may pre-empt applies through the package's own `TryAdd`, which compares `(ServiceType, ServiceKey)` and reads no implementation type, so the keyed and unkeyed arms are ONE member and the deleted lifetime switch was both redundant and narrower — it dereferenced `KeyedImplementationType`, which is null on every keyed factory and keyed instance descriptor this law admits. A default that must override an earlier registration is a `Service` row.
- `FromKeyedServicesAttribute` binds keyed constructor parameters, `ServiceKeyAttribute` injects the resolved key into the implementation, and `KeyedService.AnyKey` selects keyed enumerables and never resolves a single service.
- `Registrars` carries collection-shaped package registrations that no descriptor spelling expresses — the validator-discovery row and other collection-shaped admissions — each a `Func<IServiceCollection, IServiceCollection>` applied after the module's descriptor rows.
- `Decorations` carries the typed decoration column: each entry is one `DecorationRow` application naming the inner service contract and the wrapping decorator, so the decoration topology is data the fold reads, never an opaque registrar `Func`. A profile that drops a contributor port carries the entry with `Conditional: true`, so the same column decorates on the service profile and skips on the plugin profile by `TryDecorate` row presence.
- The `Scan` column is `Option`-typed: a row constructed with `Scan: default` composes through explicit descriptor rows alone. The web and AOT module tables construct every row that way — the same table, zero parallel composition system, and the column flip is the growth proof.

```csharp
// --- [TABLES] --------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class DescriptorSlot {
    public static readonly DescriptorSlot Service = new("service", rank: 0,
        admits: static row => Slotted(row, keyed: false), admit: static services => services.Add);
    public static readonly DescriptorSlot Keyed = new("keyed", rank: 1,
        admits: static row => Slotted(row, keyed: true), admit: static services => services.Add);
    public static readonly DescriptorSlot Default = new("default", rank: 2,
        admits: static _ => Validation<Error, Unit>.Success(unit), admit: static services => services.TryAdd);
    public static readonly DescriptorSlot Contributor = new("contributor", rank: 3,
        admits: static row => PortCardinality.Of(row.ServiceType.Name).ToValidation().Map(static _ => unit),
        admit: static services => services.TryAddEnumerable);

    public int Rank { get; }

    [UseDelegateFromConstructor]
    public partial Validation<Error, Unit> Admits(ServiceDescriptor row);

    [UseDelegateFromConstructor]
    public partial Action<ServiceDescriptor> Admit(IServiceCollection services);

    static Validation<Error, Unit> Slotted(ServiceDescriptor row, bool keyed) =>
        row.IsKeyedService == keyed
            ? Validation<Error, Unit>.Success(unit)
            : new KernelFault.InvalidValue(
                Label: row.ServiceType.Name,
                Requirement: keyed ? "<a keyed descriptor>" : "<an unkeyed descriptor>");
}

// --- [MODELS] --------------------------------------------------------------------------
public sealed record ModuleContribution(
    string Module,
    Assembly Assembly,
    Option<Action<IImplementationTypeSelector>> Scan,
    HashMap<DescriptorSlot, Seq<ServiceDescriptor>> Descriptors,
    Seq<Func<IServiceCollection, IServiceCollection>> Registrars,
    Seq<DecorationRow> Decorations) {
    public Seq<ServiceDescriptor> this[DescriptorSlot slot] => Descriptors.Find(slot).IfNone(Seq<ServiceDescriptor>());

    public Seq<ServiceDescriptor> Rows => toSeq(DescriptorSlot.Items).Bind(slot => this[slot]);
}

public readonly record struct DecorationRow(Type Service, Type Decorator, bool Conditional);

```

Module keys are `nameof`-derived assembly symbols, never free literals.

## [03]-[SCAN_AND_DECORATE]

- Owner: `CompositionSurface` — one fold composes scan, the ordinal slot pass carrying every descriptor admission, decoration, and freeze in one pass over the table.
- Entry: `Fin<Unit> Compose(params ReadOnlySpan<ModuleContribution> modules)` — `Fin` aborts on the first rejected module with module provenance in the failure, whether the rejection was thrown by the scan or returned by an admission.
- Auto: `MakeReadOnly` freezes the collection after the fold; `BuildServiceProvider` under `ServiceProviderOptions` with `ValidateOnBuild` and `ValidateScopes` proves the frozen graph on the test row.
- Packages: Scrutor, Microsoft.Extensions.DependencyInjection
- Growth: one scan filter row or one registrar row per cross-cutting concern; zero new surface — the fold absorbs it.
- Boundary: `Applied` is the composition-root boundary capsule — `Scrutor` scan, descriptor admission, and registrar application are host-owned statement boundaries, and the statement carve-out names this fence; the `Runtime/ports#PORT_RECORDS` eighth-port refusal EXECUTES through the `Contributor` row's `Admits` column and nowhere else, so a contributor descriptor naming no cardinality row refuses while the collection is still editable rather than surfacing later as a leaked inward dependency.

Pass law:
- Scan sources are `FromAssemblies` over the row's explicit `Assembly`. `FromApplicationDependencies` and `FromDependencyContext` walk the default dependency closure and are the deleted sources: plugin load contexts never appear in that closure, so closure-walking scans silently miss every plugin assembly.
- Selection composes `AddClasses`, then `AssignableTo`, `WithAttribute`, and `InNamespaces` filters, then mapping: `UsingAttributes` maps `ServiceDescriptorAttribute`-annotated classes, `AsImplementedInterfaces` and `AsSelfWithInterfaces` map the rest, and `WithLifetime` and `WithServiceKey` bind lifetime and key inside the same pass.
- Duplicate registrations resolve under `UsingRegistrationStrategy(RegistrationStrategy.Throw)` bound inside the same `Scan` pass; the thrown rejection captures onto `Fin` as conflict evidence carrying the module key — never a silent append, never a silent replace. `RegistrationStrategy.Replace` survives only as an explicit row-level policy on a row that names the contract it overrides.
- Descriptors apply through ONE ordinal pass over `DescriptorSlot.Items`: each slot accumulates its own `Admits` over its rows and then hands them to the member its `Admit` column names, so the whole descriptor stage is one expression whatever the roster holds. The pass ACCUMULATES within a slot and across the roster alike, so a module adding three foreign contributor ports beside a mis-slotted keyed row names all four on one boot rather than one per attempt.
- The `Decorations` column applies before registrars through `BoundaryActivation.Decorate`; `Conditional` selects `TryDecorate`, while a required target that remains undecorated refuses on `Fin`.
- Registration is bootstrap-only: after `MakeReadOnly`, descriptor mutation throws, so every late registration attempt surfaces at the root instead of drifting into runtime state.

```csharp
public static class CompositionSurface {
    extension(ServiceCollection services) {
        public Fin<Unit> Compose(params ReadOnlySpan<ModuleContribution> modules) =>
            Iterable<ModuleContribution>.FromSpan(modules)
                .TraverseM(module => Try.lift(() => Applied(services, module)).Run().Bind(static inner => inner)
                    .MapFail(error => (Error)new LifecycleFault.ModuleRejected(module.Module, error)))
                .As()
                .Map(_ => (fun(services.MakeReadOnly)(), unit).Item2);
    }

    private static Fin<Unit> Applied(IServiceCollection services, ModuleContribution module) {
        module.Scan.IfSome(select => services.Scan(source => select(source.FromAssemblies(module.Assembly))));
        return toSeq(DescriptorSlot.Items.OrderBy(static slot => slot.Rank))
            .Traverse(slot => Seated(services, slot, module[slot])
                .ToValidation())
            .As()
            .ToFin()
            .Bind(landed => {
                module.Decorations.Iter(decoration => BoundaryActivation.Decorate(services, decoration));
                ignore(module.Registrars.Fold(services, static (current, registrar) => registrar(current)));
                return Decorated(services, module.Decorations);
            });
    }

    private static Fin<int> Seated(IServiceCollection services, DescriptorSlot slot, Seq<ServiceDescriptor> rows) =>
        rows.Traverse(slot.Admits)
            .As()
            .ToFin()
            .Map(_ => Landed(services, rows, slot.Admit(services)));

    private static int Landed(IServiceCollection services, Seq<ServiceDescriptor> rows, Action<ServiceDescriptor> admit) =>
        services.Count is var before && rows.Iter(admit) is var _
            ? services.Count - before
            : 0;

    private static Fin<Unit> Decorated(IServiceCollection services, Seq<DecorationRow> rows) =>
        rows.Traverse(row => row.Conditional || services.IsDecorated(row.Service)
                ? Validation<Error, Unit>.Success(unit)
                : new KernelFault.InvalidValue(Label: row.Service.Name, Requirement: "<a decorated contract>"))
            .As()
            .Map(static _ => unit)
            .ToFin();
}
```

The fold is the only writer of the collection: scan first inside each module so the scanned count derives from the collection delta, the slot pass next in `Rank` order so each slot's admission runs before the rows it guards enter, decorations after the descriptors so every `DecorationRow` finds its target contract within the module or in an earlier row, registrars last.

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
- Every activation failure converts at this boundary: the capture funnel projects construction rejections onto `Fin` with the target type name, and no raw activation exception crosses inward.
- `AddValidatorsFromAssemblies` discovers validators with an explicit `ServiceLifetime` and a deterministic `AssemblyScanner.AssemblyScanResult` filter; `includeInternalTypes` stays `false`, so public validators are the admitted set. The produced delegate enters the module table as one `Registrars` row — validator discovery owns no second registration path.

Decoration pass-law:
- `Decorate` applies one `DecorationRow` column entry over the collection: a `Conditional: false` entry spells `Decorate(serviceType, decoratorType)` on a contract guaranteed present, and a `Conditional: true` entry spells `TryDecorate(serviceType, decoratorType)` so a profile where the inner port is absent decorates nothing rather than failing. The same module table decorates a contributor port on the service profile and skips it on the plugin profile by entry presence and the `Conditional` flag, never by a runtime branch at a call site.
- The decorated contract stays the public contract: a decorated port resolves to the decorator, and the decorator resolves the inner registration through the generated `DecoratedService<TService>` handle, so a third decoration wraps the second with no registration rewrite. Keyed contributor ports decorate by their smart-enum service key, so decoration composes per key without a parallel keyed-decoration path.
- `Decorated` confirms every required target through `IsDecorated(serviceType)`; `Decorated<TService>` over `GetDecoratedServices<TService>` remains the graph-introspection read.

```csharp
public static class BoundaryActivation {
    private static readonly ConcurrentDictionary<Type, ObjectFactory> Plans = new();

    extension(IServiceProvider provider) {
        public Fin<T> Activate<T>(params object[] dependencies) where T : notnull =>
            Try.lift(() => Fin.Succ(dependencies.Length == 0
                ? ActivatorUtilities.GetServiceOrCreateInstance<T>(provider)
                : (T)Plans.GetOrAdd(
                        typeof(T),
                        static (_, supplied) => ActivatorUtilities.CreateFactory(
                            typeof(T),
                            [.. supplied.Select(static value => value.GetType())]),
                        dependencies)
                    .Invoke(provider, dependencies)!)).Run().Bind(static inner => inner)
                .MapFail(error => (Error)new LifecycleFault.ActivationRejected(typeof(T).Name, error));

        public bool Available<T>(Option<object> key = default) where T : notnull =>
            key is { IsSome: true, Case: object serviceKey }
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

## [05]-[COMMAND_SURFACE]

- Owner: `VerbRow` the seed-DATA verb table row; `AppRootVerbs` the one CLI boundary adapter mounting the table onto a `RootCommand`.
- Cases: canonical rows — `dispatch` projects a descriptor + serialized arguments onto `Agent/runtime#DISPATCH_FRONT_DOOR` `CommandDispatch.Run`; `replay` and `bisect` are the `Runtime/determinism` ingress (the `ChangefeedPort.Load` windowed read feeding `ReplayVerify.Replay`/`AdversarialProbe.Bisect`); `capture-support` admits one `SupportTrigger.ExternalCommand` onto the `Observability/bundles` capture fan; `sandbox-release` projects onto `Sandbox/isolation#QUOTA_CONTROL` `QuotaControl.Release`, the one path a quarantined plugin takes back into service.
- Entry: `Mount(string description, Seq<VerbRow> rows)` returns `RootCommand` — the table mounts once at the app root; each row's `Command.SetAction(Func<ParseResult, CancellationToken, Task<int>>)` binds the projection; `ParseResult.GetValue<T>(Option<T>)`/`GetValue<T>(Argument<T>)` are the only argument reads.
- Packages: System.CommandLine, LanguageExt.Core, BCL inbox
- Growth: a new operator verb is one `VerbRow` in the table projecting onto an existing owner; a verb whose owner does not exist yet is a missing case on the owning page, never a CLI-local body; zero new surface.
- Boundary: the verb table is a BOUNDARY ADAPTER — every row's body is one projection into a composed owner (`CommandDispatch.Run`, the determinism port, the capture trigger) and a verb carrying domain logic of its own is the deleted form; `AppRootVerbs.Mount` is the named boundary capsule for the statement carve-out (the `RootCommand` mutation boundary); a rejected parse never reaches a row's action because a non-empty `ParseResult.Errors` blocks invocation by the package's own contract, so parse failure is DATA the host entry projects to an exit code and a thrown parse has no spelling here; `Exit` is the ONE exit projection every row leaves through and the status it answers is BINARY — a POSIX wait status keeps the low eight bits of what a process returns, so a banded fault code CANNOT be one: `FaultBand.Config.Code(offset: 0)` is 4100, `4100 & 0xFF` is 4 — a number naming no verdict — and any code ≡ 0 mod 256 reports SUCCESS outright to every shell, supervisor, and CI gate that reads it, which is why `(int)error.Code` as a status is the deleted form — the STATUS carries the verdict (0 admitted, 1 refused); the STREAM renders a typed fault's band code and message, an uncoded foreign error's message, or the located refusal columns; these remain local CLI projections and do not imply a ControlService RPC.

```csharp
public sealed record VerbRow(Command Command, Func<ParseResult, CancellationToken, Task<int>> Project);

public static class AppRootVerbs {
    public static RootCommand Mount(string description, Seq<VerbRow> rows) {
        var root = new RootCommand(description);
        rows.Iter(row => { row.Command.SetAction(row.Project); root.Add(row.Command); });
        return root;
    }

    static int Exit(Fin<Option<string>> located) =>
        located.Match(
            Succ: static found => found.Match(
                Some: static text => (fun(() => Console.Out.WriteLine(text))(), 1).Item2,
                None: static () => 0),
            Fail: static error => (
                fun(() => Console.Error.WriteLine(
                    error is Fault fault
                        ? string.Create(CultureInfo.InvariantCulture, $"{fault.Code}\t{fault.Message}")
                        : error.Message))(),
                1).Item2);

    public static VerbRow Dispatch(DispatchRuntime runtime, Func<string, string, Fin<CommandIntent>> intentOf) {
        var descriptor = new Argument<string>("descriptor");
        var arguments = new Option<string>("--arguments", "-a");
        var command = new Command("dispatch", "run one capability through the command front door") { descriptor, arguments };
        return new(command, (parse, token) =>
            intentOf(parse.GetValue(descriptor)!, parse.GetValue(arguments) ?? "{}")
                .Match(
                    Succ: intent => CommandDispatch.Run(runtime, intent).RunAsync().AsTask()
                        .ContinueWith(static run => run.IsCompletedSuccessfully && run.Result.Txn is CommandTxn.Committed ? 0 : 1, token),
                    Fail: error => Task.FromResult(Exit(Fin.Fail<Option<string>>(error)))));
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
                    Fail: error => Task.FromResult(Exit(Fin.Fail<Option<string>>(error)))));
    }

    public static VerbRow Bisect(ChangefeedPort port, Func<LogEntry, ChainHash> rederive) {
        var origin = new Option<Guid>("--origin");
        var from = new Option<long>("--from");
        var to = new Option<long>("--to");
        var command = new Command("bisect", "binary-search a recorded chain for its first divergent step") { origin, from, to };
        return new(command, (parse, _) => Task.FromResult(Exit(
            port.Load(new ChangefeedWindow(parse.GetValue(origin), parse.GetValue(from), parse.GetValue(to)))
                .Map(log => AdversarialProbe.Bisect(log, rederive).Map(Located)))));
    }

    static string Located(Divergence divergence) =>
        string.Create(CultureInfo.InvariantCulture,
            $"{divergence.Sequence}\t{divergence.Recorded.Hex}\t{divergence.Rederived.Hex}\t{divergence.Steps}");

    public static VerbRow SandboxRelease(Atom<Seq<HostedSolver>> hosted, ClockPolicy clocks) {
        var plugin = new Argument<string>("plugin");
        var command = new Command("sandbox-release", "return one quarantined plugin to service") { plugin };
        return new(command, (parse, _) => Task.FromResult(Exit(
            hosted.Value
                .Find(row => row.Instance.PluginId == parse.GetValue(plugin))
                .Map(static row => row.Instance)
                .ToFin(new SandboxFault.Quarantined($"{parse.GetValue(plugin)}: <no hosted plugin>"))
                .Bind(instance => QuotaControl.Release(instance, clocks.Now))
                .Map(static _ => Option<string>.None))));
    }

    public static VerbRow CaptureSupport(CorrelationId correlation, Func<SupportTrigger, IO<Unit>> capture) {
        var reason = new Option<string>("--reason");
        var command = new Command("capture-support", "admit one external-command support capture") { reason };
        return new(command, (parse, token) =>
            capture(new SupportTrigger.Requested(correlation, SupportTriggerKind.ExternalCommand, parse.GetValue(reason) ?? string.Empty))
                .RunAsync().AsTask().ContinueWith(static run => run.IsCompletedSuccessfully ? 0 : 1, token));
    }
}
```

## [06]-[MODULE_LEDGER]

- Owner: `RootBinding` `[Union]` the two-altitude binding row; `RootInputs` the composed values every row reads, with `CoordinationArrows`, `ControlSeed`, `ObservabilitySeed`, `WireSeed`, `AgentSeed`, and `SandboxSeed` the deploy-declared seeds beside it; `CompositionRoot` the static ledger, the measured `CapsulePins` load-context roster, the `Metered` pre-`Arm` instrument mount carrying the declaring-package contributor roster, and the one fold that is each row's call site.
- Cases: `Seated` binds while the collection is still editable; `Proven` runs against the BUILT provider because the fact it needs — a materialized pipeline, a loaded plugin, an issued token — does not exist before the build.
- Entry: `Arm(ServiceCollection services, RootInputs inputs, ServiceProviderOptions options, params ReadOnlySpan<ModuleContribution> modules)` returns `Fin<IServiceProvider>` after seating, composing, freezing, building, and proving every ledger row.
- Auto: the ledger is DATA, so a page declaring a new must-bind contract adds one row and no fold body changes; declaration order IS dependency order — `hooks-mount` runs first because every later fold reads the dispatcher it seats, and a fold needing a value an earlier fold produced resolves it inside a factory lambda the fold never runs, so no row reads a graph still being written and no dependency graph is computed to recover an order the roster already states; the root's own bindings seat AHEAD of the module table so a module row decorating a platform contract finds it already registered, and `Compose`'s `MakeReadOnly` stays the one freeze — a ledger row after it throws at the append rather than registers; both folds accumulate per row onto `Fin`, so one boot names every unbound contract instead of one per attempt.
- Packages: Microsoft.Extensions.DependencyInjection, Polly.Extensions, NuGet.Versioning, LanguageExt.Core, BCL inbox
- Growth: a new must-bind contract is one `RootBinding` row at its altitude; a new module is one `<module>-seat`/`<module>-boot` pair; a new composed value the rows read is one `RootInputs` column, and a value whose halves belong to other owners is one seed record beside it, so an owner's law reaches this fold as a filled column rather than a re-implementation; a package that declares an instrument family is one `Metered` contributor argument; a member measured to pin a collectible load context is one `CapsulePins` row the unload proof already folds; zero new surface.
- Boundary: this ledger is the ONE composition-scoped call site for every contract a page declares and no ordinary consumer reaches, so a declared-and-unbound contract is a missing row rather than a sentence a reader audits against the corpus — the class the ledger exists to close is a `Bind`, `Register`, `Mount`, or `Of` member with a page-long law behind it and zero callers. Runtime-scoped PRODUCERS never appear here and the two altitudes never trade: an instrument write, a dispatched tool, a continued trace binds at its own producing arm, because hoisting one into this fold fires it once at boot where the law wants it per event, and sinking a composition binding into a producing arm re-registers on a frozen collection; the `DescriptorSlot.Contributor` admission stays inside `SCAN_AND_DECORATE`'s per-module fold for the same reason — the module row is the thing that can violate the port invariant, so the admission belongs at the row and not at the root; a row's delegate composes an owner and never re-implements one, so the ledger holds no logic of its own and a body doing work past its composed calls is the deleted form; the `ILatencyContext` factory is the composition's single mint — `DrainConductor.Drain`, `OutboundSurface.Run`, and `SupportCapture.Capture` each take the context as a parameter, so a fold minting its own context, or timing a phase off a `Stopwatch`, is the deleted form; `LatencySpine.Register` owns the NAME table alone while this ledger owns the provider registration, so the option that gates the issuer sets once at this seat; capsule unload is a MEMBERSHIP proof and never a sweep — the roster carries only members a live collectible host measured as pinning, so a blanket dispose-everything row claims a guarantee the runtime does not give while a type proven not to pin (an undisposed `ActivitySource`, an instrument-free `Meter`) buys an unload nothing.

Seat law:
- `hooks-mount` freezes the point census and seats the precomposed dispatcher; `HookSet.Of` has already admitted every gate and tap before the service graph freezes.
- `redaction-and-sampling` reaches the log chain through `SignalGovernance.GovernLogs`, whose `RedactionRegistration.Bind` carries every redactor row — sealing the chain without it leaves the erasing fallback as the ONLY resolution and every classified tag erases, including the operational dimensions the pass rows exist to spare.
- `latency-context` seats the pooled provider, issuer, and the ONE `Func<ILatencyContext>` factory the three threading folds read; `latency-names` folds this root's roster with every contributed `LatencyRoster` into the single registration under `ThrowOnUnregisteredNames`, because an unregistered name resolves to a positionless token whose writes drop with nothing raised.
- `drain-thread` seats the `Func<DrainThread>` MINT rather than an opaque input: each drain opens its own context and token through `LatencySpine.Open` beside the mounted instrument set and the ledger exporter the terminal `Seal` feeds, so every drain-gated rollover receives the conductor's whole telemetry tail from the one root and an unbound tail refuses at boot instead of compiling on a trailing default. The cooperative and forced budgets are NOT on it — the conductor reads its own `DeadlineClass` rows, so a budget travelling beside the fold that owns it is a second value to disagree.
- `drain-rows` seats the late-registration cell: a bus subscription and an epoch lease both open after the build and both must drain, so the conductor folds this cell beside the contributed port fan and a participant that exists only at runtime is still a drain row rather than an orphan.
- `lifecycle` constructs the phase capsule over the composed dispatcher WHOLE — the capsule fires `AppHostPoint.Phase` itself and owns the `DegradationTap` the composition seats — so the one shielded fan-out exists before the capsule that fires it and an observer seating real I/O can never unwind past the transition dispatch.
- `verdict-fallback` binds `FlagVerdict.Inert` as the whole verdict function so an absent features API and an unready provider answer ONE shape at every consumer.
- `membership` seats the cluster view over the resolver, the per-authority `UriHealthCheck`, and the three decoded membership arrows; `peer-roster` seats the local attach set whose `contribute` closure is the two-tier edge `Wire/companion` and `Wire/coordination` both declare — the two rows reference each other only inside lambdas the fold never runs, so the mutual read resolves after both are registered, and the peer endpoint projects from the manifest's own `SocketPath` through `UnixDomainSocketEndPoint` so the UDS contract carries one address rather than two encodings obliged to agree.
- `coordination-seat` constructs the ONE `LeaseElection.Runtime` off the coordination seed's four decoded lease arrows, because those decoded arrows are its only producer and no fold below this one reaches them; the same row FORCES `LeasePolicy.Outlasts`, since this is where the reclamation window meets the drain bounds it must outlast and a proof no reader forces guarantees nothing.
- `control-inbound` completes `ControlRuntime` with the drain arrow over the degradation, support, source, and wire values in `ControlSeed`.
- `design-regime` is the seat-law row this doctrine binds on any PRODUCT root that composes `Rasm.Bim` — it lives on that root's own ledger, never here, because this package references the kernel alone and a Bim type cannot appear in this fence, exactly as the `BrickBinding` class election rides the composing root. The root elects the project's national design regime ONCE: `StageLabels.Nation` (the typed `Option<ICountry>` off the compiled `IGovernance.Country` pin, `Rasm.Bim/Planning/schedule#SCHEDULE`) feeds `AnnexRegime.Of(ICountry)` (`Rasm.Bim/Model/eurocode#EUROCODE_ALGEBRA` — the ISO-keyed nation→annex bridge whose row KEY is the SAF `ExcelNationalCode`) into the `EurocodePolicy` the root constructs, and the SAME `Option<AnnexRegime>` threads to `SafEmit.Export` (`Rasm.Bim/Exchange/export#SAF_EMIT`). Both parameters are REQUIRED and undefaulted at their Bim owners, so an unelected root breaks loudly at compile rather than silently designing under `Recommended` or writing no design code cell; a second election beside the export call, or a free country string standing in for the typed nation, forks the national annex the eurocode tables and the SAF workbook must share.
- `bim-compute-tessellation` is likewise a PRODUCT-root module row, never an AppHost project reference: the root that references both packages binds Bim's `ITessellationCompanion` directly to one `BimComputeCompanion`. The outer app call supplies its existing `CorrelationId` to `TessellationRequest.Resolve`; the adapter passes it to the Compute-owned singleton `CallSpineFactory`, which mints one spine for source put, tessellation, and output fetch. The adapter frames and puts the IFC source, projects the admitted `ArtifactRef` through `TessellationWire.Project`, drives `CompanionEdge`, and passes the returned `CompanionArtifact.Response` and GLB to `TessellationWire.Admit`. `ClockPolicy`, `WireServices`, and `StreamPool` remain composition singletons. `ValidateOnBuild` and `ValidateScopes` prove the constructor graph.
- `AgentSeed.Leases` is the ONE bearer holder both the `membership` probe and the `wire-seat` HTTP lane dereference, each at its own moment — per probe and per send — so a lease that `agent-boot` armed and its own occurrence later renewed reaches every hop with no re-registration at either seat and no held copy anywhere to go stale; `WireSeed.Credentials` is what tells them WHICH registration answers for a dialed authority, and an authority carrying no row is anonymous by declaration.
- Every `<module>-seat` row registers what its owners declare while the collection is editable; every `<module>-boot` row runs the gates whose facts exist only after the build. A gate whose refusal must stop the process lands there, so a refused trust anchor, an unhosted solver, an unrebuilt member set, or a peer surface the registry never took names itself at boot rather than at first call.

```csharp
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record RootBinding {
    private RootBinding() { }

    public sealed record Seated(string Name, Func<IServiceCollection, RootInputs, Fin<IServiceCollection>> Apply) : RootBinding;

    public sealed record Proven(string Name, Func<IServiceProvider, RootInputs, Fin<Unit>> Apply) : RootBinding;
}

public sealed record RootInputs(
    TelemetryComposition Telemetry,
    LaneGuard.Composition Lanes,
    Seq<LanePolicy> LaneRows,
    ChangefeedPort Changefeed,
    Func<CompanionPeer, IO<int>> ChildResidual,
    ConsumptionProfile Profile,
    ClockPolicy Clocks,
    CorrelationId Correlation,
    DeploymentTopology Topology,
    IConfiguration Configuration,
    RoleName Role,
    string Group,
    CoordinationArrows Coordination,
    ControlSeed Control,
    ObservabilitySeed Observability,
    WireSeed Wire,
    AgentSeed Agent,
    SandboxSeed Sandbox,
    HookSet<AppHostPoint, AppHostFact, TelemetrySource> Hooks);

public sealed record CoordinationArrows(
    Func<string, string, Duration, Fin<Unit>> MemberUpsert,
    Func<string, string, Fin<Unit>> MemberRelease,
    Func<string, Fin<Seq<(string Member, Instant Until)>>> MemberScan,
    Func<string, LeasePolicy, Fin<(ulong Generation, Instant Deadline)>> AcquireLease,
    Func<string, LeasePolicy, ulong, Fin<(ulong Generation, Instant Deadline)>> RenewLease,
    Func<string, ulong, Fin<Unit>> GuardWrite,
    Func<string, ulong, Fin<Unit>> ReleaseLease);

public sealed record ControlSeed(
    DegradationCell Degradation,
    ActivitySource Source,
    SupportRuntime Support,
    JsonSerializerOptions Wire);

public sealed record ObservabilitySeed(
    Seq<(DriverProbe Row, IHealthCheck Check)> Probes,
    EnergyCell Energy,
    Seq<HealthContributorPort> Health,
    Seq<SupportContributorPort> Support,
    Duration ProbeCadence,
    Duration RetentionSweep,
    PerfMapType Symbols,
    FrozenDictionary<string, string> Stamps,
    ProfileAttribution Attribute,
    Option<ISystemdNotifier> Notifier);

public sealed record WireSeed(
    Seq<(OutboundHop Hop, Seq<WeightedUriEndpoint> Routes)> Lanes,
    HashMap<Uri, string> Credentials,
    Seq<(Topic Topic, Seq<(string Name, Func<DomainEvent, IO<Unit>> Consume)> Subscribers)> Subscriptions,
    KeyedLane.Composition Keyed,
    OutboxRelay.Runtime Outbox,
    Func<Fin<OutboxOrdinal>> Watermark,
    LiveWireRuntime LiveWire,
    IngressPolicy Ingress,
    Seq<ServedPlane> Planes,
    BindRequest Listener,
    ModalityRow Modality);

public sealed record AgentSeed(
    HashMap<TenantId, GrantScope> Standing,
    CapabilitySet<ModalKind> Modalities,
    Seq<string> Models,
    TiktokenTokenizer Tokenizer,
    Func<IServiceProvider, IChatClient> Chat,
    Func<TensorOpFamily, JsonElement, Fin<CommandBody>> TensorCompile,
    Func<string, JsonElement, Fin<CommandBody>> ModelCompile,
    GovernanceLedger Ledger,
    FederationRuntime Federation,
    Seq<(FederatedServer Server, string Uri, BindingSpec Spec)> Subscriptions,
    IdentityRuntime Identity,
    LeaseRoster Leases);

public sealed record SandboxSeed(
    FileInfo TrustRoot,
    DirectoryInfo Staging,
    string ContractVersion,
    Func<AdmissionSubject, TrustPolicy> PolicyOf,
    HashMap<Isolation, VehicleProvider> Vehicles,
    int StackBytes,
    Duration EpochPeriod,
    CapabilitySet<SolverKind> Hosted,
    Seq<SolverManifest> Solvers,
    Func<SolverManifest, Fin<Seq<(NuGetVersion Version, PluginArtifact Artifact)>>> Catalog,
    Func<string, Option<NuGetVersion>> Installed,
    Func<Seq<CapabilityDescriptor>, IO<Unit>> Project,
    GrantScope Scope,
    UpdateChannel Channel,
    FleetRuntime Fleet);

public static class CompositionRoot {
    public static readonly Seq<RootBinding> Ledger =
    [
        new RootBinding.Seated("hooks-mount", static (services, inputs) =>
            HookRegistry.Mount([.. inputs.Hooks.Points]).Map(census => services
                .Add(ServiceDescriptor.Describe(
                    typeof(HookSet<AppHostPoint, AppHostFact, TelemetrySource>), _ => inputs.Hooks, ServiceLifetime.Singleton))
                .Add(ServiceDescriptor.Describe(typeof(HookRegistry), _ => census, ServiceLifetime.Singleton))
                .Add(ServiceDescriptor.Describe(typeof(FaultCell), _ => inputs.Hooks.Faults, ServiceLifetime.Singleton)))),

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

        new RootBinding.Seated("drain-thread", static (services, _) =>
            Fin.Succ(services.Add(ServiceDescriptor.Describe(
                typeof(Func<DrainThread>),
                static provider => (Func<DrainThread>)(() => Threaded(
                    LatencySpine.Open(
                        provider.GetRequiredService<ILatencyContextProvider>(),
                        provider.GetRequiredService<ILatencyContextTokenIssuer>(),
                        LatencyCheckpoint.Drain),
                    provider.GetRequiredService<InstrumentSet>(),
                    provider.GetRequiredService<ILatencyDataExporter>())),
                ServiceLifetime.Singleton)))),

        new RootBinding.Seated("drain-rows", static (services, _) =>
            Fin.Succ(services.Add(ServiceDescriptor.Describe(
                typeof(Atom<Seq<DrainRow>>), static _ => Atom(Seq<DrainRow>()), ServiceLifetime.Singleton)))),

        new RootBinding.Seated("lifecycle", static (services, inputs) =>
            Fin.Succ(services.Add(ServiceDescriptor.Describe(
                typeof(Lifecycle),
                _ => new Lifecycle(inputs.Profile, inputs.Clocks, inputs.Correlation, inputs.Hooks, inputs.Key),
                ServiceLifetime.Singleton)))),

        new RootBinding.Seated("verdict-fallback", static (services, _) =>
            Fin.Succ((services.TryAdd(ServiceDescriptor.Describe(
                typeof(Func<EvaluationContext, FlagVerdict>),
                static _ => (Func<EvaluationContext, FlagVerdict>)(static _ => FlagVerdict.Inert),
                ServiceLifetime.Singleton)), services).Item2)),

        new RootBinding.Seated("membership", static (services, inputs) =>
            Fin.Succ(services
                .Add(ServiceDescriptor.Describe(
                    typeof(Membership.Runtime),
                    provider => new Membership.Runtime(
                        NodeId: Environment.ProcessId,
                        Role: inputs.Role,
                        Group: inputs.Group,
                        Health: provider.GetRequiredService<HealthCheckService>(),
                        Local: provider.GetRequiredService<WireHealthRow>(),
                        Remote: async (authority, token) =>
                            (await new UriHealthCheck(
                                    new UriHealthCheckOptions().UseGet().AddUri(authority, uri =>
                                        inputs.Wire.Credentials.Find(authority)
                                            .Bind(id => inputs.Agent.Leases.Bearer(id, inputs.Clocks.Now))
                                            .Match(
                                                Some: drawn => uri.AddCustomHeader("Authorization", $"Bearer {drawn}"),
                                                None: () => uri)),
                                    () => provider.GetRequiredService<IHttpClientFactory>().CreateClient(nameof(Membership)))
                                .CheckHealthAsync(new HealthCheckContext(), token)).Status,
                        Attached: pid => provider.GetRequiredService<PeerRoster>().Attached.Exists(entry => entry.Pid == pid),
                        MemberUpsert: inputs.Coordination.MemberUpsert,
                        MemberRelease: inputs.Coordination.MemberRelease,
                        MemberScan: inputs.Coordination.MemberScan,
                        View: Atom(MembershipView.Empty),
                        Scheme: DialScheme.Secure,
                        Clocks: inputs.Clocks,
                        Staleness: LeasePolicy.Maintenance.CrashStaleness),
                    ServiceLifetime.Singleton))
                .AddHttpClient(nameof(Membership))
                .AddServiceDiscovery()
                .AddStandardResilienceHandler()
                .Services)),

        new RootBinding.Seated("peer-roster", static (services, inputs) =>
            Fin.Succ(services.Add(ServiceDescriptor.Describe(
                typeof(PeerRoster),
                provider => PeerRoster.Boot(
                    service: inputs.Profile.HostKey,
                    contribute: (credential, manifest) => ignore(Membership.Contribute(
                        provider.GetRequiredService<Membership.Runtime>(),
                        credential.Pid,
                        inputs.Role,
                        new UnixDomainSocketEndPoint(manifest.SocketPath))),
                    clocks: inputs.Clocks),
                ServiceLifetime.Singleton)))),

        new RootBinding.Seated("control-inbound", static (services, inputs) =>
            Fin.Succ(services.Add(ServiceDescriptor.Describe(
                typeof(ControlRuntime),
                provider => new ControlRuntime(
                    Degradation: inputs.Control.Degradation,
                    Drain: inherited => Conducted(provider, inherited),
                    Clocks: inputs.Clocks,
                    Correlation: inputs.Correlation,
                    Source: inputs.Control.Source,
                    Support: inputs.Control.Support),
                ServiceLifetime.Singleton)))),

        // --- [MODULE_SEATS] ------------------------------------------------------------
        new RootBinding.Seated("telemetry-seat", static (services, inputs) =>
            RedactionRegistration.Federated([.. inputs.Telemetry.Classifications]).ToFin().Map(_ => services
                .Add(ServiceDescriptor.Describe(typeof(TelemetryComposition), _ => inputs.Telemetry, ServiceLifetime.Singleton))
                .Add(ServiceDescriptor.Describe(typeof(SpanBand), _ => inputs.Telemetry.Band, ServiceLifetime.Singleton))
                .Fold(
                    inputs.Telemetry.Offline.Armed
                        ? Seq(Participant("otlp-queues", DrainBand.Telemetry, 1, static provider => token =>
                            toSeq(provider.GetRequiredService<TelemetryComposition>().Queues.Values)
                                .TraverseM(queue => queue.Release(token)).As().Map(static _ => unit)))
                        : Seq<ServiceDescriptor>(),
                    static (current, row) => current.Add(row)))),

        new RootBinding.Seated("observability-seat", static (services, inputs) =>
            UtilizationCell.Of(PressurePolicy.Canonical.Source, inputs.Clocks.Line, inputs.Key).Map(utilization => services
                .Add(ServiceDescriptor.Describe(typeof(UtilizationCell), _ => utilization, ServiceLifetime.Singleton))
                .Add(ServiceDescriptor.Describe(typeof(InstrumentSet), _ => inputs.Telemetry.Signals, ServiceLifetime.Singleton))
                .Add(ServiceDescriptor.Describe(typeof(AlertCell), _ => new AlertCell(AlertPolicy.Canonical), ServiceLifetime.Singleton))
                .Add(ServiceDescriptor.Describe(
                    typeof(DegradationCell),
                    provider => new DegradationCell(
                        DegradationPolicy.Canonical, inputs.Clocks.Clock, inputs.Correlation,
                        provider.GetRequiredService<HookSet<AppHostPoint, AppHostFact, TelemetrySource>>(), inputs.Key),
                    ServiceLifetime.Singleton))
                .Add(ServiceDescriptor.Describe(
                    typeof(BenchmarkRun.Session),
                    provider => new BenchmarkRun.Session(
                        Source: inputs.Control.Source,
                        Instruments: inputs.Telemetry.Signals,
                        Hooks: inputs.Hooks,
                        Signals: ProfileTracking.Canonical,
                        Capture: ProfileCapturePolicy.Canonical,
                        Symbols: inputs.Observability.Symbols,
                        Stamps: inputs.Observability.Stamps,
                        Attribute: inputs.Observability.Attribute),
                    ServiceLifetime.Singleton))
                .Add(ServiceDescriptor.Describe(
                    typeof(SupportRuntime), _ => inputs.Control.Support, ServiceLifetime.Singleton))
                .AddHealthChecks()
                .Register([.. Probed(inputs, utilization) + inputs.Observability.Health.Bind(static port => port.Rows)])
                .Services
                .Configure<HealthCheckPublisherOptions>(static options =>
                    options.Period = DegradationPolicy.Canonical.PublishPeriod.ToTimeSpan()))),

        new RootBinding.Seated("schedule-arrow", static (services, inputs) =>
            Fin.Succ(services
                .Add(ServiceDescriptor.Describe(
                    typeof(Atom<HashMap<string, ScheduleEntry>>),
                    static _ => Atom(HashMap<string, ScheduleEntry>()), ServiceLifetime.Singleton))
                .Add(ServiceDescriptor.Describe(
                    typeof(Func<ScheduleEntry, IO<Unit>>),
                    provider => new Func<ScheduleEntry, IO<Unit>>(entry => Armed(
                        provider.GetRequiredService<Atom<HashMap<string, ScheduleEntry>>>(), inputs.Clocks, entry)),
                    ServiceLifetime.Singleton)))),

        new RootBinding.Seated("runtime-seat", static (services, inputs) =>
            Fin.Succ(services
                .Add(ServiceDescriptor.Describe(
                    typeof(OrchestrationRuntime),
                    provider => new OrchestrationRuntime(
                        Dispatch: provider.GetRequiredService<DispatchRuntime>(),
                        Store: provider.GetRequiredService<StepStatePort>(),
                        Assess: provider.GetRequiredService<Func<CommandResult, Option<StepDisposition>>>(),
                        Redrive: Orchestrator.StepRedrive,
                        Lease: provider.GetRequiredService<LeaseElection.Runtime>(),
                        Schedule: provider.GetRequiredService<Func<ScheduleEntry, IO<Unit>>>(),
                        Clocks: inputs.Clocks),
                    ServiceLifetime.Singleton))
                .Add(ServiceDescriptor.Describe(
                    typeof(Atom<Option<InMemoryProvider>>), static _ => Atom(Option<InMemoryProvider>.None), ServiceLifetime.Singleton))
                .Add(ServiceDescriptor.Describe(
                    typeof(Atom<Option<LaneGuard.Runtime>>), static _ => Atom(Option<LaneGuard.Runtime>.None), ServiceLifetime.Singleton))
                .Add(Participant("lane-permits", DrainBand.Telemetry, 0, static provider => _ =>
                    provider.GetRequiredService<LanePermits>().Reclaim()
                        .Bind(_ => provider.GetRequiredService<LanePermits>().Drain())))
                .Fold(Enrolled(inputs), static (current, row) => current.Add(row)))),

        new RootBinding.Seated("wire-seat", static (services, inputs) =>
            inputs.Wire.Lanes
                .Fold(Fin.Succ(services), (held, lane) => held.Bind(current =>
                    HttpLane.Wire(current, inputs.Configuration, lane.Hop, static row => row.Bound,
                        bearer: authority => inputs.Wire.Credentials.Find(authority)
                            .Bind(id => inputs.Agent.Leases.Bearer(id, inputs.Clocks.Now)),
                        [.. lane.Routes])))
                .Map(current => KeyedLane.Register(current, inputs.Wire.Keyed)
                    .Add(ServiceDescriptor.Describe(typeof(LiveWireRuntime), _ => inputs.Wire.LiveWire, ServiceLifetime.Singleton))
                    .Add(ServiceDescriptor.Describe(typeof(OutboxRelay.Runtime), _ => inputs.Wire.Outbox, ServiceLifetime.Singleton))
                    .Add(ServiceDescriptor.Describe(
                        typeof(EventBus.Runtime),
                        provider => new EventBus.Runtime(
                            Delivery: provider.GetRequiredService<DeliveryRuntime>(),
                            Level: () => provider.GetRequiredService<DegradationCell>().Level,
                            Instruments: inputs.Telemetry.Signals,
                            Register: row => ignore(provider.GetRequiredService<Atom<Seq<DrainRow>>>().Swap(held => held.Add(row))),
                            Clocks: inputs.Clocks,
                            Spine: provider.GetRequiredService<Lifecycle>().Spine),
                        ServiceLifetime.Singleton))
                    .Add(ServiceDescriptor.Describe(
                        typeof(Atom<Option<EventBus.Cell>>), static _ => Atom(Option<EventBus.Cell>.None), ServiceLifetime.Singleton))
                    .Add(Participant("live-wire", DrainBand.Interaction, 0, static provider => _ =>
                        provider.GetRequiredService<LiveWireRuntime>() is var runtime
                            ? runtime.Bound().TraverseM(handle => LiveWire.Release(runtime, handle)).As().Map(static _ => unit)
                            : IO.pure(unit)))
                    .Add(ServiceDescriptor.Describe(
                        typeof(HealthContributorRow),
                        provider => BindingHealth.Contribute(
                            provider.GetRequiredService<LiveWireRuntime>(), inputs.Observability.ProbeCadence),
                        ServiceLifetime.Singleton)))),

        new RootBinding.Seated("coordination-seat", static (services, inputs) =>
            Try.lift(static () => Fin.Succ(LeasePolicy.Outlasts)).Run().Bind(static inner => inner)
                .MapFail(static _ => (Error)new KernelFault.InvalidValue(
                    Label: $"{nameof(LeasePolicy)}.{nameof(LeasePolicy.Maintenance)}",
                    Requirement: "<a crash-staleness window outlasting the cooperative and forced drain bounds>"))
                .Map(_ => services
                    .Add(ServiceDescriptor.Describe(
                        typeof(LeaseElection.Runtime),
                        _ => new LeaseElection.Runtime(
                            AcquireLease: inputs.Coordination.AcquireLease,
                            RenewLease: inputs.Coordination.RenewLease,
                            GuardWrite: inputs.Coordination.GuardWrite,
                            ReleaseLease: inputs.Coordination.ReleaseLease,
                            Lease: LeasePolicy.Maintenance),
                        ServiceLifetime.Singleton))
                    .Add(ServiceDescriptor.Describe(
                        typeof(FencedRuntime),
                        provider => new FencedRuntime(
                            NodeId: Environment.ProcessId,
                            Lease: provider.GetRequiredService<LeaseElection.Runtime>(),
                            Clocks: inputs.Clocks,
                            Staleness: LeasePolicy.Maintenance.CrashStaleness),
                        ServiceLifetime.Singleton))
                    )),

        new RootBinding.Seated("companion-seat", static (services, inputs) =>
            Fin.Succ(ServiceHost.Register(services, [.. inputs.Wire.Planes])
                .Add(ServiceDescriptor.Describe(typeof(IngressPolicy), _ => inputs.Wire.Ingress, ServiceLifetime.Singleton))
                .Add(ServiceDescriptor.Describe(typeof(ModalityRow), _ => inputs.Wire.Modality, ServiceLifetime.Singleton))
                .Add(Participant("host-binding", DrainBand.Interaction, 1, static provider => _ =>
                    HostBinding.Release(provider.GetRequiredService<BoundEndpoint>())))
                .Add(Participant("degradation-cascade", DrainBand.Interaction, 2, static provider => _ =>
                    provider.GetRequiredService<PeerRoster>().Attached
                        .TraverseM(entry => DegradationCascade.Cascade(
                            entry.Peer,
                            provider.GetRequiredService<DegradationCell>().Level,
                            nameof(DrainBand.Interaction), provider.GetRequiredService<ModalityRow>()))
                        .As().Map(static _ => unit))))),

        new RootBinding.Seated("agent-seat", static (services, inputs) =>
            FederationProjection.Federate(
                    inputs.Agent.Federation,
                    ModelProjection.Project(
                        TensorProjection.Project(services, inputs.Agent.TensorCompile),
                        inputs.Agent.Models, inputs.Agent.Tokenizer, inputs.Agent.ModelCompile))
                .Run()
                .Map(federated => federated
                    .Add(ServiceDescriptor.Describe(
                        typeof(ConsentRoster),
                        _ => new ConsentRoster(inputs.Agent.Standing, AmbientSlot<Consent>.One("consent-elevation")),
                        ServiceLifetime.Singleton))
                    .Add(ServiceDescriptor.Describe(
                        typeof(GrantBroker),
                        provider => new GrantBroker(
                            Ledger: Atom(HashMap<TenantId, Budget>()),
                            ConsentOf: tenant => provider.GetRequiredService<ConsentRoster>().Of(tenant, inputs.Clocks.Now),
                            Clocks: inputs.Clocks),
                        ServiceLifetime.Singleton))
                    .Add(ServiceDescriptor.Describe(
                        typeof(Atom<EventLog.Chain>), _ => Atom(EventLog.Chain.Genesis), ServiceLifetime.Singleton))
                    .Add(ServiceDescriptor.Describe(
                        typeof(CommandRuntime),
                        provider => new CommandRuntime(
                            Registry: provider.GetRequiredService<CapabilityRegistry>(),
                            Broker: provider.GetRequiredService<GrantBroker>(),
                            Lanes: provider.GetRequiredService<Atom<Option<LaneGuard.Runtime>>>(),
                            Dispatch: provider.GetRequiredService<Func<CommandBody, Spec, CommandArguments, IO<Fin<DispatchResult>>>>(),
                            CompensationOf: provider.GetRequiredService<Func<string, Option<string>>>(),
                            Clocks: inputs.Clocks,
                            Spine: provider.GetRequiredService<Lifecycle>().Spine),
                        ServiceLifetime.Singleton))
                    .Add(ServiceDescriptor.Describe(
                        typeof(DispatchRuntime),
                        provider => new DispatchRuntime(
                            Command: provider.GetRequiredService<CommandRuntime>(),
                            Chain: provider.GetRequiredService<Atom<EventLog.Chain>>(),
                            Context: inputs.Telemetry.Determinism,
                            Changefeed: inputs.Changefeed,
                            Instruments: inputs.Telemetry.Signals,
                            Hooks: inputs.Hooks,
                            Key: inputs.Key),
                        ServiceLifetime.Singleton))
                    .Add(ServiceDescriptor.Describe(
                        typeof(GovernanceRuntime), provider => Governed(provider, inputs), ServiceLifetime.Singleton))
                    .Add(ServiceDescriptor.Describe(
                        typeof(ReasoningRuntime),
                        provider => new ReasoningRuntime(
                            Chat: provider.GetRequiredService<IChatClient>(),
                            Adopted: provider.GetRequiredService<McpAdoption>(),
                            Ledger: inputs.Agent.Ledger,
                            Clocks: inputs.Clocks,
                            Faults: provider.GetRequiredService<FaultCell>(),
                            Wire: inputs.Control.Wire),
                        ServiceLifetime.Singleton))
                    .Add(ServiceDescriptor.Describe(
                        typeof(McpAdoption),
                        provider => ToolProjection.Adopt(
                            provider.GetRequiredService<McpRuntime>(),
                            ToolProjection.Project(provider.GetRequiredService<McpRuntime>())),
                        ServiceLifetime.Singleton))
                    .Add(ServiceDescriptor.Describe(typeof(IdentityRuntime), _ => inputs.Agent.Identity, ServiceLifetime.Singleton))
                    .AddAuthorizationCore()
                    .AddChatClient(provider => ModelGovernance.Compose(
                        provider.GetRequiredService<GovernanceRuntime>(), inputs.Agent.Chat(provider)).Client)
                    .Services)),

        new RootBinding.Seated("sandbox-seat", static (services, inputs) =>
            SupplyChainGate.Runtime.Of(
                    new TrustAnchor.PinnedCase(inputs.Sandbox.TrustRoot), inputs.Sandbox.PolicyOf,
                    inputs.Sandbox.Staging, inputs.Sandbox.ContractVersion, inputs.Clocks)
                .Bind(gate => FeedBinding.Of(inputs.Sandbox.Channel, inputs.Configuration).Map(feed => services
                    .Add(ServiceDescriptor.Describe(typeof(SupplyChainGate.Runtime), _ => gate, ServiceLifetime.Singleton))
                    .Add(ServiceDescriptor.Describe(
                        typeof(SandboxRuntime),
                        provider => new SandboxRuntime(
                            Gate: gate,
                            Command: provider.GetRequiredService<CommandRuntime>(),
                            Engine: SandboxRuntime.Preempting(inputs.Sandbox.StackBytes),
                            EpochPeriod: inputs.Sandbox.EpochPeriod,
                            Vehicles: inputs.Sandbox.Vehicles,
                            Clocks: inputs.Clocks,
                            Spine: provider.GetRequiredService<Lifecycle>().Spine),
                        ServiceLifetime.Singleton))
                    .Add(ServiceDescriptor.Describe(
                        typeof(UpdateMachine),
                        provider => new UpdateMachine(
                            feed, provider.GetRequiredService<Lifecycle>(),
                            gate,
                            provider.GetRequiredService<IMeterFactory>().Create(nameof(UpdateMachine))),
                        ServiceLifetime.Singleton))
                    .Add(ServiceDescriptor.Describe(typeof(FleetRuntime), _ => inputs.Sandbox.Fleet, ServiceLifetime.Singleton))
                    .Add(ServiceDescriptor.Describe(
                        typeof(Atom<Seq<HostedSolver>>), static _ => Atom(Seq<HostedSolver>()), ServiceLifetime.Singleton))))),

        // --- [MODULE_BOOTS] ------------------------------------------------------------
        new RootBinding.Proven("lane-closure", static (provider, inputs) =>
            LaneGuard.Proven(
                    provider.GetRequiredService<ResiliencePipelineProvider<string>>(), inputs.Lanes, [.. inputs.LaneRows])
                .Map(lanes => ignore(provider.GetRequiredService<Atom<Option<LaneGuard.Runtime>>>().Swap(_ => Some(lanes))))),

        new RootBinding.Proven("hop-closure", static (provider, _) =>
            KeyedLane.Proven(provider.GetRequiredService<ResiliencePipelineProvider<string>>())),

        new RootBinding.Proven("hop-checkpoint", static (provider, _) =>
            Try.lift(() => Fin.Succ(provider.GetRequiredService<ILatencyContextTokenIssuer>()
                    .GetCheckpointToken(LatencyCheckpoint.Hop.Key))).Run().Bind(static inner => inner)
                .Bind(static token => token.Position >= 0
                    ? Fin.Succ(unit)
                    : Fin.Fail<Unit>(new KernelFault.InvalidValue(
                        Label: LatencyCheckpoint.Hop.Key,
                        Requirement: "<a checkpoint name the folded latency roster carries>")))),

        new RootBinding.Proven("observability-boot", static (provider, inputs) =>
            Scheduled(provider,
                Cadence("alert-sweep", DegradationPolicy.Canonical.PublishPeriod, DeadlineClass.HealthProbe, () =>
                    AlertEngine.Sweep(
                            new AlertEngine.Runtime(Hooks: inputs.Hooks, Key: inputs.Key),
                            provider.GetRequiredService<AlertCell>(),
                            provider.GetRequiredService<DegradationCell>().Read(),
                            inputs.Clocks.Now)
                        .Map(static _ => unit)),
                Cadence("support-sweep", inputs.Observability.RetentionSweep, DeadlineClass.SupportWindow, () =>
                    SupportLedger.Sweep(provider.GetRequiredService<SupportRuntime>()).Map(static _ => unit)))),

        new RootBinding.Proven("runtime-boot", static (provider, inputs) =>
            FlagCompilation.Register(
                    provider.GetRequiredService<FeaturesRuntime>(), provider.GetRequiredService<FlagRegistry>(),
                    provider.GetRequiredService<OperatorOverride>(), inputs.Profile.HostKey)
                .Run()
                .Map(compiled => ignore(provider.GetRequiredService<Atom<Option<InMemoryProvider>>>().Swap(_ => Some(compiled))))
                .Bind(_ => CrashResume
                    .Resume(provider.GetRequiredService<OrchestrationRuntime>(), TenantContext.Current)
                    .Run()
                    .Map(static _ => unit))
                .Bind(_ => Scheduled(provider,
                    Cadence("workflow-reclaim", LeasePolicy.Maintenance.CrashStaleness, DeadlineClass.LaneFold, () =>
                        CrashResume.Reclaim(provider.GetRequiredService<OrchestrationRuntime>(), TenantContext.Current)
                            .Map(static _ => unit))))),

        new RootBinding.Proven("wire-boot", static (provider, inputs) =>
            OutboundSurface.Seat(provider.GetRequiredService<OutboundRuntime>())
                .Bind(_ => OutboundSurface
                    .Enforce(
                        provider.GetRequiredService<OutboundRuntime>(),
                        provider.GetRequiredService<DegradationCell>().Level)
                    .Run())
                .Bind(_ => EventBus.Mount(provider.GetRequiredService<EventBus.Runtime>(), [.. inputs.Wire.Subscriptions]))
                .Map(bus => ignore(provider.GetRequiredService<Atom<Option<EventBus.Cell>>>().Swap(_ => Some(bus))))
                .Bind(_ => Scheduled(provider,
                    Cadence("outbox-sweep", inputs.Wire.Outbox.Cadence, () =>
                        RoleElection.Elect(
                                provider.GetRequiredService<FencedRuntime>(),
                                provider.GetRequiredService<Membership.Runtime>().View.Value,
                                RoleName.Create(OutboxRelay.SweepRole.Id))
                            .Bind(elected => elected.Match(
                                Succ: holding => RoleElection
                                    .Hold(
                                        provider.GetRequiredService<FencedRuntime>(),
                                        holding, FenceVerb.Renew)
                                    .Bind(_ => inputs.Wire.Watermark().Match(
                                        Succ: watermark => OutboxRelay
                                            .Sweep(inputs.Wire.Outbox, TenantContext.Current, watermark)
                                            .Map(static _ => unit),
                                        Fail: IO.fail<Unit>)),
                                Fail: static _ => IO.pure(unit))))))),

        new RootBinding.Proven("coordination-boot", static (provider, _) =>
            Membership.Rebuild(provider.GetRequiredService<Membership.Runtime>()).Run()
                .Bind(static view => view.ToFin())
                .Bind(_ => Scheduled(provider, Membership.Cadence(provider.GetRequiredService<Membership.Runtime>())))),

        new RootBinding.Proven("companion-boot", static (provider, inputs) =>
            HostBinding.Acquire(inputs.Wire.Listener).Run()
                .Map(static _ => unit)),

        new RootBinding.Proven("agent-boot", static (provider, inputs) =>
            provider.GetRequiredService<CapabilityRegistry>()
                .Mount(provider.GetRequiredService<InstrumentSet>())
                .Bind(_ => inputs.Agent.Subscriptions
                    .TraverseM(row => FederationSubscription.Subscribe(
                        inputs.Agent.Federation, row.Server, row.Uri, row.Spec,
                        provider.GetRequiredService<ChannelWriter<ExternalValue>>()))
                    .As().Run().Map(static _ => unit))
                .Bind(_ => Scheduled(provider, [.. inputs.Agent.Leases.Refreshes]))),

        new RootBinding.Proven("sandbox-boot", static (provider, inputs) =>
            Hosting(provider, inputs)
                .Bind(hosting => SolverHost
                    .Register(hosting, inputs.Sandbox.Solvers, inputs.Sandbox.Scope, inputs.Key)
                    .Run())
                .Bind(static hosted => hosted.ToFin())
                .Map(hosted => Paced(provider, hosted, inputs.Key))
                .Bind(_ => Resumed(provider))),

        new RootBinding.Proven("capsule-unload", static (provider, inputs) =>
            inputs.Topology != DeploymentTopology.InHost
                ? Fin.Succ(unit)
                : CapsulePins
                    .Traverse(pin => provider.GetService(pin) is IDisposable or IAsyncDisposable
                        ? Validation<Error, Unit>.Success(unit)
                        : new KernelFault.InvalidValue(Label: pin.Name, Requirement: "<a container-owned disposable>"))
                    .As()
                    .Map(static _ => unit)
                    .ToFin()),
    ];

    public static readonly Seq<Type> CapsulePins = [typeof(UtilizationCell), typeof(TelemetryComposition)];

    public static Fin<IServiceProvider> Arm(
        ServiceCollection services, RootInputs inputs, ServiceProviderOptions options,
        params ReadOnlySpan<ModuleContribution> modules) =>
        Seated(services, inputs)
            .Bind(seated => seated.Compose(modules))
            .Map(_ => (IServiceProvider)services.BuildServiceProvider(options))
            .Bind(provider => Built(provider, inputs).Map(_ => provider));

    static Fin<ServiceCollection> Seated(ServiceCollection services, RootInputs inputs) =>
        Ledger.Traverse(row => row.Switch(
                state: (Services: (IServiceCollection)services, Inputs: inputs),
                seated: static (state, row) => row.Apply(state.Services, state.Inputs).Map(static _ => unit).ToValidation(),
                proven: static (_, _) => Validation<Error, Unit>.Success(unit)))
            .As()
            .Map(_ => services)
            .ToFin();

    static Fin<Unit> Built(IServiceProvider provider, RootInputs inputs) =>
        Ledger.Traverse(row => row.Switch(
                state: (Provider: provider, Inputs: inputs),
                seated: static (_, _) => Validation<Error, Unit>.Success(unit),
                proven: static (state, row) => row.Apply(state.Provider, state.Inputs).ToValidation()))
            .As()
            .Map(static _ => unit)
            .ToFin();

    // --- [COMPOSITION] -----------------------------------------------------------------
    static IO<PhaseCommit> Conducted(IServiceProvider provider, Duration inherited) =>
        from thread in IO.lift(provider.GetRequiredService<Func<DrainThread>>())
        from commit in provider.GetRequiredService<Lifecycle>().Drain(
            toSeq(provider.GetServices<DrainParticipantPort>())
                .Map(static row => new DrainRow(row.Name, row.Band, row.Rank, row.Drain))
                + provider.GetRequiredService<Atom<Seq<DrainRow>>>().Value,
            thread.Latency, thread.Checkpoint, thread.Instruments, inherited)
        from _sealed in LatencySpine.Seal(thread.Exporter, thread.Latency)
        select commit;

    static DrainThread Threaded(
        (ILatencyContext Context, CheckpointToken Phase) opened, InstrumentSet instruments, ILatencyDataExporter exporter) =>
        new(opened.Context, opened.Phase, instruments, exporter);

    static ServiceDescriptor Participant(
        string name, DrainBand band, int rank, Func<IServiceProvider, Func<CancellationToken, IO<Unit>>> drain) =>
        ServiceDescriptor.Describe(
            typeof(DrainParticipantPort),
            provider => new DrainParticipantPort(name, band, rank, drain(provider)),
            ServiceLifetime.Singleton);

    static Seq<HealthContributorRow> Probed(RootInputs inputs, UtilizationCell utilization) =>
        inputs.Observability.Probes
            .Map(row => HealthContributorRow.Of(
                new ProbeSource.Driver(row.Row, row.Check), inputs.Observability.ProbeCadence))
            .Add(HealthContributorRow.Of(
                new ProbeSource.Gauge(utilization, inputs.Observability.Energy, PressurePolicy.Canonical),
                inputs.Observability.ProbeCadence));

    static Seq<ServiceDescriptor> Enrolled(RootInputs inputs) =>
        (from notifier in inputs.Observability.Notifier
         from enrollment in ProfileBoot.Enrolled()
         select Seq(
             ServiceDescriptor.Describe(
                 typeof(ScheduleEntry),
                 _ => new ScheduleEntry(
                     Key: nameof(ProfileBoot.Watchdog),
                     Spec: new OccurrenceSpec.Every(enrollment.Period),
                     Deadline: DeadlineClass.ReadyProbe,
                     Lease: None,
                     Redrive: RedrivePolicy.None,
                     Work: () => IO.lift(() => ProfileBoot.Watchdog(notifier)).Map(static _ => unit)),
                 ServiceLifetime.Singleton),
             ServiceDescriptor.Describe(
                 typeof(SupportContributorPort),
                 _ => new SupportContributorPort(
                     nameof(ProfileBoot),
                     SupportArtifact.ProcessDump(enrollment.Stalled, inputs.Control.Support.StorageRoot).ToSeq()),
                 ServiceLifetime.Singleton)))
        .IfNone(Seq<ServiceDescriptor>());

    static GovernanceRuntime Governed(IServiceProvider provider, RootInputs inputs) =>
        new(Services: provider,
            Cache: provider.GetRequiredService<IDistributedCache>(),
            Loggers: provider.GetRequiredService<ILoggerFactory>(),
            TelemetrySource: TelemetrySource.AppHost.Key,
            Policy: ReasoningPolicy.Auto(inputs.Telemetry.Determinism, DeadlineClass.LaneFold),
            Modalities: inputs.Agent.Modalities,
            Tokenizer: inputs.Agent.Tokenizer,
            Images: Optional(provider.GetService<IImageGenerator>()),
            Speech: Optional(provider.GetService<ISpeechToTextClient>()),
            Turn: AmbientSlot<ModelRoute>.One("reasoning-turn"),
            CacheEpoch: Seq<object>(inputs.Correlation),
            Ledger: inputs.Agent.Ledger,
            Verdict: provider.GetRequiredService<Func<EvaluationContext, FlagVerdict>>(),
            Targeting: provider.GetRequiredService<Func<EvaluationContext>>(),
            Broker: provider.GetRequiredService<GrantBroker>(),
            FilterClassification: DataClassificationSet.Of(DataClassification.UserContent),
            Redactors: provider.GetRequiredService<IRedactorProvider>(),
            Faults: provider.GetRequiredService<FaultCell>());

    static Fin<SolverHostRuntime> Hosting(IServiceProvider provider, RootInputs inputs) =>
        Isolation.Wasm.Row.Map(row => new SolverHostRuntime(
            Sandbox: provider.GetRequiredService<SandboxRuntime>(),
            Row: row,
            Mcp: provider.GetRequiredService<McpRuntime>(),
            Hosted: inputs.Sandbox.Hosted,
            Resolve: manifest => Ranked(inputs, manifest),
            CompileOf: (instance, negotiation, op) => arguments =>
                SandboxRows.Enter(instance, guest => Optional(guest.GetFunction(op.OpId)))
                    .Run()
                    .ToFin(new SolverFault.ContractRejected($"{instance.PluginId}.{op.OpId}: <no guest export>"))
                    .Map(export => new CommandBody(
                        instance.PluginId, op.OpId,
                        JsonSerializer.SerializeToElement(
                            export.Invoke(arguments.Payload.GetRawText(), negotiation.Tolerance), SuiteContracts.Host))),
            Project: inputs.Sandbox.Project));

    static Fin<PluginArtifact> Ranked(RootInputs inputs, SolverManifest manifest) =>
        inputs.Sandbox.Catalog(manifest)
            .Bind(offered => offered.Head
                .ToFin(new SupplyChainFault.VersionIncompatible($"{manifest.PluginId}: <no candidate offered>"))
                .Map(head => (Offered: offered, Policy: inputs.Sandbox.PolicyOf(new AdmissionSubject.Plugin(head.Artifact)))))
            .Bind(ranked => SupplyChainGate
                .Best(ranked.Policy, inputs.Sandbox.Installed(manifest.PluginId), ranked.Offered.Map(static row => row.Version))
                .ToFin()
                .Bind(best => ranked.Offered
                    .Find(row => row.Version == best)
                    .Map(static row => row.Artifact)
                    .ToFin(new SupplyChainFault.VersionIncompatible(
                        $"{manifest.PluginId}: {best.ToNormalizedString()} <left the catalog>"))));

    static Unit Paced(IServiceProvider provider, Seq<HostedSolver> hosted) =>
        provider.GetRequiredService<Atom<Seq<HostedSolver>>>() is var roster
            && ignore(roster.Swap(_ => hosted)) is var _
            && EpochPacer.Open(
                provider.GetRequiredService<SandboxRuntime>(),
                () => roster.Value.Map(static row => row.Instance)) is var lease
            ? ignore(provider.GetRequiredService<Atom<Seq<DrainRow>>>().Swap(held => held.Add(
                new DrainRow(nameof(EpochPacer), DrainBand.Compute, 0, _ => IO.lift(lease.Dispose)))))
            : unit;

    static Fin<Unit> Resumed(IServiceProvider provider) =>
        provider.GetRequiredService<UpdateMachine>() is var machine
            ? machine.Pending.Match(
                Some: asset => machine.Rollover(asset, provider.GetRequiredService<Func<DrainThread>>()()).Run().Map(static _ => unit),
                None: static () => Fin.Succ(unit))
            : Fin.Succ(unit);

    static Fin<Unit> Scheduled(IServiceProvider provider, params ReadOnlySpan<ScheduleEntry> entries) =>
        Iterable<ScheduleEntry>.FromSpan(entries)
            .Traverse(entry => provider.GetRequiredService<Func<ScheduleEntry, IO<Unit>>>()(entry).Run().ToValidation())
            .As()
            .Map(static _ => unit)
            .ToFin();

    static IO<Unit> Armed(Atom<HashMap<string, ScheduleEntry>> roster, ClockPolicy clocks, ScheduleEntry entry) =>
        Cell.Claim(roster, entry.Key, () => entry) switch {
            Transition<HashMap<string, ScheduleEntry>>.Committed => Occurring(roster, clocks, entry.Key).Fork(None).Map(static _ => unit),
            _ => IO.lift(() => ignore(roster.Swap(held => held.SetItem(entry.Key, entry)))),
        };

    static IO<Unit> Occurring(Atom<HashMap<string, ScheduleEntry>> roster, ClockPolicy clocks, string key) =>
        roster.Value.Find(key).Match(
            None: () => IO.pure(unit),
            Some: entry => SchedulePort.Next(entry, clocks.Now).Match(
                None: () => IO.lift(() => ignore(roster.Swap(held => held.Remove(key)))),
                Some: next => IO.yieldFor((next - clocks.Now).ToTimeSpan())
                    .Bind(_ => SchedulePort.Run(clocks, entry))
                    .Bind(_ => Occurring(roster, clocks, key))));

    static ScheduleEntry Cadence(string key, Duration every, DeadlineClass deadline, Func<IO<Unit>> work) =>
        new(Spec: new OccurrenceSpec.Every(every), Deadline: deadline,
            Lease: Some(LeasePolicy.Maintenance), Redrive: RedrivePolicy.None, Work: work);

    static ScheduleEntry Cadence(string key, ScheduleEntry declared, Func<IO<Unit>> work) =>
        declared with { Key = key, Work = work };
}
```

The following fence belongs in each product composition assembly that admits both Bim and Compute. The product passes its Compute module before `ProductModules.BimCompute` so the descriptor graph proves every constructor dependency at the one provider build; AppHost itself keeps its kernel-and-contracts-only dependency direction.

```csharp
using System;
using System.Threading;
using LanguageExt;
using Microsoft.Extensions.DependencyInjection;
using Rasm;
using Rasm.AppHost;
using Rasm.Bim;
using Rasm.Compute;
using static LanguageExt.Prelude;

namespace Rasm.Product;

public sealed class BimComputeCompanion(
    WireServices services,
    CallSpineFactory spines,
    StreamPool pool) : ITessellationCompanion {
    public IO<Fin<TessellationCross>> Cross(
        Rasm.Bim.TessellationRequest request,
        CorrelationId correlation,
        CancellationToken cancel) {
        CallSpine spine = spines.Create(correlation);
        WireCall calls = services.Bind(spine);
        return FrameEdge.Frames(request.SourceBytes).Match(
            Succ: partition => FrameEdge.Put(calls, spine, partition, cancel).Bind(uploaded => uploaded.Match(
                Succ: source => TessellationWire.Project(request, source, key).Match(
                    Succ: wire => CompanionEdge
                        .Tessellate(services, spine, pool, wire, cancel)
                        .Map(outcome => outcome.Bind(artifact =>
                            TessellationWire.Admit(request, artifact.Response, artifact.Glb, key))),
                    Fail: static error => IO.pure(Fin.Fail<TessellationCross>(error))),
                Fail: static error => IO.pure(Fin.Fail<TessellationCross>(error)))),
            Fail: static error => IO.pure(Fin.Fail<TessellationCross>(error)));
    }
}

public static class ProductModules {
    public static readonly ModuleContribution BimCompute = new(
        Module: nameof(BimComputeCompanion),
        Assembly: typeof(BimComputeCompanion).Assembly,
        Scan: None,
        Descriptors: HashMap<DescriptorSlot, Seq<ServiceDescriptor>>()
            .Add(DescriptorSlot.Service, Seq(
                ServiceDescriptor.Describe(
                    typeof(CallSpineFactory),
                    typeof(CallSpineFactory),
                    ServiceLifetime.Singleton),
                ServiceDescriptor.Describe(
                    typeof(ITessellationCompanion),
                    typeof(BimComputeCompanion),
                    ServiceLifetime.Singleton))),
        Registrars: Seq<Func<IServiceCollection, IServiceCollection>>(),
        Decorations: Seq<DecorationRow>());
}
```

## [07]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
