# [RASM_API_OTEL_INSTRUMENTATION_ENTITYFRAMEWORKCORE]

`OpenTelemetry.Instrumentation.EntityFrameworkCore` is the ORM-layer db-semconv span owner for the relational tier: it subscribes EF Core's `"Microsoft.EntityFrameworkCore"` `DiagnosticSource` and converts each command lifecycle event into a `Client`-kind span named `OpenTelemetry.Instrumentation.EntityFrameworkCore.Execute`. Its single `ActivitySource` shares the package name, admitted at the root like every foreign source. Direction of the layer split is load-bearing — this package sees the LINQ-translated command as EF issues it, while the held ADO-layer `Npgsql.OpenTelemetry` sees the same statement at the driver wire; both spans nest under one operation, so the pair is complementary, never redundant. Trace-only — no `Meter` ships, so relational operation-duration and pool instruments stay the `Npgsql` meter's roster.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `OpenTelemetry.Instrumentation.EntityFrameworkCore`
- package: `OpenTelemetry.Instrumentation.EntityFrameworkCore`
- assembly: `OpenTelemetry.Instrumentation.EntityFrameworkCore`
- namespace: `OpenTelemetry.Trace`, `OpenTelemetry.Instrumentation.EntityFrameworkCore`
- driver source: `Microsoft.EntityFrameworkCore` — EF Core's built-in `DiagnosticSource` emitting the command lifecycle this package hooks
- asset: runtime library
- rail: storage instrumentation

## [02]-[PUBLIC_TYPES]

[OPTION_TYPES]: trace shaping and command enrichment
- rail: storage instrumentation

| [INDEX] | [SYMBOL]                                | [KIND]          | [CAPABILITY]                                      |
| :-----: | :-------------------------------------- | :-------------- | :------------------------------------------------ |
|  [01]   | `EntityFrameworkInstrumentationOptions` | options carrier | command filter, span enrichment over `IDbCommand` |

`EntityFrameworkInstrumentationOptions` carries `Func<string?, IDbCommand, bool>? Filter` — receives the EF event name and the live `IDbCommand`, returns false to drop a command before its span mints — and `Action<Activity, IDbCommand>? EnrichWithIDbCommand`, invoked on the minted span with the executing command for tag augmentation. Old-versus-new database attribute emission is not a public knob: internal `EmitOldAttributes`/`EmitNewAttributes` resolve from the `OTEL_SEMCONV_STABILITY_OPT_IN` configuration opt-in, so semconv posture rides host configuration, never options properties.

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: admission (`TracerProviderBuilderExtensions`)
- rail: storage instrumentation

| [INDEX] | [SURFACE]                               | [KIND]          | [CAPABILITY]                                                   |
| :-----: | :-------------------------------------- | :-------------- | :------------------------------------------------------------- |
|  [01]   | `AddEntityFrameworkCoreInstrumentation` | trace admission | source subscribe + optional named-options and options-delegate |

`AddEntityFrameworkCoreInstrumentation(TracerProviderBuilder)` subscribes the source alone; overloads bind an `Action<EntityFrameworkInstrumentationOptions>? configure` and a `(string? name, Action<EntityFrameworkInstrumentationOptions>? configure)` pair that keys options through the named `IOptionsMonitor` slot, so two EF contexts under one provider carry disjoint filter/enrich policy.

## [04]-[IMPLEMENTATION_LAW]

[EF_TOPOLOGY]:
- subscription root: `AddEntityFrameworkCoreInstrumentation` registers at the AppHost composition root; the EF Core dependency stays Persistence-owned, and the root reaches only the `OpenTelemetry.Instrumentation.EntityFrameworkCore` source name
- source binding: a `DiagnosticSourceSubscriber` filters `DiagnosticListener.Name == "Microsoft.EntityFrameworkCore"` and handles `CommandCreated`, `CommandExecuting`, `CommandExecuted`, `CommandError` — the command span opens on execution and closes on executed/error
- span shape: `Client`-kind, name `OpenTelemetry.Instrumentation.EntityFrameworkCore.Execute`, db-semconv tags governed by the semconv opt-in

[STACKING]:
- `Npgsql.OpenTelemetry`(`api-npgsql-opentelemetry.md`): ORM-layer complement — EF command spans nest over the ADO-layer Npgsql command spans; both admitted at the root, the pair partitions by layer, and the `Npgsql` meter owns the relational instrument roster this trace-only package never mints.
- `OpenTelemetry`(`api-opentelemetry.md`): `AddEntityFrameworkCoreInstrumentation` is the source subscription with the diagnostic-listener hook; a bare `AddSource("OpenTelemetry.Instrumentation.EntityFrameworkCore")` subscribes the name but never installs the subscriber, so the convenience verb is the only complete path — never a bare `AddSource` shim.
- `Microsoft.EntityFrameworkCore`: instrumentation reads the provider's own diagnostic emission; context configuration and provider selection stay EF-owned rows, and the hook adds no EF vocabulary.

[LOCAL_ADMISSION]:
- Composition-root-only, at the AppHost root that owns the EF contexts; the named overload scopes filter/enrich policy per `DbContext` where one provider hosts several.
- `Filter` and `EnrichWithIDbCommand` run on the command hot path — both stay unset on high-fanout query lanes unless the drop or tag earns the per-command delegate cost.

[RAIL_LAW]:
- Package: `OpenTelemetry.Instrumentation.EntityFrameworkCore`
- Owns: EF Core command spans and the `Microsoft.EntityFrameworkCore` diagnostic-source hook at the composition root
- Accept: `AddEntityFrameworkCoreInstrumentation` admission beside `AddNpgsql`, named-options scoping per context
- Reject: hand-rolled db-semconv spans over EF command interceptors; a bare `AddSource` that subscribes the name without the diagnostic-listener hook
