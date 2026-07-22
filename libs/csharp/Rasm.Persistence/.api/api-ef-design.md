# [RASM_PERSISTENCE_API_EF_DESIGN]

`Microsoft.EntityFrameworkCore.Design` owns EF Core's design-time schema tooling — migration scaffolding, compiled-model generation, and idempotent SQL scripting — driven through the `OperationExecutor` reflection surface the `dotnet ef` CLI constructs. It enters as a private develop-and-build asset under `PrivateAssets=all`, never a runtime dependency. `Element/identity` consumes it to emit migrations, compiled models, and idempotent SQL as reviewed artifacts; store-to-model reverse engineering is the inverted, rejected direction.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Microsoft.EntityFrameworkCore.Design`
- package: `Microsoft.EntityFrameworkCore.Design` (`MIT`, Microsoft)
- assembly: `Microsoft.EntityFrameworkCore.Design` (`lib/net10.0`, single-TFM at the consumer floor)
- namespace: `Microsoft.EntityFrameworkCore.Design`, `Microsoft.EntityFrameworkCore.Design.Internal`, `Microsoft.EntityFrameworkCore.Migrations.Design`, `Microsoft.EntityFrameworkCore.Scaffolding`, `Microsoft.EntityFrameworkCore.Scaffolding.Internal`
- role: design-time/build-time tool asset (`PrivateAssets=all`)
- abi: operation drivers ship `public` but `*.Internal`-namespaced and `[EntityFrameworkInternal]`, unstable across EF minors
- rail: schema-tooling

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: tool dispatch and operation drivers

| [INDEX] | [SYMBOL]                                   | [TYPE_FAMILY]    | [CAPABILITY]                                                           |
| :-----: | :----------------------------------------- | :--------------- | :--------------------------------------------------------------------- |
|  [01]   | `OperationExecutor`                        | class            | `Design`; reflection entrypoint `dotnet ef` drives via `OperationBase` |
|  [02]   | `OperationExecutor.OperationBase`          | class            | `Design`; per-verb nested op (`AddMigration`, `OptimizeContext`, …)    |
|  [03]   | `MigrationsOperations`                     | class            | `Design.Internal`; add/remove/script/list                              |
|  [04]   | `DbContextOperations`                      | class            | `Design.Internal`; optimize/scaffold/pending                           |
|  [05]   | `DesignTimeServicesBuilder`                | class            | `Design.Internal`; composes the design `IServiceProvider`              |
|  [06]   | `DesignTimeServiceCollectionExtensions`    | static class     | `Design`; the two `Add*DesignTimeServices` registrars                  |
|  [07]   | `IOperationReporter` / `OperationReporter` | interface, class | `Design` + `Design.Internal`; error/warn/info/verbose                  |
|  [08]   | `OperationException`                       | class            | `Design`; design-time operation failure                                |
|  [09]   | `DbContextActivator`                       | static class     | `Design`; instantiates a `DbContext` for design-time use               |

[PUBLIC_TYPE_SCOPE]: migration and model scaffolding outputs

| [INDEX] | [SYMBOL]                                         | [TYPE_FAMILY]    | [CAPABILITY]                                                |
| :-----: | :----------------------------------------------- | :--------------- | :---------------------------------------------------------- |
|  [01]   | `IMigrationsScaffolder` / `MigrationsScaffolder` | interface, class | `Migrations.Design`; scaffolds + saves + removes migrations |
|  [02]   | `IReverseEngineerScaffolder`                     | interface        | `Scaffolding`; DB-first model scaffold (rejected direction) |
|  [03]   | `ScaffoldedMigration`                            | class            | `Migrations.Design`; migration + designer + snapshot files  |
|  [04]   | `ScaffoldedModel`                                | class            | `Scaffolding`; context + entity-type files                  |
|  [05]   | `ScaffoldedFile(string path, string code)`       | class            | `Scaffolding`; one generated file (path + code)             |
|  [06]   | `MigrationFiles` / `SavedModelFiles`             | class            | `Migrations.Design` / `Scaffolding`; emitted file paths     |
|  [07]   | `ModelCodeGenerationOptions`                     | class            | `Scaffolding`; reverse-engineer output options              |
|  [08]   | `ModelReverseEngineerOptions`                    | class            | `Scaffolding`; store-inspection options                     |
|  [09]   | `CompiledModelCodeGenerationOptions`             | class            | `Scaffolding`; `Optimize` compiled-model output options     |
|  [10]   | `IMigrationsCodeGeneratorSelector`               | interface        | `Migrations.Design`; selects the migrations code generator  |

[PUBLIC_TYPE_SCOPE]: C# generation surfaces and bundle

| [INDEX] | [SYMBOL]                            | [TYPE_FAMILY]    | [CAPABILITY]                                                    |
| :-----: | :---------------------------------- | :--------------- | :-------------------------------------------------------------- |
|  [01]   | `ICSharpHelper` / `CSharpHelper`    | interface, class | `Design` + `Design.Internal`; C# literals/fragments             |
|  [02]   | `CSharpMigrationsGenerator`         | class            | `Migrations.Design`; full migration + designer + snapshot       |
|  [03]   | `CSharpMigrationOperationGenerator` | class            | `Migrations.Design`; emits `MigrationOperation` Up/Down code    |
|  [04]   | `CSharpSnapshotGenerator`           | class            | `Migrations.Design`; emits the model snapshot                   |
|  [05]   | `IMigrationsCodeGenerator`          | interface        | `Migrations.Design`; the language-agnostic contract             |
|  [06]   | `MigrationsCodeGenerator`           | abstract class   | `Migrations.Design`; the abstract generator base                |
|  [07]   | `CSharpDbContextGeneratorBase`      | abstract class   | `Scaffolding.Internal`; reverse-engineer context code           |
|  [08]   | `CSharpEntityTypeGeneratorBase`     | abstract class   | `Scaffolding.Internal`; reverse-engineer entity-type code       |
|  [09]   | `MigrationsBundle`                  | static class     | `Migrations.Design`; `Execute(...)` self-contained migrator EXE |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: design-service registration and operation dispatch

| [INDEX] | [SURFACE]                                                             | [SHAPE]  | [CAPABILITY]                                    |
| :-----: | :-------------------------------------------------------------------- | :------- | :---------------------------------------------- |
|  [01]   | `services.AddEntityFrameworkDesignTimeServices(reporter?, accessor?)` | static   | registers the EF design-time service graph      |
|  [02]   | `services.AddDbContextDesignTimeServices(DbContext)`                  | static   | adds a context's own design-time services       |
|  [03]   | `new DesignTimeServicesBuilder(...).Build(context)`                   | instance | builds the design `IServiceProvider` (internal) |
|  [04]   | `new OperationExecutor(IOperationReportHandler, IDictionary args)`    | ctor     | the reflection executor `dotnet ef` constructs  |
|  [05]   | `DbContextActivator.CreateInstance(contextType, ...)`                 | static   | instantiates a design-time `DbContext`          |

[ENTRYPOINT_SCOPE]: migration operations (`MigrationsOperations`, internal API)

Every row is a `MigrationsOperations` instance operation. A service deploy applies schema through `ScriptMigration` idempotent SQL or `MigrationsBundle`, never `UpdateDatabase`; `MigrationsScaffolder.ScaffoldMigration` (row [06]) is the scaffold behind `AddMigration`.

| [INDEX] | [SURFACE]                                                                                     | [CAPABILITY]                          |
| :-----: | :-------------------------------------------------------------------------------------------- | :------------------------------------ |
|  [01]   | `AddMigration(name, outputDir, contextType, namespace, dryRun)`                               | scaffold → `MigrationFiles`           |
|  [02]   | `RemoveMigration(contextType, force, dryRun)`                                                 | remove last → `MigrationFiles`        |
|  [03]   | `ScriptMigration(fromMigration, toMigration, MigrationsSqlGenerationOptions, contextType)`    | idempotent SQL between two migrations |
|  [04]   | `GetMigrations(contextType, connectionString, noConnect)`                                     | list → `IEnumerable<MigrationInfo>`   |
|  [05]   | `UpdateDatabase(targetMigration, connectionString, contextType)`                              | apply migrations to a target          |
|  [06]   | `MigrationsScaffolder.ScaffoldMigration(name, rootNamespace, subNamespace, language, dryRun)` | scaffold → `ScaffoldedMigration`      |

[ENTRYPOINT_SCOPE]: context operations (`DbContextOperations`, internal API)

`DbContextOperations.Optimize(outputDir, …, nativeAot)` generates the compiled model and precompiled queries, returning the emitted `IReadOnlyList<string>` file paths; it backs the `dotnet ef dbcontext optimize` verb through the nested `OperationExecutor.OptimizeContext`.

| [INDEX] | [SURFACE]                                                            | [SHAPE]  | [CAPABILITY]                                      |
| :-----: | :------------------------------------------------------------------- | :------- | :------------------------------------------------ |
|  [01]   | `ScaffoldContext(provider, connectionString, ...)`                   | instance | DB-first context scaffold (rejected direction)    |
|  [02]   | `HasPendingModelChanges(contextType)`                                | instance | model drifted past the last migration             |
|  [03]   | `GetContextInfo(contextType)` / `CreateContext(contextType)`         | instance | context metadata / a live design-time `DbContext` |
|  [04]   | `MigrationsBundle.Execute(context, assembly, startupAssembly, args)` | static   | the `efbundle` self-contained migrator `Main`     |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Every design-time op emits a reviewed artifact — a migration, compiled model, or idempotent SQL script — from the model as the single source of truth; store-to-model reverse engineering inverts the flow and is the named defect. Generation is provider-aware (one model emits per-provider SQL) and vocabulary-neutral.

[STACKING]:
- `api-ef-naming.md`(`.api/api-ef-naming.md`): `NameRewritingConvention` rewrites table/column/key/index identifiers at model-build time, so the `CSharp*Generator` migration DDL and snapshot carry the snake_case names as schema facts without a second naming pass.
- `Element/identity` `MigrationLaw`: composes this package as the emission and packaging substrate — `Optimize`, `ScriptMigration`, `MigrationsBundle.Execute`, and `GetMigrations` own emission, so hand-authored migration code and custom `MigrationOperation` subclasses are deleted patterns.
- `Element/identity` `Classify`: folds at generation time over the `MigrationOperation` rows the C# generators emit (`AddColumnOperation`, `RenameColumnOperation`, `AlterColumnOperation`, `DropColumnOperation`, …), splitting each change into expand and contract waves; this package supplies the operation vocabulary, migration assembly, and per-provider SQL generators, while wave classification, lock-light `NOT VALID`/`NOT ENFORCED` emission, and destructive-approval gating stay the Persistence owner's.
- `ConverterRail`: `Optimize` freezes the model into a generated compiled model that `ConverterRail.Compose(options, compiled)` mounts through `UseModel`, and the snake-case rewrites survive the freeze so compiled and fresh models emit identical column names and SQL; `CompiledModelCodeGenerationOptions` is the compiled-output policy.
- `IMigrationsCodeGeneratorSelector`: swaps the emission generator without a hand-written generator class, `CSharpMigrationsGenerator`/`CSharpMigrationOperationGenerator`/`CSharpSnapshotGenerator` the default C# arm; `ScriptMigration` under the idempotent option produces the deploy-time SQL and `MigrationsBundle.Execute` the self-contained `efbundle` migrator that runs without the SDK.

[LOCAL_ADMISSION]:
- Admit the package as a `PrivateAssets=all` asset only; the design assembly never enters a runtime dependency closure or a published output.
- Admit in-process driving of the `*.Internal` `[EntityFrameworkInternal]` drivers only with the EF minor pinned to the consumed version; the `dotnet ef` CLI / `OperationExecutor` reflection surface and the `MigrationsBundle.Execute` EXE are the stable, version-tolerant path.
- Admit reverse engineering as an implementation aid only, never a published store contract; scaffolding output is reviewed as generated shape before it enters source.

[RAIL_LAW]:
- Package: `Microsoft.EntityFrameworkCore.Design`
- Owns: EF design-time schema tooling — migration scaffolding, compiled-model (`Optimize`) generation, idempotent SQL scripting, and migration bundling
- Accept: a `PrivateAssets=all` tool asset; emission through `OperationExecutor`/CLI, `MigrationsOperations`/`DbContextOperations`, `ScriptMigration`, `Optimize`, `MigrationsBundle`, and `MigrationsCodeGeneratorSelector`
- Reject: a runtime dependency on the design assembly, DB-first reverse engineering as the store contract, hand-authored migration code, or unpinned in-process use of the `[EntityFrameworkInternal]` drivers
