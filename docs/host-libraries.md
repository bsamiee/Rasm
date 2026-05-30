# [H1][HOST_LIBRARIES]
>**Dictum:** *Domain libs use LanguageExt rails and runtime records; host packages belong at composition roots and marked boundary adapters.*

<br>

Scope: packages that extend `IServiceCollection` or external I/O boundaries — not domain modules. Pin guidance below is adoption truth; the MSBuild graph may lag until a host consumer exists.

Package pins (doc): Scrutor **7.0.0**; Microsoft.Extensions.DependencyInjection.Abstractions **10.0.x**; FluentValidation **11.x**; NodaTime **3.x**; EF Core **10.x**; Serilog **4.x**; OpenTelemetry.Extensions.Hosting **1.x**; Microsoft.Extensions.Http.Resilience **10.0.x**.

---
## [1][WHEN_HOST_DI_EXISTS]
>**Dictum:** *Scrutor and host adapters pay off only where `Microsoft.Extensions.DependencyInjection` is the composition root.*

<br>

| [INDEX] | [SIGNAL]                                                                   |
| :-----: | -------------------------------------------------------------------------- |
|   [1]   | Single library with `Live()` / `Eff.runtime<RT>()` factories, no container |
|   [2]   | Generic host (`WebApplication`, `IHost`, test host, bridge service)        |
|   [3]   | Many sibling apps sharing port implementations                             |
|   [4]   | Cross-cutting `Eff<RT,T>` decorators (retry, trace, metrics)               |
|   [5]   | In-process plugin with no `IServiceCollection`                             |

[ACTION]
- [1] [NEVER] add Scrutor to domain; keep runtime-record DI.
- [2] Add Scrutor at bootstrap; `RegistrationStrategy.Throw` per scan block.
- [3] `Scan` + bounded assemblies/namespaces.
- [4] `Decorate` / `TryDecorate` at root only.
- [5] Explicit factories; optional thin bootstrap scope per load — not inside command/solve hot paths.

Runtime-record composition (`Eff.runtime<RT>()`, scoped `IServiceScope` for hosted services) and Scrutor are complementary: records inside pipelines; Scrutor wires the graph that builds `RT` or boundary services.

---
## [2][SCRUTOR]
>**Dictum:** *Scan discovers implementations; Decorate orders cross-cutting topology — both are composition-root only.*

<br>

Scrutor **7.0.0** targets net462, netstandard2.0, net8.0, **net10.0** (no net9.0 TFM on the package). Imports: `using Scrutor;`.

### Capabilities

| [INDEX] | [MECHANISM]                                               | [USE]                                                                       |
| :-----: | --------------------------------------------------------- | --------------------------------------------------------------------------- |
|   [1]   | `Scan` + `AddClasses`                                     | Register handlers, exporters, strategies behind shared interfaces           |
|   [2]   | `UsingRegistrationStrategy(Throw)`                        | Fail fast on duplicate non-keyed registrations                              |
|   [3]   | `UsingRegistrationStrategy(Skip)`                         | Optional plugin modules (first wins on `ServiceType`)                       |
|   [4]   | `Replace(ReplacementBehavior.*)`                          | Override default implementation in tests or profiles                        |
|   [5]   | `Decorate` / `TryDecorate`                                | Retry, tracing, caching, authorization shells                               |
|   [6]   | `DecorationStrategy` / factory decorate overloads         | Predicate or factory-based decoration when type-based match is insufficient |
|   [7]   | `WithServiceKey` / `[ServiceDescriptor(..., serviceKey)]` | Variant routing (region, backend, feature flag) — v7                        |
|   [8]   | `DecoratedService<T>` + `out`                             | Inspect decorator chain at startup tests                                    |
|   [9]   | `GetRequiredDecoratedService` / `GetDecoratedServices`    | Resolve inner layer or full decoration stack — v7                           |

### Filter algebra

Chained class filters (`AssignableTo<A>().AssignableTo<B>()`) intersect (AND). `AssignableToAny(typeof(A), typeof(B))` unions (OR). Multiple `AddClasses(...)` blocks in one scan union candidate sets — not intersection. `InNamespaces` prefix-matches; `InExactNamespaces` exact-matches. `WithLifetime(Func<Type, ServiceLifetime>)` resolves lifetime per type without splitting scans.

### Decorator topology

Resolution reads bottom-up: the **last** `Decorate` call is **outermost**. Typical ordering (inner → outer): concrete implementation → retry → auth/validation → tracing/logging.

| [INDEX] | [LAYER]       | [ROLE]                                                          |
| :-----: | :------------ | :-------------------------------------------------------------- |
|   [1]   | Core          | Concrete `Eff<RT,T>` or sync service                            |
|   [2]   | Retry         | `Prelude.retry(schedule, …)` + `Map`/`MapFail` inside decorator |
|   [3]   | Policy        | Validation, authorization, caching                              |
|   [4]   | Observability | Tracing, metrics, structured logging shell                      |

`Decorate` throws `DecorationException` when no match; `TryDecorate` returns `false` for optional modules. Internal decoration uses `+Decorated` string keys — distinct from user `WithServiceKey` keys.

### High-value patterns

1. **Multi-assembly handler discovery** — `FromDependencyContext(context, predicate)` with bounded assembly name prefix, `AssignableToAny(...)`, `AsImplementedInterfaces()`, `Throw`. New assemblies register without editing a central list.
2. **`Eff` decorators** — register concrete pipeline, then `Decorate<IThing, RetryThingDecorator>()`, then `Decorate<IThing, TracingThingDecorator>()`. Compose with `Map`/`MapFail`/`Prelude.retry(schedule, eff)` — never `Match` and re-lift.
3. **Keyed backends** — `WithServiceKey` or `[ServiceDescriptor(..., serviceKey)]` for `IStorage`, `ICache`, `IQueue` variants. Assert every key in the vocabulary resolves at startup. **Public `Decorate<T>()` only wraps non-keyed registrations** (`ServiceKey == null`); enum/object keyed services need a manual decorate fold per key.
4. **`TryDecorate` for optional modules** — telemetry or audit in a separate assembly; core host builds when the assembly is absent.
5. **Hosted-service scope** — `IHostedService` + `CreateAsyncScope()` + runtime record wrapping `IServiceProvider`. Scrutor registers boot/drain `Eff` programs; scope lifetime matches async scope disposal.

### Keyed + Throw footgun

`RegistrationStrategy.Throw` checks duplicate `ServiceType` **without** considering `ServiceKey`. Multiple keyed registrations of the same interface throw on the second key. Use `Append`, separate scan blocks, or `Skip` for keyed multi-registration.

### Testing

```csharp
services.Decorate<ICommandBus, TracingCommandBus>(out DecoratedService<ICommandBus> handle);
// handle.UnderlyingInstance — inner before this Decorate step
// provider.GetRequiredDecoratedService<ICommandBus>() — same
// provider.GetDecoratedServices<ICommandBus>() — all layers
```

### When Scrutor adds little

- One implementation per interface maintained by hand (≤5 registrations).
- In-process plugins using static factory entry points only.
- Domain, analysis, or geometry modules.

### Rules

- Always `.UsingRegistrationStrategy(...)` on every `Scan`.
- Prefer `FromDependencyContext` + assembly predicate over unfiltered `FromApplicationDependencies()`.
- `ValidateOnBuild` / `ValidateScopes` on development hosts.
- Open generic service registrations are **excluded** from `ValidateOnBuild`.
- Type-based and factory-based decoration both participate in container validation in v7 — prefer type-based when sufficient.
- Optional niche: **ServiceScan.SourceGenerator** (**not-in-graph** until pinned) for AOT/trim-friendly compile-time registration instead of runtime scan.
- `UsingAttributes()` + `[ServiceDescriptor<...>(..., serviceKey:)]` for attribute-driven registration.
- Multiple `AddClasses(...)` blocks in one scan **union** candidate sets; chained filters within a block intersect (AND).
- `TryDecorate` on enum/object keyed services silently no-ops when keys are non-`string` — use manual per-key decorate folds.

---
## [3][FLUENTVALIDATION]
>**Dictum:** *FluentValidation validates DTOs at the boundary; domain stays on `Validation<Error,T>`.*

<br>

| [INDEX] | [LAYER]                              | [PACKAGE]                        |
| :-----: | :----------------------------------- | :------------------------------- |
|   [1]   | HTTP/API, import config, admin forms | FluentValidation 11.x            |
|   [2]   | Domain rules, aggregates             | `Validation<Error,T>` / `Fin<T>` |

Bridge: `await validator.ValidateAsync(dto, ct)` → fold `ValidationFailure` entries to `Validation<Error,T>` via property path + `Error.New` before domain modules. Use `IncludeRuleSets("Import")` at HTTP/import boundaries. Use `ValidateAsync` when rules include `MustAsync` / `CustomAsync`. Never import FluentValidation into domain namespaces (CSP0402); prefer `ValidateAsync` over sync `Validate` at boundaries (CSP0403).

```csharp
var result = await validator.ValidateAsync(dto, ct);
var validated = result.Errors.Fold(
    Validation<Error, Dto>.Success(dto),
    (acc, f) => acc.Bind(_ => fail<Error, Dto>(Error.New(f.PropertyName, f.ErrorMessage))));
```

**Low value:** in-process command flows with domain `Requirement` / `Op` acceptance only.

---
## [4][NODATIME]
>**Dictum:** *Inject `IClock`; persist and compare with `Instant` — convert `DateTime` only at boundaries.*

<br>

| [INDEX] | [USE]                                | [API]                                       |
| :-----: | :----------------------------------- | :------------------------------------------ |
|   [1]   | Domain timestamps, audit, scheduling | `Instant`, `IClock`                         |
|   [2]   | User time zones, calendars           | `ZonedDateTime`, `DateTimeZone` at boundary |
|   [3]   | Native SDK interop (`DateTime`)      | Single adapter in `// BOUNDARY ADAPTER`     |

**With EF:** `UseNodaTime()` on Npgsql provider when pinned — prefer native Npgsql NodaTime mapping over manual `InstantConverter` except for custom columns. Manual `ValueConverter<Instant, DateTime>` remains valid for non-Npgsql providers or JSON columns.

---
## [5][EF_CORE]
>**Dictum:** *EF is an effectful shell around a relational kernel — not an in-process algorithm hot path.*

<br>

| [INDEX] | [FIT]                                                         | [PATTERN]                            |
| :-----: | :------------------------------------------------------------ | :----------------------------------- |
|   [1]   | Catalogue, audit log, licensing DB, desktop companion service | `Eff<RT,T>` + `AppDbContext` on `RT` |
|   [2]   | In-process plugin command/solve paths                         | [NEVER] `DbContext` in hot paths     |

Use `IQueryable` until boundary materialization; Thinktecture value objects via `ValueConverter` / `OwnsOne`; sealed DUs as owned types or string discriminators. When `Thinktecture.EntityFrameworkCore` is pinned: `modelBuilder.UseThinktectureValueConverters()` at boundary; manual converters for DUs, discriminators, and JSON columns only.

```csharp
options.UseNpgsql(connection, o => o.UseNodaTime().EnableRetryOnFailure());
// modelBuilder.UseThinktectureValueConverters() when integration package pinned
```

Repositories as operation algebras, not method-per-entity sprawl. `EnableRetryOnFailure` handles transient DB errors at the **persistence boundary** — distinct from LanguageExt `Schedule` on domain `Eff`.

| [INDEX] | [RETRY_OWNER]         | [MECHANISM]                                          | [SCOPE]                    |
| :-----: | :-------------------- | :--------------------------------------------------- | :------------------------- |
|   [1]   | Domain business rules | `Prelude.retry(Schedule, eff)` in Scrutor decorators | In-process `Eff<RT,T>`     |
|   [2]   | DB transient faults   | `EnableRetryOnFailure` on Npgsql/EF                  | Persistence boundary       |
|   [3]   | Outbound HTTP         | `AddStandardResilienceHandler`                       | Typed `HttpClient` at root |

One retry owner per hop — do not stack domain `Schedule` and HTTP resilience on the same operation.

Keep host-free domain libs separate from `*.Persistence` projects that reference EF.

---
## [6][SERILOG_OPENTELEMETRY]
>**Dictum:** *Structured logs and traces belong to the host process, not domain transforms.*

<br>

| [INDEX] | [CONCERN]          | [SURFACE]                                              |
| :-----: | :----------------- | :----------------------------------------------------- |
|   [1]   | Structured logging | Serilog 4.x + `[LoggerMessage]` or `ILogger`           |
|   [2]   | Traces / metrics   | `ActivitySource`, `Meter`, OpenTelemetry 1.x exporters |
|   [3]   | Correlation        | `LogContext`, trace id on `Activity`                   |

Register exporters and enrichers in composition root (`Serilog.Extensions.Hosting` when host lands). Put `ActivitySource` / `Meter` on the runtime record when `Eff<RT,T>` pipelines emit telemetry. For EF hosts: OpenTelemetry `AddNpgsql()` correlates DB spans with HTTP and domain traces when packages are pinned.

Scrutor may register instrumentation decorators at bootstrap.

---
## [7][HTTP_RESILIENCE]
>**Dictum:** *Outbound HTTP resilience is a named handler at the root — not domain `Schedule`.*

<br>

`Microsoft.Extensions.Http.Resilience` — `AddStandardResilienceHandler` on typed `HttpClient` registrations. Use for external APIs (licensing, cloud storage, webhooks).

Domain retry/backoff for business rules remains LanguageExt `Schedule` on `Eff` inside decorators — not raw Polly in domain. Wire `OnRetry` with resilience context logger attachment before pipeline execution; map attempts through structured retry telemetry at the host boundary.

**Out of scope** for offline desktop plugins with no outbound HTTP.

---
## [8][ADOPTION_ORDER]
>**Dictum:** *Add packages only with a concrete host consumer.*

<br>

| [INDEX] | [PACKAGE]               | [TRIGGER]                               |
| :-----: | :---------------------- | :-------------------------------------- |
|   [1]   | Scrutor 7.0.0           | Bootstrap project exists                |
|   [2]   | NodaTime + `IClock`     | Persisted time or scheduled jobs        |
|   [3]   | FluentValidation        | External DTO/config validation boundary |
|   [4]   | EF Core                 | Relational store owner identified       |
|   [5]   | Serilog / OpenTelemetry | Centralized logging/tracing host        |
|   [6]   | Http.Resilience         | Typed `HttpClient` to external services |

Core numerics (`MathNet`, `CSparse`) and FP stack (`LanguageExt`, `Thinktecture`) remain domain-first.
