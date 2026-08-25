# [APPHOST_COMPOSITION_AND_MODULES]

One composition root per process folds a frozen module table into the service graph, arms every seam the corpus declares must bind, and freezes it. Composition owns four axes: the `ModuleContribution` row — assembly, scan, slot-keyed descriptor carrier, registrar, and decoration columns — the one-pass receipted composition fold whose single ordinal pass over `DescriptorSlot.Items` carries every admission and every apply, admission-boundary activation carrying availability probing, async-scope ownership, keyed decoration introspection, and validator discovery, and the MUST-BIND ledger whose module folds and root seams are every declared owner's one call site. One descriptor algebra serves every seam: `DescriptorSlot` rows name the admitting member and the admission their descriptors cross first, so registration, keyed registration, idempotent defaults, and ordered fan-in sets are four rows of one roster rather than four columns and four counts. The package spine is `Microsoft.Extensions.DependencyInjection` with `Scrutor` scanning and decoration, `FluentValidation.DependencyInjectionExtensions` validator discovery at the root, and `System.CommandLine` as the app-root verb boundary — one `ParseResult`-driven projection onto the existing owners, never a second dispatcher.

## [01]-[INDEX]

- [02]-[MODULE_TABLE]: Frozen contribution rows over one slot roster carrying every descriptor admission.
- [03]-[SCAN_AND_DECORATE]: One-pass scan, slot fold, and decoration with receipted freeze.
- [04]-[BOUNDARY_ACTIVATION]: Activation plans, availability probes, async scopes, keyed decoration, and validators.
- [05]-[COMMAND_SURFACE]: `System.CommandLine` verb table — seed DATA projecting `ParseResult` onto existing owners.
- [06]-[MODULE_LEDGER]: Module folds, the must-bind seam roster, and the two-altitude fold that is every row's one call site.

## [02]-[MODULE_TABLE]

- Owner: `DescriptorSlot` `[SmartEnum<string>]` — the descriptor algebra as ROW DATA, each row carrying the admission its descriptors cross and the collection member that admits them; `ModuleContribution` — the frozen per-process module-table row; modules contribute registrations and never resolve services.
- Cases: `Service` admits unkeyed descriptors and `Keyed` keyed ones — each row enforces its own regime against the descriptor's `IsKeyedService` — `Default` adds idempotently across both regimes, and `Contributor` joins the ordered fan-in set behind the port-cardinality admission.
- Auto: the composition fold walks `DescriptorSlot.Items` in `Rank` order, so slot ordering is the roster's own column and no fold body names a slot.
- Receipt: `ContributionReceipt` — module key, scan delta, registrar and decoration counts, the APPLIED per-slot tally (`Slots`, each slot's collection delta across its own apply, so an idempotent `TryAdd` that landed nothing counts nothing), and the DECLARED lifetime partition (`Lifetimes`, the mix the module authored) — the two questions named apart rather than folded under one derivation claim.
- Packages: Microsoft.Extensions.DependencyInjection, Thinktecture.Runtime.Extensions
- Growth: a new admission regime is ONE `DescriptorSlot` row carrying its `Admits` and `Admit` columns — the fold, the receipt, and every module row are untouched; one module row per contributing package, one descriptor row per service; zero new surface.
- Boundary: descriptor construction spells `ServiceDescriptor.Describe` and `DescribeKeyed` only — the `AddSingleton`/`AddScoped`/`AddTransient` and `AddKeyedSingleton`/`AddKeyedScoped`/`AddKeyedTransient` overload families are the deleted spellings; NAMED LOSS — the named-property receipt read. `receipt.Singletons` and `receipt.Contributors` become `receipt.Lifetimes[ServiceLifetime.Singleton]` and `receipt.Slots[DescriptorSlot.Contributor]`, so a reader keys instead of dotting. What that buys is the ordinal fold: four `Seq<ServiceDescriptor>` columns applied by four hand-written statements, guarded by one hand-written admission, and counted by six stored ints were one concept spelled sixteen times, and a fifth admission regime edited every one of them; it now lands as one roster row while the fold, the carrier, and the receipt hold still. No stored count survives that a fold already reconstructs.

Row law:
- One composition root per process folds the table; packages ship rows into it. A per-package registration extension, a module interface with configure members, and an event-style registration hook are the deleted patterns — the row is the whole module contract.
- Table order is semantic: a registrar that wraps a sibling module's contract sits in a later row than the contract it wraps, and the fold preserves declaration order end to end.
- `Descriptors` is the ONE slot-keyed carrier: `Service` holds unkeyed `Describe` rows, `Keyed` holds `DescribeKeyed` rows whose keys are smart-enum policy values from the owning vocabulary pages, `Contributor` holds the ordered fan-in sets — health, support, drain, and telemetry contributor families register there, never through a bespoke aggregator contract. A slot a module never fills reads empty rather than absent, so the fold is total over the roster and a module omits a slot by saying nothing.
- `Default` is the additive-only floor: a package-shipped default whose contract a host or later module may pre-empt applies through the package's own `TryAdd`, which compares `(ServiceType, ServiceKey)` and reads no implementation type, so the keyed and unkeyed arms are ONE member and the deleted lifetime switch was both redundant and narrower — it dereferenced `KeyedImplementationType`, which is null on every keyed factory and keyed instance descriptor this law admits. A default that must override an earlier registration is a `Service` row.
- `FromKeyedServicesAttribute` binds keyed constructor parameters, `ServiceKeyAttribute` injects the resolved key into the implementation, and `KeyedService.AnyKey` selects keyed enumerables and never resolves a single service.
- `Registrars` carries collection-shaped package registrations that no descriptor spelling expresses — the validator-discovery row and other collection-shaped admissions — each a `Func<IServiceCollection, IServiceCollection>` applied after the module's descriptor rows.
- `Decorations` carries the typed decoration column: each entry is one `DecorationRow` application naming the inner service contract and the wrapping decorator, so the decoration topology is data the fold reads and the receipt counts, never an opaque registrar `Func`. A profile that drops a contributor port carries the entry with `Conditional: true`, so the same column decorates on the service profile and skips on the plugin profile by `TryDecorate` row presence.
- The `Scan` column is `Option`-typed: a row constructed with `Scan: default` composes through explicit descriptor rows alone. The web and AOT module tables construct every row that way — the same table, zero parallel composition system, and the column flip is the growth proof.

```csharp signature
// --- [TABLES] -------------------------------------------------------------------------------
// The descriptor algebra as DATA. Each row names the admission its descriptors cross and the collection
// member that takes them, so the composition fold is one ordinal pass and a fifth regime is one row here.
// `Service` and `Keyed` share an apply member and stay apart on their AUTHORING regime — a keyed row's key is
// a smart-enum policy value some vocabulary page owns — and the discriminant is ENFORCED rather than asserted:
// each row admits only descriptors whose own `IsKeyedService` matches its regime, so a keyed descriptor
// authored into the unkeyed slot (or the reverse) refuses by service-type name while the collection is still
// editable, and the two rows are no longer behavioural twins wearing different keys.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class DescriptorSlot {
    public static readonly DescriptorSlot Service = new("service", rank: 0,
        admits: static row => Slotted(row, keyed: false), admit: static services => services.Add);
    public static readonly DescriptorSlot Keyed = new("keyed", rank: 1,
        admits: static row => Slotted(row, keyed: true), admit: static services => services.Add);
    // `Default` is the ONE row spanning both regimes by construction: `TryAdd` compares `(ServiceType,
    // ServiceKey)` and takes keyed and unkeyed descriptors alike, so a regime test here would refuse a
    // descriptor the apply member admits.
    public static readonly DescriptorSlot Default = new("default", rank: 2,
        admits: static _ => Validation<Error, Unit>.Success(unit), admit: static services => services.TryAdd);
    // The `Runtime/ports#PORT_RECORDS` eighth-port refusal rides HERE as the contributor row's own admission:
    // every fan-in descriptor resolves a cardinality row by service-type name while the collection is still
    // editable, so the mandate and its enforcement are one declaration rather than a fold-body special case.
    public static readonly DescriptorSlot Contributor = new("contributor", rank: 3,
        admits: static row => PortCardinality.Of(row.ServiceType.Name).ToValidation().Map(static _ => unit),
        admit: static services => services.TryAddEnumerable);

    public int Rank { get; }

    [UseDelegateFromConstructor]
    public partial Validation<Error, Unit> Admits(ServiceDescriptor row);

    [UseDelegateFromConstructor]
    public partial Action<ServiceDescriptor> Admit(IServiceCollection services);

    // The regime test PARAMETERIZED, so the two rows differ in one boolean rather than in two bodies and the
    // refusal names the service type an authoring mistake put in the wrong slot.
    static Validation<Error, Unit> Slotted(ServiceDescriptor row, bool keyed) =>
        row.IsKeyedService == keyed
            ? Validation<Error, Unit>.Success(unit)
            : new KernelFault.InvalidValue(
                Label: row.ServiceType.Name,
                Requirement: keyed ? "<a keyed descriptor>" : "<an unkeyed descriptor>");
}

// --- [MODELS] -------------------------------------------------------------------------------
public sealed record ModuleContribution(
    string Module,
    Assembly Assembly,
    Option<Action<IImplementationTypeSelector>> Scan,
    HashMap<DescriptorSlot, Seq<ServiceDescriptor>> Descriptors,
    Seq<Func<IServiceCollection, IServiceCollection>> Registrars,
    Seq<DecorationRow> Decorations) {
    // Total over the roster: an unfilled slot reads empty, so the fold never asks whether a module declared one.
    public Seq<ServiceDescriptor> this[DescriptorSlot slot] => Descriptors.Find(slot).IfNone(Seq<ServiceDescriptor>());

    public Seq<ServiceDescriptor> Rows => toSeq(DescriptorSlot.Items).Bind(slot => this[slot]);
}

public readonly record struct DecorationRow(Type Service, Type Decorator, bool Conditional);

// Three counts, and the receipt states WHICH question each answers rather than reading as one derivation.
// `Scanned` and `Slots` are ADMITTED censuses read off the collection itself — the scan delta and the
// per-slot apply delta — because `TryAdd` and `TryAddEnumerable` are idempotent and a declared row count
// therefore answers what a module asked for, not what the graph took. `Lifetimes` partitions the DECLARED
// carrier and says so: it is the lifetime mix the module authored, the one question the collection delta
// cannot answer, since a re-declared row that landed nothing still carries the lifetime it asked for.
public readonly record struct ContributionReceipt(
    string Module,
    int Scanned,
    int Registrars,
    int Decorated,
    HashMap<DescriptorSlot, int> Slots,
    HashMap<ServiceLifetime, int> Lifetimes);
```

Module keys are `nameof`-derived assembly symbols, never free literals; the receipt's `Module` field repeats the row key so receipt streams group by module without positional reconstruction.

## [03]-[SCAN_AND_DECORATE]

- Owner: `CompositionSurface` — one fold composes scan, the ordinal slot pass carrying every descriptor admission, decoration, and freeze in one pass over the table.
- Entry: `Fin<Seq<ContributionReceipt>> Compose(params ReadOnlySpan<ModuleContribution> modules)` — `Fin` aborts on the first rejected module with module provenance in the failure, whether the rejection was thrown by the scan or railed by an admission.
- Auto: `MakeReadOnly` freezes the collection after the fold; `BuildServiceProvider` under `ServiceProviderOptions` with `ValidateOnBuild` and `ValidateScopes` proves the frozen graph on the test row.
- Packages: Scrutor, Microsoft.Extensions.DependencyInjection
- Growth: one scan filter row or one registrar row per cross-cutting concern; zero new surface — the fold absorbs it.
- Boundary: `Applied` is the composition-root boundary capsule — `Scrutor` scan, descriptor admission, and registrar application are host-owned statement seams, and the statement carve-out names this fence; the `Runtime/ports#PORT_RECORDS` eighth-port refusal EXECUTES through the `Contributor` row's `Admits` column and nowhere else, so a contributor descriptor naming no cardinality row refuses while the collection is still editable rather than surfacing later as a leaked inward dependency.

Pass law:
- Scan sources are `FromAssemblies` over the row's explicit `Assembly`. `FromApplicationDependencies` and `FromDependencyContext` walk the default dependency closure and are the deleted sources: plugin load contexts never appear in that closure, so closure-walking scans silently miss every plugin assembly.
- Selection composes `AddClasses`, then `AssignableTo`, `WithAttribute`, and `InNamespaces` filters, then mapping: `UsingAttributes` maps `ServiceDescriptorAttribute`-annotated classes, `AsImplementedInterfaces` and `AsSelfWithInterfaces` map the rest, and `WithLifetime` and `WithServiceKey` bind lifetime and key inside the same pass.
- Duplicate registrations resolve under `UsingRegistrationStrategy(RegistrationStrategy.Throw)` bound inside the same `Scan` pass; the thrown rejection captures into the rail as conflict evidence carrying the module key — never a silent append, never a silent replace. `RegistrationStrategy.Replace` survives only as an explicit row-level policy on a row that names the contract it overrides.
- Descriptors apply through ONE ordinal pass over `DescriptorSlot.Items`: each slot accumulates its own `Admits` over its rows and then hands them to the member its `Admit` column names, so the whole descriptor stage is one expression whatever the roster holds. The pass ACCUMULATES within a slot and across the roster alike, so a module adding three foreign contributor ports beside a mis-slotted keyed row names all four on one boot rather than one per attempt; each slot's apply answers its own collection delta, which is the count the receipt carries.
- The `Decorations` column applies before registrars through `BoundaryActivation.Decorate`, wrapping contributor ports with telemetry and receipt decoration; the decorated contract stays the public contract, and the `Conditional` flag selects `TryDecorate` on a profile-conditional target. A `Conditional: false` entry whose target reports undecorated refuses on the rail — a count alone leaves the composition defect for a reader to notice — and the surviving count is the receipt's `Decorated` column. Decoration owns this cluster's keyed-decoration pass-law; `BOUNDARY_ACTIVATION` owns the decoration introspection.
- Registration is bootstrap-only: after `MakeReadOnly`, descriptor mutation throws, so every late registration attempt surfaces at the root instead of drifting into runtime state.

```csharp signature
public static class CompositionSurface {
    extension(ServiceCollection services) {
        public Fin<Seq<ContributionReceipt>> Compose(params ReadOnlySpan<ModuleContribution> modules) =>
            Iterable<ModuleContribution>.FromSpan(modules)
                // The self-flattening bind collapses the capture rail into the module's own admission rail, so
                // a thrown scan conflict and a railed cardinality refusal both leave carrying the module key.
                .TraverseM(module => Op.Of().Catch(() => Fin.Succ(Applied(services, module)))
                    .Bind(static admitted => admitted)
                    .MapFail(error => (Error)new LifecycleFault.ModuleRejected(module.Module, error)))
                .As()
                .Map(receipts => (fun(services.MakeReadOnly)(), receipts.ToSeq()).Item2);
    }

    private static Fin<ContributionReceipt> Applied(IServiceCollection services, ModuleContribution module) {
        int admitted = services.Count;
        module.Scan.IfSome(select => services.Scan(source => select(source.FromAssemblies(module.Assembly))));
        int scanned = services.Count - admitted;
        return toSeq(DescriptorSlot.Items)
            .OrderBy(static slot => slot.Rank)
            .ToSeq()
            // ACCUMULATES across slots as well as within one: a module whose keyed rows and whose contributor
            // rows both carry a refusal names both on one boot, so the slot pass answers the whole module's
            // admission rather than whichever slot the rank order reached first.
            .Traverse(slot => Seated(services, slot, module[slot])
                .Map(landed => (Slot: slot, Landed: landed))
                .ToValidation())
            .As()
            .ToFin()
            .Bind(landed => {
                module.Decorations.Iter(decoration => BoundaryActivation.Decorate(services, decoration));
                ignore(module.Registrars.Fold(services, static (current, registrar) => registrar(current)));
                return Decorated(services, module.Decorations)
                    .Map(decorated => (Landed: landed, Decorated: decorated));
            })
            .Map(applied => new ContributionReceipt(
                Module: module.Module,
                Scanned: scanned,
                Registrars: module.Registrars.Count,
                Decorated: applied.Decorated,
                Slots: applied.Landed.ToHashMap(static row => row.Slot, static row => row.Landed),
                Lifetimes: Lifetimes(module)));
    }

    // One slot, one shape: accumulate the row's own admission, apply through the member the row names, and
    // answer what the COLLECTION took. A fifth admission regime lands as one roster row and this body never moves.
    private static Fin<int> Seated(IServiceCollection services, DescriptorSlot slot, Seq<ServiceDescriptor> rows) =>
        rows.Traverse(slot.Admits)
            .As()
            .ToFin()
            .Map(_ => Landed(services, rows, slot.Admit(services)));

    // The APPLIED count, never the declared one: `TryAdd` and `TryAddEnumerable` compare before they add, so a
    // module re-declaring a default a host already seated asks for a row and lands none — the collection delta
    // across the apply is the only number the receipt can defend.
    private static int Landed(IServiceCollection services, Seq<ServiceDescriptor> rows, Action<ServiceDescriptor> admit) =>
        services.Count is var before && rows.Iter(admit) is var _
            ? services.Count - before
            : 0;

    // The decoration proof: an unconditional row whose contract the frozen collection does not wrap is a
    // composition defect, so the pass yields a refusal and the confirmed count in one fold rather than a
    // number a reader compares against the declaration by hand.
    private static Fin<int> Decorated(IServiceCollection services, Seq<DecorationRow> rows) =>
        rows.Traverse(row => row.Conditional || services.IsDecorated(row.Service)
                ? Validation<Error, Unit>.Success(unit)
                : new KernelFault.InvalidValue(Label: row.Service.Name, Requirement: "<a decorated contract>"))
            .As()
            .Map(_ => rows.Filter(row => services.IsDecorated(row.Service)).Count)
            .ToFin();

    // Lifetime partitions read the WHOLE folded carrier rather than three named columns, so a lifetime the
    // contributor slot carries is counted where the prior three-column fold silently dropped it. This one IS
    // the declared mix by design — the question is what the module authored, which the collection delta the
    // slot tally reads cannot answer.
    private static HashMap<ServiceLifetime, int> Lifetimes(ModuleContribution module) =>
        module.Rows.Fold(
            HashMap<ServiceLifetime, int>(),
            static (tally, row) => tally.AddOrUpdate(row.Lifetime, static held => held + 1, 1));
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
            Op.Of().Catch(() => Fin.Succ(dependencies.Length == 0
                ? ActivatorUtilities.GetServiceOrCreateInstance<T>(provider)
                : (T)Plans.GetOrAdd(
                        typeof(T),
                        static (_, supplied) => ActivatorUtilities.CreateFactory(
                            typeof(T),
                            [.. supplied.Select(static value => value.GetType())]),
                        dependencies)
                    .Invoke(provider, dependencies)!))
                .MapFail(error => (Error)new LifecycleFault.ActivationRejected(typeof(T).Name, error));

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

## [05]-[COMMAND_SURFACE]

- Owner: `VerbRow` the seed-DATA verb table row; `AppRootVerbs` the one CLI boundary adapter mounting the table onto a `RootCommand`.
- Cases: canonical rows — `dispatch` projects a descriptor + serialized arguments onto `Agent/runtime#DISPATCH_FRONT_DOOR` `CommandDispatch.Run`; `replay` and `bisect` are the `Runtime/determinism` ingress (the `ChangefeedPort.Load` windowed read feeding `ReplayVerify.Replay`/`AdversarialProbe.Bisect`); `capture-support` admits one `SupportTrigger.ExternalCommand` onto the `Observability/bundles` capture fan; `sandbox-release` projects onto `Sandbox/isolation#QUOTA_CONTROL` `QuotaControl.Release`, the one path a quarantined plugin takes back into service.
- Entry: `Mount(string description, Seq<VerbRow> rows)` returns `RootCommand` — the table mounts once at the app root; each row's `Command.SetAction(Func<ParseResult, CancellationToken, Task<int>>)` binds the projection; `ParseResult.GetValue<T>(Option<T>)`/`GetValue<T>(Argument<T>)` are the only argument reads.
- Packages: System.CommandLine, LanguageExt.Core, BCL inbox
- Growth: a new operator verb is one `VerbRow` in the table projecting onto an existing owner; a verb whose owner does not exist yet is a missing case on the owning page, never a CLI-local body; zero new surface.
- Boundary: the verb table is a BOUNDARY ADAPTER — every row's body is one projection into a composed owner (`CommandDispatch.Run`, the determinism port, the capture trigger) and a verb carrying domain logic of its own is the deleted form; `AppRootVerbs.Mount` is the named boundary capsule for the statement carve-out (the `RootCommand` mutation seam); a rejected parse never reaches a row's action because a non-empty `ParseResult.Errors` blocks invocation by the package's own contract, so parse failure is DATA the host entry projects to an exit code and a thrown parse has no spelling here; `Exit` is the ONE exit projection every row leaves through and the status it answers is BINARY — a POSIX wait status keeps the low eight bits of what a process returns, so a banded fault code CANNOT be one: `FaultBand.Config.Code(offset: 0)` is 4100, `4100 & 0xFF` is 4 — a number naming no verdict — and any code ≡ 0 mod 256 reports SUCCESS outright to every shell, supervisor, and CI gate that reads it, which is why `(int)error.Code` as a status is the deleted form — the STATUS carries the verdict (0 admitted, 1 refused); the STREAM renders a typed fault's band code and message, an uncoded foreign error's message, or the located refusal columns; these remain local CLI projections and do not imply a ControlService RPC.

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

    // The ONE exit projection every row leaves through. The status is BINARY by construction: a POSIX wait
    // status keeps the low eight bits of what a process returns, so a banded fault code cannot BE one —
    // `FaultBand.Config.Code(offset: 0)` is 4100, truncated to 4, and any code ≡ 0 mod 256 reports a refusal
    // as SUCCESS to every shell and supervisor reading it. The status answers admitted-or-refused; the stream renders a
    // typed fault's band code and message, an uncoded foreign error's message, or the located refusal columns.
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

    // The bisect row rides the SAME windowed read the replay row does — one durable ingress, two probes — so
    // an operator narrowing a divergence never re-records a run to find it. The verdict is the CARRIER: a
    // clean chain answers `None` and a real divergence answers `Some`, so sequence zero is an ordinary finding
    // and the retired genesis-pair sentinel — a legal shape a divergence at sequence zero produces — is gone
    // with the hash comparison that read it. The located `Divergence` RENDERS: a status alone tells an operator
    // a chain diverged and nothing about where, which is the whole answer the narrowing computed.
    public static VerbRow Bisect(ChangefeedPort port, Func<LogEntry, ChainHash> rederive) {
        var origin = new Option<Guid>("--origin");
        var from = new Option<long>("--from");
        var to = new Option<long>("--to");
        var command = new Command("bisect", "binary-search a recorded chain for its first divergent step") { origin, from, to };
        return new(command, (parse, _) => Task.FromResult(Exit(
            port.Load(new ChangefeedWindow(parse.GetValue(origin), parse.GetValue(from), parse.GetValue(to)))
                .Map(log => AdversarialProbe.Bisect(log, rederive).Map(Located)))));
    }

    // The divergence's own typed columns, tab-separated for a line-oriented operator read: the sequence the
    // narrowing pinned, the recorded and re-derived hashes that disagreed there, and the steps it took.
    static string Located(Divergence divergence) =>
        string.Create(CultureInfo.InvariantCulture,
            $"{divergence.Sequence}\t{divergence.Recorded.Hex}\t{divergence.Rederived.Hex}\t{divergence.Steps}");

    // The operator's review arm: `QuotaControl.Release` is the ONE path a quarantined plugin takes back into
    // service, so this row projects onto it and re-seats no disposition of its own. The roster it addresses is
    // the SAME cell the epoch pacer sweeps, so an operator and the pacer read one live set and a plugin this
    // host never hosted refuses by name rather than answering a silent no-op.
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

- Owner: `RootBinding` `[Union]` the two-altitude seam row; `RootInputs` the composed values every row reads, with `CoordinationArrows`, `ControlSeed`, `ObservabilitySeed`, `WireSeed`, `AgentSeed`, and `SandboxSeed` the deploy-declared seeds beside it; `CompositionRoot` the static ledger, the measured `CapsulePins` load-context roster, the `Metered` pre-`Arm` instrument mount carrying the declaring-package contributor roster, and the one fold that is each row's call site.
- Cases: `Seated` binds while the collection is still editable; `Proven` runs against the BUILT provider because the fact it needs — a materialized pipeline, a loaded plugin, an issued token — does not exist before the build.
- Entry: `Arm(ServiceCollection services, RootInputs inputs, ServiceProviderOptions options, params ReadOnlySpan<ModuleContribution> modules)` returns `Fin<(IServiceProvider Provider, Seq<ContributionReceipt> Receipts)>` — folds every `Seated` row, folds the module table, freezes, builds, then folds every `Proven` row, so a boot either yields a provider whose declared seams are all armed or names the seam that refused.
- Auto: the ledger is DATA, so a page declaring a new must-bind seam adds one row and no fold body changes; declaration order IS dependency order — `hooks-mount` runs first because every later fold reads the rail it seats, and a fold needing a value an earlier fold produced resolves it inside a factory lambda the fold never runs, so no row reads a graph still being written and no dependency graph is computed to recover an order the roster already states; the root's own seams seat AHEAD of the module table so a module row decorating a platform contract finds it already registered, and `Compose`'s `MakeReadOnly` stays the one freeze — a ledger row after it throws at the append rather than registers; both folds accumulate per row on the rail, so one boot names every unbound seam instead of one per attempt.
- Receipt: `Arm` yields the module `ContributionReceipt` sequence beside the provider; a ledger row mints no receipt of its own — its evidence is the boot that either produced a provider or named the refusing seam.
- Packages: Microsoft.Extensions.DependencyInjection, Polly.Extensions, NuGet.Versioning, LanguageExt.Core, BCL inbox
- Growth: a new must-bind seam is one `RootBinding` row at its altitude; a new module is one `<module>-seat`/`<module>-boot` pair; a new composed value the rows read is one `RootInputs` column, and a value whose halves belong to other owners is one seed record beside it, so an owner's law reaches this fold as a filled column rather than a re-implementation; a package that declares an instrument family is one `Metered` contributor argument; a member measured to pin a collectible load context is one `CapsulePins` row the unload proof already folds; zero new surface.
- Boundary: this ledger is the ONE composition-scoped call site for every seam a page declares and no ordinary consumer reaches, so a declared-and-unbound seam is a missing row rather than a sentence a reader audits against the corpus — the class the ledger exists to close is a `Bind`, `Register`, `Mount`, or `Of` member with a page-long law behind it and zero callers. Runtime-scoped PRODUCERS never appear here and the two altitudes never trade: an instrument write, a dispatched tool, a continued trace binds at its own producing arm, because hoisting one into this fold fires it once at boot where the law wants it per event, and sinking a composition binding into a producing arm re-registers on a frozen collection; the `DescriptorSlot.Contributor` admission stays inside `SCAN_AND_DECORATE`'s per-module fold for the same reason — the module row is the thing that can violate the port invariant, so the admission belongs at the row and not at the root; a row's delegate composes an owner and never re-implements one, so the ledger holds no logic of its own and a body doing work past its composed calls is the deleted form; the `ILatencyContext` factory is the composition's single mint — `DrainConductor.Drain`, `OutboundSurface.Run`, and `SupportCapture.Capture` each take the context as a parameter, so a fold minting its own context, or timing a phase off a `Stopwatch`, is the deleted form; `LatencySpine.Register` owns the NAME table alone while this ledger owns the provider registration, so the option that gates the issuer sets once at this seat; capsule unload is a MEMBERSHIP proof and never a sweep — the roster carries only members a live collectible host measured as pinning, so a blanket dispose-everything row claims a guarantee the runtime does not give while a type proven not to pin (an undisposed `ActivitySource`, an instrument-free `Meter`) buys an unload nothing.

Seat law:
- `hooks-mount` freezes the point census through `HookRegistry.Mount` and decorates `ReceiptSinkPort.Emit` through `AppHostHooks.Tap`, so every stamped envelope crosses the `Receipt` row before egress and every later fold resolves the decorated port. The rail arrives on `RootInputs` already composed because `HookRail.Of` seats its gates and taps AT construction and rolls the whole subscription set back on the first refusal — the taps it mounts project capsules this same fold registers, so a mint at this seat obliges a row to resolve what a later row writes.
- `redaction-and-sampling` reaches the log chain through `SignalGovernance.GovernLogs`, whose `RedactionRegistration.Bind` carries every redactor row — sealing the chain without it leaves the erasing fallback as the ONLY resolution and every classified tag erases, including the operational dimensions the pass rows exist to spare.
- `latency-context` seats the pooled provider, issuer, and the ONE `Func<ILatencyContext>` factory the three threading folds read; `latency-names` folds this root's roster with every contributed `LatencyRoster` into the single registration under `ThrowOnUnregisteredNames`, because an unregistered name resolves to a positionless token whose writes drop with nothing raised.
- `drain-thread` seats the `Func<DrainThread>` MINT rather than an opaque input: each drain opens its own context and token through `LatencySpine.Open` beside the mounted instrument set and the ledger exporter the terminal `Seal` feeds, so every drain-gated rollover receives the conductor's whole telemetry tail from the one root and an unbound tail refuses at boot instead of compiling on a trailing default. The cooperative and forced budgets are NOT on it — the conductor reads its own `DeadlineClass` rows, so a budget travelling beside the fold that owns it is a second value to disagree.
- `drain-rows` seats the late-registration cell: a bus subscription and an epoch lease both open after the build and both must drain, so the conductor folds this cell beside the contributed port fan and a participant that exists only at runtime is still a drain row rather than an orphan.
- `lifecycle` constructs the phase capsule over the composed rail WHOLE — the capsule fires `AppHostPoint.Phase` itself and owns the `DegradationTap` the composition seats — so the one shielded fan-out exists before the capsule that fires it and an observer seating real I/O can never unwind past the transition rail.
- `verdict-fallback` binds `FlagVerdict.Inert` as the whole verdict function so an absent features rail and an unready provider answer ONE shape at every consumer.
- `membership` seats the cluster view over the resolver, the per-authority `UriHealthCheck`, and the three decoded membership arrows; `peer-roster` seats the local attach set whose `contribute` closure is the two-tier edge `Wire/companion` and `Wire/coordination` both declare — the two rows reference each other only inside lambdas the fold never runs, so the mutual read resolves after both are registered, and the peer endpoint projects from the manifest's own `SocketPath` through `UnixDomainSocketEndPoint` so the UDS contract carries one address rather than two encodings obliged to agree.
- `coordination-seat` constructs the ONE `LeaseElection.Runtime` off the coordination seed's four decoded lease arrows, because those decoded arrows are its only producer and no fold below this one reaches them; the same row FORCES `LeasePolicy.Outlasts`, since this is where the reclamation window meets the drain bounds it must outlast and a proof no reader forces guarantees nothing.
- `control-inbound` completes `ControlRuntime` with the drain arrow over the degradation, support, source, and wire values in `ControlSeed`.
- `design-regime` is the seat-law row this doctrine binds on any PRODUCT root that composes `Rasm.Bim` — it lives on that root's own ledger, never here, because this package references the kernel alone and a Bim type cannot appear in this fence, exactly as the `BrickBinding` class election rides the composing root. The root elects the project's national design regime ONCE: `StageLabels.Nation` (the typed `Option<ICountry>` off the compiled `IGovernance.Country` pin, `Rasm.Bim/Planning/schedule#SCHEDULE`) feeds `AnnexRegime.Of(ICountry)` (`Rasm.Bim/Model/eurocode#EUROCODE_ALGEBRA` — the ISO-keyed nation→annex bridge whose row KEY is the SAF `ExcelNationalCode`) into the `EurocodePolicy` the root constructs, and the SAME `Option<AnnexRegime>` threads to `SafEmit.Export` (`Rasm.Bim/Exchange/export#SAF_EMIT`). Both parameters are REQUIRED and undefaulted at their Bim owners, so an unelected root breaks loudly at compile rather than silently designing under `Recommended` or writing no design code cell; a second election beside the export call, or a free country string standing in for the typed nation, forks the national annex the eurocode tables and the SAF workbook must share.
- `bim-compute-tessellation` is likewise a PRODUCT-root module row, never an AppHost project reference: the root that references both packages binds Bim's `ITessellationCompanion` directly to one `BimComputeCompanion`. The outer app call supplies its existing `CorrelationId` to `TessellationRequest.Resolve`; the adapter passes it to the Compute-owned singleton `CallSpineFactory`, which mints one spine for source Put, Tessellate, and output Fetch. The adapter frames and puts the IFC source, projects that admitted `ArtifactRef` through `TessellationWire.Project`, drives `CompanionEdge`, proves the peer's reported content key and projects its count, semantic, and generated spill fields through `TessellationWire.Admit`, and returns one `TessellationCross` on the asynchronous port. The module seats `CallSpineFactory` itself; `ClockPolicy`, `WireServices`, `StreamPool`, and `ReceiptSurface` are one-per-composition singleton dependencies already seated by that root's Compute module. `ValidateOnBuild` and `ValidateScopes` prove the complete singleton constructor graph and refuse any missing or shorter-lived dependency. No `IServiceProvider` crosses the adapter, no blocking wait collapses `IO`, no correlation is derived from `Op` or captured at composition, no `AdmittedIntent` is fabricated for a call outside the compute-dispatch algebra, and no second request, frame, semantic, spill, or fault shape exists.
- `AgentSeed.Leases` is the ONE bearer holder both the `membership` probe and the `wire-seat` HTTP lane dereference, each at its own moment — per probe and per send — so a lease that `agent-boot` armed and its own occurrence later renewed reaches every hop with no re-registration at either seat and no held copy anywhere to go stale; `WireSeed.Credentials` is what tells them WHICH registration answers for a dialed authority, and an authority carrying no row is anonymous by declaration.
- Every `<module>-seat` row registers what its owners declare while the collection is editable; every `<module>-boot` row runs the gates whose facts exist only after the build. A gate whose refusal must stop the process rails there, so a refused trust anchor, an unhosted solver, an unrebuilt member set, or a peer surface the registry never took names itself at boot rather than at first call.

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
    // The resolved posture and the boot identity every seam stamps, the UNDECORATED receipt port, the folder's
    // operation key, and the configuration the lane rows read. The rail is a composed VALUE rather than a
    // resolved service because the capsule that fires its phase point and the fan that taps its receipt point
    // are both constructed by rows in this same fold, and `HookRail.Of` seats every subscription at
    // construction and rolls the whole set back on the first refusal.
    ConsumptionProfile Profile,
    ClockPolicy Clocks,
    CorrelationId Correlation,
    DeploymentTopology Topology,
    ReceiptSinkPort Sink,
    Op Key,
    IConfiguration Configuration,
    // This node's own cluster identity — the `Services` config key the endpoint resolver keys on and the
    // membership group its durable rows live under. Both are deploy-declared values no service can produce,
    // which is why they seat here rather than resolving; a companion attaching over the control hop
    // contributes under THIS role, because it is a local process of this role and not a peer of its own.
    RoleName Role,
    string Group,
    CoordinationArrows Coordination,
    ControlSeed Control,
    ObservabilitySeed Observability,
    WireSeed Wire,
    AgentSeed Agent,
    SandboxSeed Sandbox,
    HookRail<AppHostPoint, AppHostFact, TelemetrySource> Rail);

// The WHOLE Persistence coordination port, decoded to WIRE-STABLE PRIMITIVES at this root: the membership half
// — group and member cross as their string keys and the scan answers keys beside deadlines — and the lease
// half, the four store arrows `Runtime/time#FENCING_TOKEN` `LeaseElection.Runtime` binds, whose issued
// generation crosses as the bare `ulong` the adapter decodes into `FencingToken`. Both halves sit on ONE record
// because they are one port; a second seed for the lease arrows would let a deployment wire membership against
// one store and fencing against another. Every column is a `Func<>` because every column is a PER-CALL store
// effect no service produces, and no store record crosses upward into the cluster view.
public sealed record CoordinationArrows(
    Func<string, string, Duration, Fin<Unit>> MemberUpsert,
    Func<string, string, Fin<Unit>> MemberRelease,
    Func<string, Fin<Seq<(string Member, Instant Until)>>> MemberScan,
    Func<string, LeasePolicy, Fin<(ulong Generation, Instant Deadline)>> AcquireLease,
    Func<string, LeasePolicy, ulong, Fin<(ulong Generation, Instant Deadline)>> RenewLease,
    Func<string, ulong, Fin<Unit>> GuardWrite,
    Func<string, ulong, Fin<Unit>> ReleaseLease);

// The half of `ControlRuntime` this ledger does not own: the degradation, support, source, and wire values.
public sealed record ControlSeed(
    DegradationCell Degradation,
    ActivitySource Source,
    SupportRuntime Support,
    JsonSerializerOptions Wire);

// The four module seeds, the same shape `ControlSeed` is: deploy-declared values no service produces and no
// fold derives — WHICH probe rows this deployment binds, WHICH hops it dials, WHICH categories it hosts,
// WHICH manifests it declares — beside the projections their owning pages fill. The contributed port rosters
// ride here for the reason the module table does: the root's own seams seat AHEAD of the table, so a
// contributed row resolved from the graph would not yet exist at the seat that folds it.
public sealed record ObservabilitySeed(
    // The mounted receipt fan, minted through `CompositionRoot.Metered` BEFORE this fold runs, because
    // `InstrumentFan.Tap` is a subscription `HookRail.Of` seats at construction and the rail arrives on
    // `RootInputs` already composed — so the fan is the same kind of pre-`Arm` composed value the rail is, and
    // this seat REGISTERS it rather than minting a second one whose meters would double every stream.
    ReceiptFan Fan,
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
    // Which authority each lease authenticates hops to — a deploy fact no service produces, because only the
    // deployment knows that this object store answers to that registration. An authority absent from the map
    // dials ANONYMOUS: the empty map is the honest spelling of an unauthenticated estate, where a per-lane
    // credential flag would need a second column stating whether the flag was ever set.
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
    // The BOOT-DECLARED credential cells, not lease values: `agent-boot` arms their occurrences and every
    // later reader — the membership probe, the HTTP lane's per-send link — dereferences this one roster, so a
    // refreshed lease is visible at every consumer with no second registration and no held copy to go stale.
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
    // The plugin registry as a LISTING and the version this host already runs — never a resolver's own pick.
    // Ranking is `Sandbox/admission#SUPPLY_CHAIN_GATE` `SupplyChainGate.Best`'s policy over the contract range,
    // so a registry handing back the version it liked is the second policy for one contract that gate deletes.
    Func<SolverManifest, Fin<Seq<(NuGetVersion Version, PluginArtifact Artifact)>>> Catalog,
    Func<string, Option<NuGetVersion>> Installed,
    Func<Seq<CapabilityDescriptor>, IO<Seq<DescriptorReceipt>>> Project,
    GrantScope Scope,
    UpdateChannel Channel,
    FleetRuntime Fleet);

public static class CompositionRoot {
    // The must-bind roster in DEPENDENCY ORDER. Every entry is one composed call, so the row names the seam
    // and the delegate is its whole body — a row growing logic of its own is the composition root absorbing a
    // page's law.
    public static readonly Seq<RootBinding> Ledger =
    [
        // FIRST, and the rail is why: the census a module row fires against must be frozen and the receipt
        // port decorated before any fold registers a producer, so every later row resolves ONE rail, one
        // fault cell, and one decorated sink rather than each composing its own.
        new RootBinding.Seated("hooks-mount", static (services, inputs) =>
            HookRegistry.Mount([.. inputs.Rail.Points]).Map(census => services
                .Add(ServiceDescriptor.Describe(
                    typeof(HookRail<AppHostPoint, AppHostFact, TelemetrySource>), _ => inputs.Rail, ServiceLifetime.Singleton))
                .Add(ServiceDescriptor.Describe(typeof(HookRegistry), _ => census, ServiceLifetime.Singleton))
                .Add(ServiceDescriptor.Describe(typeof(FaultCell), _ => inputs.Rail.Faults, ServiceLifetime.Singleton))
                .Add(ServiceDescriptor.Describe(
                    typeof(ReceiptSinkPort),
                    _ => AppHostHooks.Tap(inputs.Sink, inputs.Rail, inputs.Key), ServiceLifetime.Singleton)))),

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

        // The tail is MINTED here, not handed in: each drain opens its own context and token and carries the
        // mounted instrument set and the ledger exporter the terminal `Seal` feeds, so the conductor's whole
        // telemetry tail derives from the one root and no call site assembles a partial one.
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

        // Participants that come into being AFTER the build — a bus subscription's sink, an epoch lease —
        // register into this cell, which the conductor folds beside the contributed port fan. Without it a
        // runtime-born participant has no seat, because the contributor fan closed at the freeze.
        new RootBinding.Seated("drain-rows", static (services, _) =>
            Fin.Succ(services.Add(ServiceDescriptor.Describe(
                typeof(Atom<Seq<DrainRow>>), static _ => Atom(Seq<DrainRow>()), ServiceLifetime.Singleton)))),

        // The capsule takes the rail WHOLE — it fires the phase point itself and owns the `DegradationTap` the
        // composition seats — so constructing it over a single point would leave the rail's own teardown and
        // fault custody unreachable from the one capsule that publishes to it.
        new RootBinding.Seated("lifecycle", static (services, inputs) =>
            Fin.Succ(services.Add(ServiceDescriptor.Describe(
                typeof(Lifecycle),
                _ => new Lifecycle(inputs.Profile, inputs.Clocks, inputs.Correlation, inputs.Rail, inputs.Key),
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
        // after both rows are registered and neither factory reads a graph still being written. `Remote` dials
        // the NAMED client alone, because the round-robin instance selector is the package's own internal
        // default and reaches this probe through `AddServiceDiscovery` on that client's builder; the hand
        // cursor that stood in for it is deleted, and a client without that registration resolves one authority
        // forever. The shared resilience handler rides the same builder so probe and live traffic share one
        // breaker state. The check constructs PER probed authority, because it grades the URI set fixed at ITS
        // construction and a shared instance would accumulate every peer ever probed.
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
                        // The probe reads its bearer PER PROBE off the lease cell, so a peer whose credential
                        // renewed between two rounds is probed with the live one and never re-registered.
                        // `AddCustomHeader` is an `IUriOptions` member, so the header seats inside the
                        // per-URI override callback rather than on the group options; an authority the
                        // deployment declared no credential for adds no header and probes anonymously.
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
                        // Scheme is the resolver QUERY's own ordered preference, so it lands as a declared
                        // composition value rather than a literal inside the projection: `Secure` states TLS
                        // only, and a deployment wanting the package's ordered fallback flips one row.
                        Scheme: DialScheme.Secure,
                        Clocks: inputs.Clocks,
                        Staleness: LeasePolicy.Maintenance.CrashStaleness,
                        Fan: Fanned<CoordinationSignal>(provider, inputs, static signal => new AppHostFact.Coordination(signal))),
                    ServiceLifetime.Singleton))
                .AddHttpClient(nameof(Membership))
                .AddServiceDiscovery()
                .AddStandardResilienceHandler()
                .Services)),

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
                    service: inputs.Profile.HostKey,
                    contribute: (credential, manifest) => ignore(Membership.Contribute(
                        provider.GetRequiredService<Membership.Runtime>(),
                        credential.Pid,
                        inputs.Role,
                        new UnixDomainSocketEndPoint(manifest.SocketPath))),
                    fan: Fanned<CompanionSignal>(provider, inputs, static signal => new AppHostFact.Companion(signal)),
                    clocks: inputs.Clocks,
                    key: inputs.Key),
                ServiceLifetime.Singleton)))),

        // Control-hop dependency frame: this row binds the drain arrow beside the composed seed.
        new RootBinding.Seated("control-inbound", static (services, inputs) =>
            Fin.Succ(services.Add(ServiceDescriptor.Describe(
                typeof(ControlRuntime),
                provider => new ControlRuntime(
                    Degradation: inputs.Control.Degradation,
                    // The admitted peer remainder reaches the one conductor; it intersects local policy once.
                    Drain: inherited => Conducted(provider, inherited),
                    Clocks: inputs.Clocks,
                    Correlation: inputs.Correlation,
                    Source: inputs.Control.Source,
                    Support: inputs.Control.Support,
                    Fan: Fanned<CompanionSignal>(provider, inputs, static signal => new AppHostFact.Companion(signal)),
                    Wire: inputs.Control.Wire),
                ServiceLifetime.Singleton)))),

        // --- [MODULE_SEATS] ---------------------------------------------------------------------
        // The federated classification registry is a boot GATE ahead of every provider mount: a redactor set
        // that refuses after a provider is live leaves the erasing fallback grading the tags the pass rows
        // exist to spare, so the refusal lands before the composition is registered at all. An UNARMED offline
        // policy registers no queue-release participant, because a drain row over a queue set nothing opened
        // reports a flush that moved no bytes.
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

        // Every observability owner this deployment binds, in ONE row: the utilization cell refuses at
        // composition when its provider's timestamp frequency does not admit, so a boot proves the pressure
        // authority rather than discovering absence at the first sample. WHICH driver rows register is this
        // seed's argument list and never a per-row flag — a registration flag beside the registration argument
        // is two answers to one question. The health fold is one call, and that call is also what mounts the
        // resource monitor the Gauge row's policy reads, so no separate monitor registration exists here.
        new RootBinding.Seated("observability-seat", static (services, inputs) =>
            UtilizationCell.Of(PressurePolicy.Canonical.Source, inputs.Clocks.Line, inputs.Key).Map(utilization => services
                .Add(ServiceDescriptor.Describe(typeof(UtilizationCell), _ => utilization, ServiceLifetime.Singleton))
                // The mounted fan and the instrument set it carries are ONE value registered twice under the
                // two contracts their readers spell: the drain thread and the capability-registry mount resolve
                // `InstrumentSet`, the receipt projection resolves the fan, and neither mints a meter.
                .Add(ServiceDescriptor.Describe(typeof(ReceiptFan), _ => inputs.Observability.Fan, ServiceLifetime.Singleton))
                .Add(ServiceDescriptor.Describe(typeof(InstrumentSet), _ => inputs.Observability.Fan.Set, ServiceLifetime.Singleton))
                .Add(ServiceDescriptor.Describe(typeof(AlertCell), _ => new AlertCell(AlertPolicy.Canonical), ServiceLifetime.Singleton))
                .Add(ServiceDescriptor.Describe(
                    typeof(DegradationCell),
                    provider => new DegradationCell(
                        DegradationPolicy.Canonical, inputs.Clocks.Clock, inputs.Correlation,
                        provider.GetRequiredService<HookRail<AppHostPoint, AppHostFact, TelemetrySource>>(), inputs.Key),
                    ServiceLifetime.Singleton))
                .Add(ServiceDescriptor.Describe(
                    typeof(BenchmarkRun.Session),
                    provider => new BenchmarkRun.Session(
                        Source: inputs.Control.Source,
                        Sink: provider.GetRequiredService<ReceiptSinkPort>(),
                        Rail: inputs.Rail,
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

        // The ONE schedule arrow every consumer resolves — `SchedulePort` publishes no `Register` member, so
        // the composition supplies the registration this row seats. Registration is IDEMPOTENT BY KEY: a
        // fresh key seats the row and arms its occurrence loop once; a held key replaces the row and forks
        // nothing, and the loop reads the CURRENT row each pass — so a renewed lease's fresh closure takes
        // effect on the standing loop and a boot row re-registered at runtime cannot double-fire.
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

        // The orchestration capsule and the two runtime-band participants. `Redrive` is the composition's own
        // policy unless a deployment declares one, so a step that exhausts its schedule surfaces as an
        // exhaustion fault rather than a retry loop no bound closes. The permit reclaim is a CADENCE row on the
        // telemetry band beside the permit drain, never a boot gate: a limiter retired mid-run is reclaimed on
        // the same pass that flushes the band it belongs to.
        new RootBinding.Seated("runtime-seat", static (services, inputs) =>
            Fin.Succ(services
                .Add(ServiceDescriptor.Describe(
                    typeof(OrchestrationRuntime),
                    provider => new OrchestrationRuntime(
                        Dispatch: provider.GetRequiredService<DispatchRuntime>(),
                        Store: provider.GetRequiredService<StepStateSeam>(),
                        Assess: provider.GetRequiredService<Func<CommandReceipt, Option<StepDisposition>>>(),
                        Redrive: Orchestrator.StepRedrive,
                        Lease: provider.GetRequiredService<LeaseElection.Runtime>(),
                        Schedule: provider.GetRequiredService<Func<ScheduleEntry, IO<Unit>>>(),
                        Clocks: inputs.Clocks,
                        Sink: provider.GetRequiredService<ReceiptSinkPort>()),
                    ServiceLifetime.Singleton))
                .Add(ServiceDescriptor.Describe(
                    typeof(Atom<Option<InMemoryProvider>>), static _ => Atom(Option<InMemoryProvider>.None), ServiceLifetime.Singleton))
                .Add(ServiceDescriptor.Describe(
                    typeof(Atom<Option<LaneGuard.Runtime>>), static _ => Atom(Option<LaneGuard.Runtime>.None), ServiceLifetime.Singleton))
                .Add(Participant("lane-permits", DrainBand.Telemetry, 0, static provider => _ =>
                    provider.GetRequiredService<LanePermits>().Reclaim()
                        .Bind(_ => provider.GetRequiredService<LanePermits>().Drain())))
                // An absent watchdog enrollment registers NO heartbeat and NO process-dump contributor, so a
                // host the supervisor never enrolled carries neither a cadence nothing reads nor a dump policy
                // no trigger can reach.
                .Fold(Enrolled(inputs), static (current, row) => current.Add(row)))),

        // One `HttpLane.Wire` per `SocketsHttpHandler`-borne hop case, each with the deployment's own weighted
        // route span — a route span is genuine deployment data and stays a parameter. `KeyedLane.Register`
        // folds `HopRows.Items` itself, so no row span crosses here, and the one `HopEvidence` the composition
        // minted rides on both the keyed composition and the outbound runtime rather than on process statics
        // that could not reset between compositions. The bearer crosses as an ARROW closing over the roster,
        // so the lane's link resolves the live cell at send time; a bearer resolved here would freeze the
        // credential every registered client carries at the instant the collection was still editable.
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
                            // The drop sink FANS: a drop that terminates in process is a loss no operator can
                            // count and no dashboard can show, so it crosses the receipt estate under its own
                            // kind exactly as every other settled fact does.
                            Drops: drop => ignore(provider.GetRequiredService<ReceiptSinkPort>().Send(
                                inputs.Correlation, TenantContext.Current, TelemetrySource.AppHost,
                                ReceiptKind.Drop.Key,
                                WireJson.Element(drop)).Run()),
                            // Bus subscriptions open after the build, so their drain rows land in the late cell
                            // the conductor folds rather than on a contributor fan that closed at the freeze.
                            Register: row => ignore(provider.GetRequiredService<Atom<Seq<DrainRow>>>().Swap(held => held.Add(row))),
                            Clocks: inputs.Clocks,
                            Spine: provider.GetRequiredService<Lifecycle>().Spine),
                        ServiceLifetime.Singleton))
                    .Add(ServiceDescriptor.Describe(
                        typeof(Atom<Option<EventBus.Cell>>), static _ => Atom(Option<EventBus.Cell>.None), ServiceLifetime.Singleton))
                    // The detach rail was STORED and never invoked: every channel the process opened outlived
                    // it, so release rides the interaction band where the sockets it holds belong.
                    .Add(Participant("live-wire", DrainBand.Interaction, 0, static provider => _ =>
                        provider.GetRequiredService<LiveWireRuntime>() is var runtime
                            ? runtime.Bound().TraverseM(handle => LiveWire.Release(runtime, handle)).As().Map(static _ => unit)
                            : IO.pure(unit)))
                    .Add(ServiceDescriptor.Describe(
                        typeof(HealthContributorRow),
                        provider => BindingHealth.Contribute(
                            provider.GetRequiredService<LiveWireRuntime>(), inputs.Observability.ProbeCadence),
                        ServiceLifetime.Singleton)))),

        // ONE fenced runtime serves both namespaces: `RoleElection` and `DistributedLock` each take it beside
        // the coordination fan, so the two seated runtime records the split demanded collapse to one value and
        // a lease acquired under one is fenced under the other by construction. This is also the seat where
        // staleness config MEETS the drain bounds it must outlast, so `LeasePolicy.Outlasts` is FORCED here as
        // a boot claim: the guard is a `Lazy` that throws on a reclaim window shorter than the cooperative plus
        // forced drain sum, and a proof nothing forces is a guarantee nothing holds — a draining holder would
        // be reclaimed mid-drain and the fence would read perfectly armed.
        new RootBinding.Seated("coordination-seat", static (services, inputs) =>
            Op.Of().Catch(static () => Fin.Succ(LeasePolicy.Outlasts))
                .MapFail(static _ => (Error)new KernelFault.InvalidValue(
                    Label: $"{nameof(LeasePolicy)}.{nameof(LeasePolicy.Maintenance)}",
                    Requirement: "<a crash-staleness window outlasting the cooperative and forced drain bounds>"))
                .Map(_ => services
                    // The lease adapter's four store arrows arrive DECODED on the coordination seed exactly as
                    // the membership three do, so the runtime every fenced holder resolves is constructed here
                    // rather than resolved from a registration no fold performs.
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
                            Staleness: LeasePolicy.Maintenance.CrashStaleness,
                            Sink: provider.GetRequiredService<ReceiptSinkPort>()),
                        ServiceLifetime.Singleton))
                    .Add(ServiceDescriptor.Describe(
                        typeof(FactSink<CoordinationSignal>),
                        provider => Fanned<CoordinationSignal>(provider, inputs, static signal => new AppHostFact.Coordination(signal)),
                        ServiceLifetime.Singleton)))),

        // Both gRPC services mount on the SAME served-plane fold, so the control verbs and the diagnostic
        // verbs share one `ControlRuntime` and an operator over either hop invokes one semantics.
        new RootBinding.Seated("companion-seat", static (services, inputs) =>
            Fin.Succ(ServiceHost.Register(services, [.. inputs.Wire.Planes])
                .Add(ServiceDescriptor.Describe(
                    typeof(FactSink<CompanionSignal>),
                    provider => Fanned<CompanionSignal>(provider, inputs, static signal => new AppHostFact.Companion(signal)),
                    ServiceLifetime.Singleton))
                .Add(ServiceDescriptor.Describe(typeof(IngressPolicy), _ => inputs.Wire.Ingress, ServiceLifetime.Singleton))
                .Add(ServiceDescriptor.Describe(typeof(ModalityRow), _ => inputs.Wire.Modality, ServiceLifetime.Singleton))
                // The bound listener releases on the interaction band, because the acquisition IS the modality
                // transition and a socket set held past the drain outlives the process that owns it.
                .Add(Participant("host-binding", DrainBand.Interaction, 1, static provider => _ =>
                    HostBinding.Release(provider.GetRequiredService<BoundEndpoint>())))
                // The cascade is the WRITE half of the degradation seam: a parent's forced level enters the
                // child's cell as a floor, so a degraded parent never leaves a child serving above it.
                .Add(Participant("degradation-cascade", DrainBand.Interaction, 2, static provider => _ =>
                    provider.GetRequiredService<PeerRoster>().Attached
                        .TraverseM(entry => DegradationCascade.Cascade(
                            provider.GetRequiredService<PeerRoster>(), entry.Peer,
                            provider.GetRequiredService<DegradationCell>().Level,
                            nameof(DrainBand.Interaction), provider.GetRequiredService<ModalityRow>()))
                        .As().Map(static _ => unit)))),

        // The consent roster is the ONE seat that makes all four `Consent` cases producible: the broker's
        // `ConsentOf` closes over the roster AND the clock, because the roster grades a standing scope's window
        // against an instant while the broker's own column carries the tenant alone. Federation lands in this
        // seat rather than after the build because `Federate` REGISTERS one descriptor per peer, and the
        // capability registry freezes at the module fold — a peer opened later reaches no catalog.
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
                    // ONE chain cell the dispatch front door and the reasoning transcript projection both
                    // advance, so two heads can never fork off one command log.
                    .Add(ServiceDescriptor.Describe(
                        typeof(Atom<EventLog.Chain>), _ => Atom(EventLog.Chain.Genesis), ServiceLifetime.Singleton))
                    // `Lanes` is the runtime `lane-closure` proved, so every brokered dispatch crosses the
                    // governor. The cell is filled by the FIRST `Proven` row and this factory runs no earlier
                    // than the first resolve after the build, so the ordering is the roster's, not a race —
                    // and an empty cell here is a ledger that lost its own order rather than a runtime state.
                    .Add(ServiceDescriptor.Describe(
                        typeof(CommandRuntime),
                        provider => new CommandRuntime(
                            Registry: provider.GetRequiredService<CapabilityRegistry>(),
                            Broker: provider.GetRequiredService<GrantBroker>(),
                            Lanes: provider.GetRequiredService<Atom<Option<LaneGuard.Runtime>>>().Value
                                .IfNone(() => throw new UnreachableException(nameof(LaneGuard))),
                            Dispatch: provider.GetRequiredService<Func<CommandBody, Spec, CommandArguments, IO<Fin<DispatchReceipt>>>>(),
                            CompensationOf: provider.GetRequiredService<Func<string, Option<string>>>(),
                            Clocks: inputs.Clocks,
                            Sink: provider.GetRequiredService<ReceiptSinkPort>(),
                            Wire: inputs.Control.Wire,
                            Spine: provider.GetRequiredService<Lifecycle>().Spine),
                        ServiceLifetime.Singleton))
                    .Add(ServiceDescriptor.Describe(
                        typeof(DispatchRuntime),
                        provider => new DispatchRuntime(
                            Command: provider.GetRequiredService<CommandRuntime>(),
                            Mediation: provider.GetRequiredService<MediationRuntime>(),
                            ScopeOf: provider.GetRequiredService<Func<CommandIntent, Option<GrantScope>>>(),
                            Chain: provider.GetRequiredService<Atom<EventLog.Chain>>(),
                            Context: inputs.Telemetry.Determinism,
                            Changefeed: inputs.Changefeed,
                            Rail: inputs.Rail,
                            Key: inputs.Key),
                        ServiceLifetime.Singleton))
                    // A handle absent while its modality row is admitted REFUSES at the mint, so a governance
                    // runtime claiming a modality it cannot serve never reaches a caller.
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
                            Sink: provider.GetRequiredService<ReceiptSinkPort>(),
                            Wire: inputs.Control.Wire),
                        ServiceLifetime.Singleton))
                    // ONE adoption per composition, its halves handed to the two front doors: the MCP server
                    // takes the adopted server tools and the reasoning loop takes the AI functions, so a tool
                    // projected twice is unspellable and the plugin route takes neither.
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

        // The trust anchor admits at COMPOSITION: a refused root stops boot on the typed rail naming it, and
        // both the sandbox runtime's gate and the update rail's read this ONE value rather than each admitting
        // its own copy. The anchor crosses as a `TrustAnchor` CASE rather than a bare file handle, so the
        // hermetic pinned root and the connected TUF root are two shapes of one argument and this deployment
        // states which it holds at the seat instead of a provider choice buried inside the gate.
        new RootBinding.Seated("sandbox-seat", static (services, inputs) =>
            SupplyChainGate.Runtime.Of(
                    new TrustAnchor.PinnedCase(inputs.Sandbox.TrustRoot), inputs.Sandbox.PolicyOf,
                    inputs.Sandbox.Staging, inputs.Sandbox.ContractVersion, inputs.Clocks)
                .Bind(gate => FeedBinding.Of(inputs.Sandbox.Channel, inputs.Configuration).Map(feed => services
                    .Add(ServiceDescriptor.Describe(typeof(SupplyChainGate.Runtime), _ => gate, ServiceLifetime.Singleton))
                    // The engine is ONE per host and both preemption mechanisms are unsettable after it exists,
                    // so `Preempting` mints it whole — an engine composed without epoch interruption renders
                    // the entire kill rail inert while every deadline still reads as armed.
                    .Add(ServiceDescriptor.Describe(
                        typeof(SandboxRuntime),
                        provider => new SandboxRuntime(
                            Gate: gate,
                            Command: provider.GetRequiredService<CommandRuntime>(),
                            Engine: SandboxRuntime.Preempting(inputs.Sandbox.StackBytes),
                            EpochPeriod: inputs.Sandbox.EpochPeriod,
                            Vehicles: inputs.Sandbox.Vehicles,
                            Clocks: inputs.Clocks,
                            Sink: provider.GetRequiredService<ReceiptSinkPort>(),
                            Spine: provider.GetRequiredService<Lifecycle>().Spine),
                        ServiceLifetime.Singleton))
                    .Add(ServiceDescriptor.Describe(
                        typeof(UpdateRail),
                        provider => new UpdateRail(
                            feed, provider.GetRequiredService<Lifecycle>(),
                            provider.GetRequiredService<ReceiptSinkPort>(), gate,
                            provider.GetRequiredService<IMeterFactory>().Create(nameof(UpdateRail))),
                        ServiceLifetime.Singleton))
                    .Add(ServiceDescriptor.Describe(typeof(FleetRuntime), _ => inputs.Sandbox.Fleet, ServiceLifetime.Singleton))
                    // The hosted roster is a CELL rather than a sequence the pacer closes over, because two
                    // readers need it: the epoch ticker sweeps the live set every tick and the operator's
                    // `sandbox-release` verb addresses one member of it by plugin id. `sandbox-boot` fills it.
                    .Add(ServiceDescriptor.Describe(
                        typeof(Atom<Seq<HostedSolver>>), static _ => Atom(Seq<HostedSolver>()), ServiceLifetime.Singleton))))),

        // --- [MODULE_BOOTS] ---------------------------------------------------------------------
        // Proven, not seated: the pipelines exist only after the build, and the RUNTIME the probe returns is
        // the value `CommandRuntime.Lanes` carries — so the roster probe and the governor seat are one act,
        // and a brokered dispatch cannot reach the graph past a lane this probe never proved.
        new RootBinding.Proven("lane-closure", static (provider, inputs) =>
            LaneGuard.Proven(
                    provider.GetRequiredService<ResiliencePipelineProvider<string>>(), inputs.Lanes, [.. inputs.LaneRows])
                .Map(lanes => ignore(provider.GetRequiredService<Atom<Option<LaneGuard.Runtime>>>().Swap(_ => Some(lanes))))),

        new RootBinding.Proven("hop-closure", static (provider, _) =>
            KeyedLane.Proven(provider.GetRequiredService<ResiliencePipelineProvider<string>>())),

        // Proven, not seated: the issuer exists only after the build, and the token's POSITION is the fact worth
        // proving — an unfolded roster answers a positionless token whose writes drop with nothing raised, while
        // the name echoes back whatever string was handed in and proves nothing. The capture is here because
        // `ThrowOnUnregisteredNames` makes the same omission a throw, so both shapes of the one defect leave on
        // this rail naming this seam, and the outbound runtime then carries the resolved token rather than a
        // name it re-resolves per hop.
        new RootBinding.Proven("hop-checkpoint", static (provider, _) =>
            Op.Of().Catch(() => Fin.Succ(provider.GetRequiredService<ILatencyContextTokenIssuer>()
                    .GetCheckpointToken(LatencyCheckpoint.Hop.Key)))
                .Bind(static token => token.Position >= 0
                    ? Fin.Succ(unit)
                    : Fin.Fail<Unit>(new KernelFault.InvalidValue(
                        Label: LatencyCheckpoint.Hop.Key,
                        Requirement: "<a checkpoint name the folded latency roster carries>")))),

        // Both sweeps are CADENCES, never boot gates: the alert sweep rides the health publish period so the
        // engine reads the reading the publisher just committed and the two grade one observation of one
        // moment, and the retention sweep rides the bundles policy row.
        new RootBinding.Proven("observability-boot", static (provider, inputs) =>
            Scheduled(provider,
                Cadence("alert-sweep", DegradationPolicy.Canonical.PublishPeriod, DeadlineClass.HealthProbe, () =>
                    AlertEngine.Sweep(
                            new AlertEngine.Runtime(Sink: provider.GetRequiredService<ReceiptSinkPort>(), Key: inputs.Key),
                            provider.GetRequiredService<AlertCell>(),
                            provider.GetRequiredService<DegradationCell>().Read(),
                            inputs.Clocks.Now)
                        .Map(static _ => unit)),
                Cadence("support-sweep", inputs.Observability.RetentionSweep, DeadlineClass.SupportWindow, () =>
                    SupportLedger.Sweep(provider.GetRequiredService<SupportRuntime>()).Map(static _ => unit)))),

        // The features rail seats FOUR concerns in one call and the module HOLDS the returned provider, because
        // `FlagCompilation.Reload` is the re-fold leg and a compile with no handle has nowhere to land — a
        // compile no module reaches leaves the whole rail unregistered while every consumer reads
        // `FlagVerdict.Inert` as policy. Recovery is a boot gate and reclamation a cadence under the
        // reclaim-role lease, so an interrupted workflow resumes on THIS node's boot while an orphan left by a
        // dead node is reclaimed by whichever node holds the lease.
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

        // `Seat` runs `HopRows.Admitted` ahead of its claim fold, so an authored row carrying an illegal corner
        // refuses at boot rather than at the hop that first dials it; `Enforce` then folds the faculty groups
        // against the live level, because a group narrowed at boot and never re-read holds the boot posture
        // through every level change after it.
        new RootBinding.Proven("wire-boot", static (provider, inputs) =>
            OutboundSurface.Seat(provider.GetRequiredService<OutboundRuntime>())
                .Bind(_ => OutboundSurface
                    .Enforce(
                        provider.GetRequiredService<OutboundRuntime>(),
                        provider.GetRequiredService<DegradationCell>().Level)
                    .Run())
                .Bind(_ => EventBus.Mount(provider.GetRequiredService<EventBus.Runtime>(), [.. inputs.Wire.Subscriptions]))
                .Map(bus => ignore(provider.GetRequiredService<Atom<Option<EventBus.Cell>>>().Swap(_ => Some(bus))))
                // The sweep is FENCED: the election names the outbox role and the cadence renews under the
                // holding it returns, so exactly one node sweeps a shared outbox and a lease lost mid-cadence
                // stops the next pass rather than racing the node that took it.
                .Bind(_ => Scheduled(provider,
                    Cadence("outbox-sweep", inputs.Wire.Outbox.Cadence, () =>
                        RoleElection.Elect(
                                provider.GetRequiredService<FencedRuntime>(),
                                provider.GetRequiredService<FactSink<CoordinationSignal>>(),
                                provider.GetRequiredService<Membership.Runtime>().View.Value,
                                RoleName.Create(OutboxRelay.SweepRole.Id))
                            .Bind(elected => elected.Match(
                                Succ: holding => RoleElection
                                    .Hold(
                                        provider.GetRequiredService<FencedRuntime>(),
                                        provider.GetRequiredService<FactSink<CoordinationSignal>>(),
                                        holding, FenceVerb.Renew)
                                    .Bind(_ => inputs.Wire.Watermark().Match(
                                        Succ: watermark => OutboxRelay
                                            .Sweep(inputs.Wire.Outbox, TenantContext.Current, watermark)
                                            .Map(static _ => unit),
                                        Fail: IO.fail<Unit>)),
                                // A contended election is the ordinary answer on every node but one, so the
                                // cadence completes rather than reporting a refusal an operator would read as
                                // a fault.
                                Fail: static _ => IO.pure(unit))))))),

        // The durable member set reseats BEFORE the first sweep, so a node returning after a restart grades
        // against the members the store holds rather than probing an empty view back into existence.
        new RootBinding.Proven("coordination-boot", static (provider, _) =>
            Membership.Rebuild(provider.GetRequiredService<Membership.Runtime>()).Run()
                .Bind(static view => view.ToFin())
                .Bind(_ => Scheduled(provider, Membership.Cadence(provider.GetRequiredService<Membership.Runtime>())))),

        // The listener bind IS the modality transition, so its `BindReceipt` fans off the acquisition rather
        // than off a caller that observed one — which is why the fan enters the arity and the unread roots
        // parameter left it.
        new RootBinding.Proven("companion-boot", static (provider, inputs) =>
            HostBinding.Acquire(inputs.Wire.Listener, provider.GetRequiredService<FactSink<CompanionSignal>>()).Run()
                .Bind(static bound => bound)
                .Map(static _ => unit)),

        // The registry census is the one descriptor claim the contributor-port fold cannot carry, so it mounts
        // against the instrument set after the build; the resource subscriptions bind with the spec their own
        // live-wire channel already holds, and the BOOT-DECLARED token CELLS register their refresh cadence
        // here — constructed at input assembly ahead of the provider, they cannot self-register — while every
        // runtime acquisition registers inside `Acquisition.Acquire`; the keyed arrow makes the two seats one
        // idempotent registration, so a boot row later re-acquired replaces rather than double-arms.
        new RootBinding.Proven("agent-boot", static (provider, inputs) =>
            provider.GetRequiredService<CapabilityRegistry>()
                .Mount(provider.GetRequiredService<InstrumentSet>())
                .Bind(_ => inputs.Agent.Subscriptions
                    .TraverseM(row => FederationSubscription.Subscribe(
                        inputs.Agent.Federation, row.Server, row.Uri, row.Spec,
                        provider.GetRequiredService<ChannelWriter<ExternalValue>>()))
                    .As().Run().Map(static _ => unit))
                .Bind(_ => Scheduled(provider, [.. inputs.Agent.Leases.Refreshes]))),

        // Every declared solver hosts under one ACCUMULATING traversal, so a boot naming two bad plugins names
        // both; the epoch ticker opens ONCE and its lease closes on the compute band, because `SetEpochDeadline`
        // arms a counter no store consults until something increments it and the whole wall guarantee is inert
        // without this ticker. A staged release resumes before normal startup, so a rollover interrupted by a
        // process bounce finishes from its staged phase rather than re-staging bytes the gate already admitted.
        new RootBinding.Proven("sandbox-boot", static (provider, inputs) =>
            Hosting(provider, inputs)
                .Bind(hosting => SolverHost
                    .Register(hosting, inputs.Sandbox.Solvers, inputs.Sandbox.Scope, inputs.Key)
                    .Run())
                .Bind(static hosted => hosted.ToFin())
                .Map(hosted => Paced(provider, hosted, inputs.Key))
                .Bind(_ => Resumed(provider))),

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
                        : new KernelFault.InvalidValue(Label: pin.Name, Requirement: "<a container-owned disposable>"))
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
    // Both ACCUMULATE on the Validation applicative, which is what makes the roster's own claim true — one
    // boot names every unbound seam. A monadic fold here would sequence the rows and answer the first refusal
    // alone, turning a boot that should report four missing bindings into four boots reporting one each.
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

    // --- [INSTRUMENT_MOUNT]
    // The one `InstrumentFan.Mount` call site, and it runs BEFORE `Arm`: the fan's `Tap` is a subscription
    // `HookRail.Of` seats at construction, and this fold receives that rail already composed on `RootInputs`,
    // so the mount cannot be a ledger row without the rail depending on a value the rail's own consumer mints.
    // The result rides back in on `ObservabilitySeed.Fan`, which the observability seat registers. The
    // CONTRIBUTOR ROSTER is this member's own declaration: a package that DECLARES an instrument family hands
    // its port here, and a family declared on a port no roster names mints instruments the view predicate, the
    // board, and the governance roster never reach — which is exactly what a source-generated instrument
    // partial with no port row buys.
    public static Fin<ReceiptFan> Metered(
        IMeterFactory factory, CorrelationId root, LevelCells cells, string version,
        Seq<HashMap<ArmKey, InstrumentArm>> contributed, params ReadOnlySpan<TelemetryContributorPort> external) =>
        InstrumentFan.Mount(factory, root, cells, contributed,
            [AppHostMeasure.Telemetry(version), UpdateMetrics.Port(version), .. external]);

    // --- [COMPOSITION] --------------------------------------------------------------------------
    // Drain arrow the control hop takes, COMPOSED rather than re-implemented: participant rows arrive
    // field-identical off the `DrainParticipantPort` contributor fan, the late cell carries the participants
    // born after the freeze, and one `DrainThread` mint per drain carries the conductor's whole telemetry
    // tail. `Seal` belongs to the act, not to a caller after it — that context is minted per drain, so
    // draining without exporting its frozen ledger discards exactly the checkpoints the phase recorded.
    static IO<DrainReceipt> Conducted(IServiceProvider provider, Duration inherited) =>
        from thread in IO.lift(provider.GetRequiredService<Func<DrainThread>>())
        from receipt in provider.GetRequiredService<Lifecycle>().Drain(
            toSeq(provider.GetServices<DrainParticipantPort>())
                .Map(static row => new DrainRow(row.Name, row.Band, row.Rank, row.Drain))
                + provider.GetRequiredService<Atom<Seq<DrainRow>>>().Value,
            thread.Latency, thread.Checkpoint, thread.Instruments, inherited)
        from _sealed in LatencySpine.Seal(thread.Exporter, thread.Latency)
        select receipt;

    static DrainThread Threaded(
        (ILatencyContext Context, CheckpointToken Phase) opened, InstrumentSet instruments, ILatencyDataExporter exporter) =>
        new(opened.Context, opened.Phase, instruments, exporter);

    // One fan mint per signal family, so a producing page's durable receipt and its in-process rail point are
    // two readers of ONE fact and no seat pairs a sink with a point the fact's own `At` did not project.
    static FactSink<TSignal> Fanned<TSignal>(
        IServiceProvider provider, RootInputs inputs, Func<TSignal, AppHostFact> fact) where TSignal : notnull =>
        new(provider.GetRequiredService<ReceiptSinkPort>(), inputs.Rail, fact, inputs.Key);

    static ServiceDescriptor Participant(
        string name, DrainBand band, int rank, Func<IServiceProvider, Func<CancellationToken, IO<Unit>>> drain) =>
        ServiceDescriptor.Describe(
            typeof(DrainParticipantPort),
            provider => new DrainParticipantPort(name, band, rank, drain(provider)),
            ServiceLifetime.Singleton);

    // The deployment's own probe list, folded with the continuous gauge row: the discrete disk and allocation
    // ceilings are the hard-breach complement to the windowed ratio, and both project onto the one
    // `Pressure`-tagged contributor set rather than two utilization sources.
    static Seq<HealthContributorRow> Probed(RootInputs inputs, UtilizationCell utilization) =>
        inputs.Observability.Probes
            .Map(row => HealthContributorRow.Of(
                new ProbeSource.Driver(row.Row, row.Check), inputs.Observability.ProbeCadence))
            .Add(HealthContributorRow.Of(
                new ProbeSource.Gauge(utilization, inputs.Observability.Energy, PressurePolicy.Canonical),
                inputs.Observability.ProbeCadence));

    // An absent enrollment registers nothing at all — no keep-alive cadence and no dump contributor — because
    // a heartbeat nothing supervises and a dump policy no trigger reaches are both apparatus without a reader.
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

    // The governance capsule is minted ONCE: an admitted modality whose handle is absent refuses here, so the
    // conditional corner the folder RULINGS name is a construction fact rather than a per-call branch.
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

    // The solver hosting capsule, and the one seat that closes the compile column: `CompileOf` takes the
    // LOADED instance because the closure it builds dispatches INTO that guest, and the instance is resolved
    // inside the hosting fold after the sandbox load — a closure minted at this root without it could only
    // have compiled against a plugin it has no handle on. `SandboxRows.Enter` is the one guest crossing, so
    // the closure never re-narrows the boundary the sandbox already owns.
    static Fin<SolverHostRuntime> Hosting(IServiceProvider provider, RootInputs inputs) =>
        Isolation.Wasm.Row.Map(row => new SolverHostRuntime(
            Sandbox: provider.GetRequiredService<SandboxRuntime>(),
            Row: row,
            Mcp: provider.GetRequiredService<McpRuntime>(),
            Hosted: inputs.Sandbox.Hosted,
            Resolve: manifest => Ranked(inputs, manifest),
            CompileOf: (instance, negotiation, op) => arguments =>
                // The ONE guest crossing, and the reason this column takes the instance: `Enter` is where a
                // `TrapException` is observable at all, so the trap seats on the capsule here and no caller
                // downstream re-classifies it. An op the guest never exported rides the Option out as a
                // contract refusal rather than a null nothing checked.
                SandboxRows.Enter(instance, guest => Optional(guest.GetFunction(op.OpId)))
                    .Run()
                    .ToFin(new SolverFault.ContractRejected($"{instance.PluginId}.{op.OpId}: <no guest export>"))
                    .Map(export => new CommandBody(
                        instance.PluginId, op.OpId,
                        JsonSerializer.SerializeToElement(
                            export.Invoke(arguments.Payload.GetRawText(), negotiation.Tolerance), SuiteContracts.Host))),
            Project: inputs.Sandbox.Project));

    // The registry resolve RANKS before it presents: `SupplyChainGate.Best` is the one candidate policy over a
    // contract range, so the chosen version is the gate's answer and the artifact behind it is what
    // `AdmissionSubject.Plugin` then carries into `Admit` — ranking and admission read one contract in one
    // order, and a registry picking its own newest is unspellable at this seat. The policy row resolves off any
    // offered candidate because the ranking is over versions of ONE plugin identity, never across subjects.
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

    // The hosted roster lands in the CELL first, and the ticker reads the cell rather than closing over the
    // sequence: the pacer's sweep and the operator's release verb then address ONE live set. The ticker's own
    // lease is a DRAIN ROW at the compute band, because closing it is what stops the epoch advancing under a
    // host that is shutting down.
    static Unit Paced(IServiceProvider provider, Seq<HostedSolver> hosted, Op key) =>
        provider.GetRequiredService<Atom<Seq<HostedSolver>>>() is var roster
            && ignore(roster.Swap(_ => hosted)) is var _
            && EpochPacer.Open(
                provider.GetRequiredService<SandboxRuntime>(),
                () => roster.Value.Map(static row => row.Instance),
                key) is var lease
            ? ignore(provider.GetRequiredService<Atom<Seq<DrainRow>>>().Swap(held => held.Add(
                new DrainRow(nameof(EpochPacer), DrainBand.Compute, 0, _ => IO.lift(lease.Dispose)))))
            : unit;

    // A staged release resumes through the SAME drain-gated rollover a fresh one takes, so the handoff has one
    // path and the bare apply-and-exit forms have none.
    static Fin<Unit> Resumed(IServiceProvider provider) =>
        provider.GetRequiredService<UpdateRail>() is var rail
            ? rail.Pending.Match(
                Some: asset => rail.Rollover(asset, provider.GetRequiredService<Func<DrainThread>>()()).Run().Map(static _ => unit),
                None: static () => Fin.Succ(unit))
            : Fin.Succ(unit);

    // Cadences register through the schedule port rather than through a timer of their own, so every recurring
    // act on this root shares one missed-occurrence law and one deadline taxonomy.
    static Fin<Unit> Scheduled(IServiceProvider provider, params ReadOnlySpan<ScheduleEntry> entries) =>
        Iterable<ScheduleEntry>.FromSpan(entries)
            .Traverse(entry => provider.GetRequiredService<Func<ScheduleEntry, IO<Unit>>>()(entry).Run().ToValidation())
            .As()
            .Map(static _ => unit)
            .ToFin();

    // The arrow's registration body: first writer per key ARMS the loop, a later writer replaces the row.
    // `Cell.Claim` is the same first-writer-wins transition the federation session seat rides, so a racing
    // double-registration commits one loop and the ceding caller lands its row for that loop to read.
    static IO<Unit> Armed(Atom<HashMap<string, ScheduleEntry>> roster, ClockPolicy clocks, ScheduleEntry entry) =>
        Cell.Claim(roster, entry.Key, () => entry) switch {
            Transition<HashMap<string, ScheduleEntry>>.Committed => Occurring(roster, clocks, entry.Key).Fork(None).Map(static _ => unit),
            _ => IO.lift(() => ignore(roster.Swap(held => held.SetItem(entry.Key, entry)))),
        };

    // One occurrence loop per armed key, riding the composition EnvIO whose token is the lifecycle spine, so
    // drain cancels every loop at one seat. Each pass re-reads the roster's CURRENT row — a replaced entry's
    // fresh spec and closure take effect without a second fork, a removed key retires its loop, and a grammar
    // answering no next occurrence retires the row; `SchedulePort.Run` carries the deadline gauge and the
    // redrive curve, and a leased row gates inside its own `Work` through its consumer's election law.
    static IO<Unit> Occurring(Atom<HashMap<string, ScheduleEntry>> roster, ClockPolicy clocks, string key) =>
        roster.Value.Find(key).Match(
            None: () => IO.pure(unit),
            Some: entry => SchedulePort.Next(entry, clocks.Now).Match(
                None: () => IO.lift(() => ignore(roster.Swap(held => held.Remove(key)))),
                Some: next => IO.yieldFor((next - clocks.Now).ToTimeSpan())
                    .Bind(_ => SchedulePort.Run(clocks, entry))
                    .Bind(_ => Occurring(roster, clocks, key))));

    static ScheduleEntry Cadence(string key, Duration every, DeadlineClass deadline, Func<IO<Unit>> work) =>
        new(Key: key, Spec: new OccurrenceSpec.Every(every), Deadline: deadline,
            Lease: Some(LeasePolicy.Maintenance), Redrive: RedrivePolicy.None, Work: work);

    static ScheduleEntry Cadence(string key, ScheduleEntry declared, Func<IO<Unit>> work) =>
        declared with { Key = key, Work = work };
}
```

The following fence belongs in each product composition assembly that admits both Bim and Compute. The product passes its Compute module before `ProductModules.BimCompute` so the descriptor graph proves every constructor dependency at the one provider build; AppHost itself keeps its kernel-and-contracts-only dependency direction.

```csharp signature
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

// The product root is the only assembly that may name both sides. This adapter is a typed projection and owns no
// request vocabulary, transport policy, retry, frame admission, artifact identity, or re-import logic.
public sealed class BimComputeCompanion(
    WireServices services,
    CallSpineFactory spines,
    StreamPool pool,
    ReceiptSurface receipts) : ITessellationCompanion {
    public IO<Fin<TessellationCross>> Cross(
        Rasm.Bim.TessellationRequest request,
        CorrelationId correlation,
        CancellationToken cancel,
        Op key) {
        CallSpine spine = spines.Create(correlation);
        WireCall calls = services.Bind(spine);
        return FrameEdge.Frames(request.SourceBytes).Match(
            Succ: partition => FrameEdge.Put(calls, spine, partition, cancel).Bind(uploaded => uploaded.Match(
                Succ: source => TessellationWire.Project(request, source, key).Match(
                    Succ: wire => CompanionEdge
                        .Tessellate(services, spine, pool, receipts, wire, cancel)
                        .Map(outcome => outcome.Bind(artifact =>
                            TessellationWire.Admit(request, artifact.Receipt, artifact.Glb, key))),
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
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
