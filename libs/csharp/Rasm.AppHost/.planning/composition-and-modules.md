# [APPHOST_COMPOSITION_AND_MODULES]

One composition root per process folds a frozen module table into the service graph and freezes it. Composition owns three axes: the `ModuleContribution` row — assembly, scan, descriptor, contributor, and registrar columns — the one-pass receipted composition fold, and admission-boundary activation with validator discovery. One descriptor algebra serves every seam: `ServiceDescriptor.Describe` and `DescribeKeyed` rows carry registrations, and `TryAddEnumerable` ordered sets carry every fan-in family. The package spine is `Microsoft.Extensions.DependencyInjection` with `Scrutor` scanning and decoration and `FluentValidation.DependencyInjectionExtensions` validator discovery at the root.

## [1]-[INDEX]

| [INDEX] | [CLUSTER]           | [OWNS]                                                            |
| :-----: | :------------------ | :---------------------------------------------------------------- |
|   [1]   | MODULE_TABLE        | Frozen contribution rows; one descriptor algebra for every fan-in seam |
|   [2]   | SCAN_AND_DECORATE   | One-pass scan, decoration, keyed registration fold with receipted freeze |
|   [3]   | BOUNDARY_ACTIVATION | Cached constructor plans and validator discovery at admission boundaries only |

## [2]-[MODULE_TABLE]

- Owner: `ModuleContribution` — the frozen per-process module-table row; modules contribute registrations and never resolve services.
- Auto: `Contributors` rows apply through `TryAddEnumerable` — one ordered descriptor algebra carries every multi-implementation fan-in family.
- Receipt: `ContributionReceipt` — per-module scan, lifetime, keyed, contributor, and registrar counts, materialized at the fold edge.
- Packages: Microsoft.Extensions.DependencyInjection
- Growth: one module row per contributing package, one descriptor row per service; zero new surface.
- Boundary: descriptor construction spells `ServiceDescriptor.Describe` and `DescribeKeyed` only — the `AddSingleton`/`AddScoped`/`AddTransient` and `AddKeyedSingleton`/`AddKeyedScoped`/`AddKeyedTransient` overload families are the deleted spellings.

Row law:
- One composition root per process folds the table; packages ship rows into it. A per-package registration extension, a module interface with configure members, and an event-style registration hook are the deleted patterns — the row is the whole module contract.
- Table order is semantic: a registrar that wraps a sibling module's contract sits in a later row than the contract it wraps, and the fold preserves declaration order end to end.
- `Services` carries unkeyed `Describe` rows; `Keyed` carries `DescribeKeyed` rows whose keys are smart-enum policy values from the owning vocabulary pages; `Contributors` carries the ordered fan-in sets — health, support, drain, and telemetry contributor families register here, never through a bespoke aggregator contract.
- `FromKeyedServicesAttribute` binds keyed constructor parameters, `ServiceKeyAttribute` injects the resolved key into the implementation, and `KeyedService.AnyKey` selects keyed enumerables and never resolves a single service.
- `Registrars` carries collection-shaped package registrations that no descriptor spelling expresses — `Scrutor` decoration rows and the validator-discovery row — each a `Func<IServiceCollection, IServiceCollection>` applied after the module's descriptor rows.
- The `Scan` column is `Option`-typed: a row constructed with `Scan: default` composes through explicit descriptor rows alone. The web and AOT module tables construct every row that way — the same table, zero parallel composition system, and the column flip is the growth proof.

```csharp signature
public sealed record ModuleContribution(
    string Module,
    Assembly Assembly,
    Option<Action<IImplementationTypeSelector>> Scan,
    Seq<ServiceDescriptor> Services,
    Seq<ServiceDescriptor> Keyed,
    Seq<ServiceDescriptor> Contributors,
    Seq<Func<IServiceCollection, IServiceCollection>> Registrars);

public readonly record struct ContributionReceipt(
    string Module,
    int Scanned,
    int Singletons,
    int Scoped,
    int Transients,
    int Keyed,
    int Contributors,
    int Registrars);
```

Module keys are `nameof`-derived assembly symbols, never free literals; the receipt's `Module` field repeats the row key so receipt streams group by module without positional reconstruction.

## [3]-[SCAN_AND_DECORATE]

- Owner: `CompositionSurface` — one fold composes scan, descriptor admission, decoration, and freeze in one pass over the table.
- Entry: `Fin<Seq<ContributionReceipt>> Compose(params ReadOnlySpan<ModuleContribution> modules)` — `Fin` aborts on the first rejected module with module provenance in the failure.
- Auto: `MakeReadOnly` freezes the collection after the fold; `BuildServiceProvider` under `ServiceProviderOptions` with `ValidateOnBuild` and `ValidateScopes` proves the frozen graph on the test row.
- Packages: Scrutor, Microsoft.Extensions.DependencyInjection
- Growth: one scan filter row or one registrar row per cross-cutting concern; zero new surface — the fold absorbs it.
- Boundary: `Applied` is the composition-root boundary capsule — `Scrutor` scan, descriptor admission, and registrar application are host-owned statement seams, and the statement carve-out names this fence.

Pass law:
- Scan sources are `FromAssemblies` over the row's explicit `Assembly`. `FromApplicationDependencies` and `FromDependencyContext` walk the default dependency closure and are the deleted sources: plugin load contexts never appear in that closure, so closure-walking scans silently miss every plugin assembly.
- Selection composes `AddClasses`, then `AssignableTo`, `WithAttribute`, and `InNamespaces` filters, then mapping: `UsingAttributes` maps `ServiceDescriptorAttribute`-annotated classes, `AsImplementedInterfaces` and `AsSelfWithInterfaces` map the rest, and `WithLifetime` plus `WithServiceKey` bind lifetime and key inside the same pass.
- Duplicate registrations resolve under the throw `RegistrationStrategy`; the thrown rejection captures into the rail as conflict evidence carrying the module key — never a silent append, never a silent replace. `RegistrationStrategy.Replace` survives only as an explicit row-level policy on a row that names the contract it overrides.
- `Decorate` and `TryDecorate` registrar rows wrap contributor ports with telemetry and receipt decoration; the decorated contract stays the public contract, and `TryDecorate` is the spelling on rows whose target contract is profile-conditional.
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
        module.Contributors.Iter(services.TryAddEnumerable);
        ignore(module.Registrars.Fold(services, static (current, registrar) => registrar(current)));
        return new ContributionReceipt(
            Module: module.Module,
            Scanned: scanned,
            Singletons: Lifetimes(module, ServiceLifetime.Singleton),
            Scoped: Lifetimes(module, ServiceLifetime.Scoped),
            Transients: Lifetimes(module, ServiceLifetime.Transient),
            Keyed: module.Keyed.Count,
            Contributors: module.Contributors.Count,
            Registrars: module.Registrars.Count);
    }

    private static int Lifetimes(ModuleContribution module, ServiceLifetime lifetime) =>
        (module.Services + module.Keyed).Filter(row => row.Lifetime == lifetime).Count;
}
```

The fold is the only writer of the collection: scan first inside each module so the scanned count derives from the collection delta, descriptor rows next, registrars last so decoration always finds its target contract within the module or in an earlier row.

## [4]-[BOUNDARY_ACTIVATION]

- Owner: `BoundaryActivation` — admission-edge activation and validator discovery over the frozen graph.
- Entry: `Fin<T> Activate<T>(params object[] dependencies)` — empty arity resolves the registered contract, supplied arity invokes the cached constructor plan.
- Packages: Microsoft.Extensions.DependencyInjection, FluentValidation.DependencyInjectionExtensions
- Growth: one validator assembly row per discovering package, one cached plan per boundary-constructed type; zero new surface.
- Boundary: activation sits at admission boundaries only — interior code receives constructor dependencies and frozen policy records, and a provider lookup inside domain flow is the deleted service-location pattern.

Activation law:
- Empty arity routes through `GetServiceOrCreateInstance` — registered contract first, constructed instance second — so optional host contracts admit without a parallel probe entrypoint.
- Supplied arity routes through the `ActivatorUtilities.CreateFactory` plan cached per boundary type; the plan's argument vector derives from the first admission, so a boundary-constructed type owns exactly one explicit-dependency shape, and a second shape for the same type is a row on a new type, never an overload.
- Every activation failure converts at this seam: the capture funnel projects construction rejections into the rail with the target type name, and no raw activation exception crosses inward.
- `AddValidatorsFromAssemblies` discovers validators with an explicit `ServiceLifetime` and a deterministic `AssemblyScanner.AssemblyScanResult` filter; `includeInternalTypes` stays `false`, so public validators are the admitted set. The produced delegate enters the module table as one `Registrars` row — validator discovery owns no second registration path.

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
    }

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

## [5]-[RESEARCH]

| [INDEX] | [ITEM]                                                                    | [PROOF]                                                                                          | [GATE]              |
| :-----: | :------------------------------------------------------------------------ | :------------------------------------------------------------------------------------------------ | :------------------ |
|   [1]   | Scrutor decoration behavior over `DescribeKeyed` keyed descriptors        | `uv run python -m tools.assay test run` over a composition spec decorating a keyed contributor row under `ValidateOnBuild` | SCAN_AND_DECORATE   |
|   [2]   | Selector spelling binding the throw `RegistrationStrategy` inside one `Scan` pass | `uv run python -m tools.assay api query scrutor IServiceTypeSelector`                              | SCAN_AND_DECORATE   |
|   [3]   | `ObjectFactory` delegate shape returned by `ActivatorUtilities.CreateFactory` | `uv run python -m tools.assay api query dependencyinjection ActivatorUtilities`                    | BOUNDARY_ACTIVATION |
