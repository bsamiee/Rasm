# [RASM_API_OTEL_INSTRUMENTATION_ENTITYFRAMEWORKCORE]

`OpenTelemetry.Instrumentation.EntityFrameworkCore` mints the ORM-layer db-semconv span: it subscribes EF Core's `Microsoft.EntityFrameworkCore` `DiagnosticSource` and folds each command lifecycle event onto one `Client`-kind span named `OpenTelemetry.Instrumentation.EntityFrameworkCore.Execute`, its `ActivitySource` name matching the package.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `OpenTelemetry.Instrumentation.EntityFrameworkCore`
- package: `OpenTelemetry.Instrumentation.EntityFrameworkCore`
- assembly: `OpenTelemetry.Instrumentation.EntityFrameworkCore`
- namespace: `OpenTelemetry.Trace`, `OpenTelemetry.Instrumentation.EntityFrameworkCore`
- rail: storage instrumentation

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: options carrier with its delegate slots

| [INDEX] | [SYMBOL]                                     | [TYPE_FAMILY] | [CAPABILITY]                  |
| :-----: | :------------------------------------------- | :------------ | :---------------------------- |
|  [01]   | `EntityFrameworkInstrumentationOptions`      | class         | per-registration span policy  |
|  [02]   | `Filter(string, IDbCommand) -> bool`         | delegate      | provider-name command gate    |
|  [03]   | `EnrichWithIDbCommand(Activity, IDbCommand)` | delegate      | span tagging from the command |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: `TracerProviderBuilderExtensions` over `TracerProviderBuilder`, every overload folding to the named-slot leg

| [INDEX] | [SURFACE]                                                                                      | [SHAPE] | [CAPABILITY]          |
| :-----: | :--------------------------------------------------------------------------------------------- | :------ | :-------------------- |
|  [01]   | `AddEntityFrameworkCoreInstrumentation()`                                                      | static  | bare source subscribe |
|  [02]   | `AddEntityFrameworkCoreInstrumentation(Action<EntityFrameworkInstrumentationOptions>)`         | static  | default options slot  |
|  [03]   | `AddEntityFrameworkCoreInstrumentation(string, Action<EntityFrameworkInstrumentationOptions>)` | static  | named options slot    |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- command span: `CommandCreated` starts the `Client` span and names it after the database, `CommandExecuted` and `CommandError` stop it, and an error stamps `ActivityStatusCode.Error` from the payload exception.
- executing seat: `CommandExecuting` runs `Filter`, then semconv tagging, then `EnrichWithIDbCommand`; a false or throwing filter un-records the started span, and a throwing enrich callback lands on the instrumentation event source.
- semconv posture: `OTEL_SEMCONV_STABILITY_OPT_IN` selects both the tag family and the `ActivitySource` instance stamping its schema URL — unset emits `db.statement` and `db.name`, `database` emits `db.query.text`, `db.query.summary`, and `db.namespace`, `database/dup` emits both.
- statement shaping: text commands land sanitized with `db.query.summary` overwriting the span display name, stored procedures land as `db.operation.name` `EXECUTE` beside `db.stored_procedure.name`, and `OTEL_DOTNET_EXPERIMENTAL_EFCORE_ENABLE_TRACE_DB_QUERY_PARAMETERS` adds raw `db.query.parameter.{key}` tags under the new family.

[STACKING]:
- `Npgsql.OpenTelemetry`(`api-npgsql-opentelemetry.md`): EF command spans nest over the ADO-layer `Npgsql` spans, so one statement carries an ORM span and a wire span; the `Npgsql` meter owns every relational instrument this trace-only package never mints.
- `OpenTelemetry`(`api-opentelemetry.md`): `AddEntityFrameworkCoreInstrumentation` calls `AddSource` on the package name and registers the subscriber that name alone never installs.
- Persistence contexts on the AppHost root: one named registration per `DbContext` pairs its EF span policy with the `AddNpgsql` wire lane, so a per-context `Filter` drops migration and health-probe commands while the driver lane keeps its connection instruments.

[LOCAL_ADMISSION]:
- Composition-root-only, at the AppHost root that owns the EF contexts; that root reaches the source name alone, never the EF assembly Persistence carries.
- `Filter` and `EnrichWithIDbCommand` run on the command hot path, so both stay unset on high-fanout query lanes unless the drop or tag earns the per-command delegate cost.

[RAIL_LAW]:
- Package: `OpenTelemetry.Instrumentation.EntityFrameworkCore`
- Owns: EF Core command spans and the `Microsoft.EntityFrameworkCore` diagnostic-source subscription at the composition root
- Accept: `AddEntityFrameworkCoreInstrumentation` beside `AddNpgsql`, named options scoping policy per context
- Reject: hand-rolled db-semconv spans over EF command interceptors; a bare `AddSource` naming the source without installing the subscriber
