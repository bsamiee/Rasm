# [RASM_PERSISTENCE_API_EF_DESIGN]

`Microsoft.EntityFrameworkCore.Design` is the EF Core design-time tooling assembly: the
`OperationExecutor` reflection-dispatch surface that `dotnet ef`/`ef.exe` invoke, the
`MigrationsOperations`/`DbContextOperations` operation drivers, the `MigrationsScaffolder`
and `ReverseEngineerScaffolder` generators, the C# code generators (`CSharpMigrationsGenerator`,
`CSharpMigrationOperationGenerator`, `CSharpSnapshotGenerator`, `CSharpHelper`), the
`ScaffoldedMigration`/`ScaffoldedModel`/`ScaffoldedFile` outputs, and the compiled-model
(`Optimize`) generator. It is a private, develop-and-build-time-only asset: it is referenced
with `PrivateAssets="all"` and never flows as a runtime dependency. Most of its operational
surface is shipped under `*.Internal` namespaces and carries `[EntityFrameworkInternal]`, so a
design page that drives it programmatically (rather than through the `dotnet ef` CLI) takes an
explicit dependency on EF Core's unstable internal API and must pin the EF minor version. The
Persistence `Schema/migration` rail consumes it for one purpose: generate migrations, compiled
models, and idempotent SQL scripts as reviewed artifacts — never to reverse-engineer a store
into the model, which is the inverted, rejected direction.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Microsoft.EntityFrameworkCore.Design`
- package: `Microsoft.EntityFrameworkCore.Design`
- version: `10.0.9`
- license: `MIT` (`requireLicenseAcceptance=true`)
- assembly: `Microsoft.EntityFrameworkCore.Design` (`lib/net10.0`; single-TFM, matches the consumer floor)
- namespace: `Microsoft.EntityFrameworkCore.Design`, `Microsoft.EntityFrameworkCore.Design.Internal`, `Microsoft.EntityFrameworkCore.Migrations.Design`, `Microsoft.EntityFrameworkCore.Scaffolding`, `Microsoft.EntityFrameworkCore.Scaffolding.Internal`
- asset: design-time/build-time tool library (`PrivateAssets=all`); never a runtime dependency
- abi: most operation drivers ship `public` but `*.Internal`-namespaced and `[EntityFrameworkInternal]` — unstable across EF minors
- rail: schema-tooling

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: tool dispatch and operation drivers
- rail: schema-tooling

| [INDEX] | [SYMBOL]                                  | [PACKAGE_ROLE]       | [CAPABILITY]                                                  |
| :-----: | :---------------------------------------- | :------------------- | :----------------------------------------------------------- |
|  [01]   | `OperationExecutor`                        | tool executor        | `Design`; reflection entrypoint `dotnet ef` drives via nested `OperationBase` |
|  [02]   | `OperationExecutor.OperationBase`          | operation marker     | `Design`; per-verb nested op (`AddMigration`, `OptimizeContext`, …) |
|  [03]   | `MigrationsOperations`                     | migration driver     | `Design.Internal` `[EntityFrameworkInternal]`; add/remove/script/list |
|  [04]   | `DbContextOperations`                      | context driver       | `Design.Internal` `[EntityFrameworkInternal]`; optimize/scaffold/pending |
|  [05]   | `DesignTimeServicesBuilder`                | service builder      | `Design.Internal` `[EntityFrameworkInternal]`; composes the design `IServiceProvider` |
|  [06]   | `DesignTimeServiceCollectionExtensions`    | service extension    | `Design`; `AddEntityFrameworkDesignTimeServices` / `AddDbContextDesignTimeServices` |
|  [07]   | `IOperationReporter` / `OperationReporter`  | reporter             | `Design` (iface) + `Design.Internal` (impl); emits error/warn/info/verbose |
|  [08]   | `OperationException`                       | tool failure         | `Design`; design-time operation failure                      |
|  [09]   | `DbContextActivator`                       | context probe        | `Design`; instantiates a `DbContext` for design-time use     |

[PUBLIC_TYPE_SCOPE]: migration and model scaffolding outputs
- rail: schema-tooling

| [INDEX] | [SYMBOL]                          | [PACKAGE_ROLE]     | [CAPABILITY]                                             |
| :-----: | :-------------------------------- | :----------------- | :------------------------------------------------------- |
|  [01]   | `IMigrationsScaffolder` / `MigrationsScaffolder` | migration tool | `Migrations.Design`; scaffolds + saves + removes migrations |
|  [02]   | `IReverseEngineerScaffolder`      | model tool         | `Scaffolding`; DB-first model scaffold (rejected direction) |
|  [03]   | `ScaffoldedMigration`             | scaffold output    | `Migrations.Design`; migration + designer + snapshot files |
|  [04]   | `ScaffoldedModel`                 | scaffold output    | `Scaffolding`; context + entity-type files               |
|  [05]   | `ScaffoldedFile(string path, string code)` | file output | `Scaffolding`; one generated file (path + code)          |
|  [06]   | `MigrationFiles` / `SavedModelFiles` | file set         | `Migrations.Design` / `Scaffolding`; emitted file paths   |
|  [07]   | `ModelCodeGenerationOptions`      | model gen policy   | `Scaffolding`; reverse-engineer output options           |
|  [08]   | `ModelReverseEngineerOptions`     | reverse policy     | `Scaffolding`; store-inspection options                  |
|  [09]   | `CompiledModelCodeGenerationOptions` | compiled policy | `Scaffolding`; `Optimize` compiled-model output options   |
|  [10]   | `IMigrationsCodeGeneratorSelector` | generator selector | `Migrations.Design`; selects the migrations code generator |

[PUBLIC_TYPE_SCOPE]: C# generation surfaces and bundle
- rail: schema-tooling

| [INDEX] | [SYMBOL]                            | [PACKAGE_ROLE] | [CAPABILITY]                                          |
| :-----: | :---------------------------------- | :------------- | :---------------------------------------------------- |
|  [01]   | `ICSharpHelper` / `CSharpHelper`     | C# helper      | `Design` (iface) + `Design.Internal` (impl); emits C# literals/fragments |
|  [02]   | `CSharpMigrationsGenerator`          | C# generator   | `Migrations.Design`; full migration + designer + snapshot |
|  [03]   | `CSharpMigrationOperationGenerator`  | C# generator   | `Migrations.Design`; emits `MigrationOperation` Up/Down code |
|  [04]   | `CSharpSnapshotGenerator`            | C# generator   | `Migrations.Design`; emits the model snapshot          |
|  [05]   | `MigrationsCodeGenerator` / `IMigrationsCodeGenerator` | C# generator base | `Migrations.Design`; the language-agnostic generator contract |
|  [06]   | `CSharpDbContextGeneratorBase` / `CSharpEntityTypeGeneratorBase` | C# generator | `Scaffolding.Internal` `[EntityFrameworkInternal]`; reverse-engineer code |
|  [07]   | `MigrationsBundle`                   | bundle tool    | `Migrations.Design`; static `Execute(...)` self-contained migrator EXE entry |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: design-service registration and operation dispatch
- rail: schema-tooling

| [INDEX] | [SURFACE]                                                        | [CALL_SHAPE]   | [CAPABILITY]                                          |
| :-----: | :--------------------------------------------------------------- | :------------- | :---------------------------------------------------- |
|  [01]   | `services.AddEntityFrameworkDesignTimeServices(reporter?, accessor?)` | service call | registers the EF design-time service graph            |
|  [02]   | `services.AddDbContextDesignTimeServices(DbContext)`            | service call   | adds a context's own design-time services             |
|  [03]   | `new DesignTimeServicesBuilder(...).Build(context)`             | service build  | builds the design `IServiceProvider` (internal API)   |
|  [04]   | `new OperationExecutor(IOperationReportHandler, IDictionary args)` | tool ctor   | the reflection-driven executor `dotnet ef` constructs |
|  [05]   | `DbContextActivator.CreateInstance(contextType, ...)`          | context probe  | instantiates a `DbContext` from a design assembly     |

[ENTRYPOINT_SCOPE]: migration operations (`MigrationsOperations`, internal API)
- rail: schema-tooling

| [INDEX] | [SURFACE]                                                                 | [CALL_SHAPE]   | [CAPABILITY]                                |
| :-----: | :------------------------------------------------------------------------ | :------------- | :------------------------------------------ |
|  [01]   | `AddMigration(name, outputDir, contextType, namespace, dryRun)`          | tool operation | scaffolds a migration → `MigrationFiles`    |
|  [02]   | `RemoveMigration(contextType, force, dryRun)`                            | tool operation | removes the last migration → `MigrationFiles` |
|  [03]   | `ScriptMigration(fromMigration, toMigration, MigrationsSqlGenerationOptions, contextType)` | tool operation | emits idempotent SQL between two migrations |
|  [04]   | `GetMigrations(contextType, connectionString, noConnect)`               | tool operation | lists migrations → `IEnumerable<MigrationInfo>` |
|  [05]   | `UpdateDatabase(targetMigration, connectionString, contextType)`        | tool operation | applies migrations (service deploys use scripts/bundle instead) |
|  [06]   | `MigrationsScaffolder.ScaffoldMigration(name, rootNamespace, subNamespace, language, dryRun)` | scaffold | the underlying scaffold → `ScaffoldedMigration` |

[ENTRYPOINT_SCOPE]: context operations (`DbContextOperations`, internal API)
- rail: schema-tooling

| [INDEX] | [SURFACE]                                                                          | [CALL_SHAPE]   | [CAPABILITY]                                |
| :-----: | :--------------------------------------------------------------------------------- | :------------- | :------------------------------------------ |
|  [01]   | `Optimize(outputDir, modelNamespace, contextTypeName, suffix, scaffoldModel, precompileQueries, nativeAot)` | tool operation | compiled-model + precompiled-query generation → `IReadOnlyList<string>` files (the `dotnet ef dbcontext optimize` verb; nested executor class `OperationExecutor.OptimizeContext`) |
|  [02]   | `ScaffoldContext(provider, connectionString, ...)`                                 | tool operation | DB-first context scaffold (rejected direction) |
|  [03]   | `HasPendingModelChanges(contextType)`                                              | tool operation | true if the model has drifted past the last migration |
|  [04]   | `GetContextInfo(contextType)` / `CreateContext(contextType)`                       | tool operation | context metadata / a live design-time `DbContext` |
|  [05]   | `MigrationsBundle.Execute(context, assembly, startupAssembly, args)`              | bundle entry   | the `efbundle` self-contained migrator EXE `Main` |

## [04]-[IMPLEMENTATION_LAW]

[TOOL_ADMISSION]:
- dependency role: design-time/build-time tool-only package, referenced `PrivateAssets="all"`; it must never appear in a runtime dependency closure or a published output.
- ABI posture: the operational drivers (`MigrationsOperations`, `DbContextOperations`, `DesignTimeServicesBuilder`, `OperationReporter`, `CSharpHelper`, `CSharpDbContextGeneratorBase`, `CSharpEntityTypeGeneratorBase`) are `public` but live under `*.Internal` namespaces and carry `[EntityFrameworkInternal]`. Driving them in process is supported but unstable across EF minor versions, so the `Microsoft.EntityFrameworkCore.Design` version is pinned to the EF minor the rest of the stack uses. The stable, version-tolerant entry is the `dotnet ef` CLI / `OperationExecutor` reflection surface and the `MigrationsBundle.Execute` EXE entry.
- output role: `ScaffoldedMigration`/`ScaffoldedModel`/`ScaffoldedFile` are reviewed as generated shape before they enter source; generation is provider-aware (one model emits per-provider SQL) and vocabulary-neutral.
- direction: the model is the source of truth. `ReverseEngineerScaffolder`/`ScaffoldContext`/`ModelReverseEngineerOptions`/`CSharp*GeneratorBase` invert the flow (store → model) and are the rejected direction — a reverse-engineered context is the named defect.

[STACK_INTEGRATION]:
- the `Schema/migration` `MigrationLaw` rail composes this package as the migration emission and gating substrate: the design-time `Optimize` (`DbContextOperations.Optimize`, CLI `dbcontext optimize`), `ScriptMigration`, `MigrationsBundle.Execute`, and `GetMigrations` own emission and packaging, so hand-authored migration code and custom `MigrationOperation` subclasses are deleted patterns.
- compiled-model adoption stacks settled with the converter rail: `Optimize` freezes the model into a generated compiled model that `ConverterRail.Compose(options, compiled)` mounts through `UseModel`; the snake-case naming rewrites survive the freeze, so a compiled model and a fresh model emit identical column names and migration SQL. `CompiledModelCodeGenerationOptions` is the compiled-output policy.
- `MigrationsCodeGeneratorSelector` (`IMigrationsCodeGeneratorSelector`) is the one seam that swaps emission language/generator without a hand-written generator class; `CSharpMigrationsGenerator`/`CSharpMigrationOperationGenerator`/`CSharpSnapshotGenerator` are the default C# arm. `ScriptMigration(..., MigrationsSqlGenerationOptions, ...)` with the idempotent option produces the deploy-time SQL the service profile applies; `MigrationsBundle.Execute` produces the self-contained `efbundle` migrator the deploy can run without the SDK.
- the `Schema/migration` `Classify` fold runs at generation time over the `MigrationOperation` rows the C# generators emit (`AddColumnOperation`, `RenameColumnOperation`, `AlterColumnOperation`, `DropColumnOperation`, …), splitting every change into expand and contract waves; this package supplies the operation vocabulary, the migration assembly, and the per-provider SQL generators, while the wave classification, lock-light `NOT VALID`/`NOT ENFORCED` emission, and destructive-approval gating stay the Persistence owner's.

[LOCAL_ADMISSION]:
- design-time services support store-profile schema work and never become runtime dependencies.
- scaffolding output is reviewed as generated shape before it enters source; migration generation is provider-aware and vocabulary-neutral.
- reverse engineering is an implementation aid only, never a public store contract; the model-first direction is enforced.
- in-process driving of `*.Internal` `[EntityFrameworkInternal]` types is admitted only with the EF version pinned to the consumed minor.

[RAIL_LAW]:
- Package: `Microsoft.EntityFrameworkCore.Design`
- Owns: EF design-time schema tooling — migration scaffolding, compiled-model (`Optimize`) generation, idempotent SQL scripting, and migration bundling
- Accept: a private (`PrivateAssets=all`) tool asset; emission through `OperationExecutor`/CLI, `MigrationsOperations`/`DbContextOperations`, `ScriptMigration`, `Optimize`, `MigrationsBundle`, `MigrationsCodeGeneratorSelector`; EF version pinned when internal-API types are driven in process
- Reject: a runtime dependency on the design assembly, DB-first reverse engineering as the store contract, hand-authored migration code, or unpinned in-process use of `[EntityFrameworkInternal]` drivers
